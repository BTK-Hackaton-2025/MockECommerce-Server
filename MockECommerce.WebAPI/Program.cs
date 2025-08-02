using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MockECommerce.BusinessLayer.Managers;
using MockECommerce.BusinessLayer.Services;
using MockECommerce.BusinessLayer.Utils;
using MockECommerce.DAL.Abstract;
using MockECommerce.DAL.Data;
using MockECommerce.DAL.Entities;
using MockECommerce.DAL.Repositories;
using MockECommerce.WebAPI.Middlewares;
using MockECommerce.WebAPI.Extensions;
using DotNetEnv;

// Load environment variables from .env file
Env.Load("../.env");

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;


builder.Services.AddCors(options
    =>
{
    options.AddPolicy("VercelDev", policy =>
    {
        policy.WithOrigins(
                "https://mock-e-commerce-front-co9f.vercel.app/" //
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });

});

// Build connection string from environment variables or fallback to appsettings.json
var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbUsername = Environment.GetEnvironmentVariable("DB_USERNAME");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUsername};Password={dbPassword}";

// Add services to the container.

builder.Services.AddRateLimiter(options =>
{
    // 1) GLOBAL LEVEL: Tüm isteklere 1 dakikada en fazla 500 istek
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        // Burada sabit bir partition key olarak "global" kullanıyoruz.
        return RateLimitPartition.GetSlidingWindowLimiter<string>(
            partitionKey: "global",
            factory: (string _) => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 500,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 5,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }
        );
    });



    // 2) Register: Aynı IP için 30 dakikada en fazla 5 istek
    options.AddPolicy("RegisterPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anon",
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(30),
                SegmentsPerWindow = 6,       // 6 segment x 5 dakika = 30 dakika
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }
        )
    );

    // 3) Login: Aynı IP için 5 dakikada en fazla 7 istek
    options.AddPolicy("LoginPolicy", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anon",
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 7,
                Window = TimeSpan.FromMinutes(5),
                SegmentsPerWindow = 5,       // 5 segment x 1 dakika = 5 dakika
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }
        )
    );


    // 4) Aşım durumunda 429 + özel mesaj dönelim
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        var payload = JsonSerializer.Serialize(new
        {
            message = "Çok fazla istek attınız. Lütfen biraz bekleyip tekrar deneyin."
        });
        await context.HttpContext.Response.WriteAsync(payload, cancellationToken);
    };


});

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(connectionString));

// 1) Identity’yi ekleme (ApplicationUser, ApplicationRole)
builder.Services.AddIdentity<AppUser, AppRole>(options =>
    {
        // Şifre kuralları
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;

        // Kullanıcı adı ve e-posta ayarları
        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters += " ";
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// 2) JWT ayarlarını environment variables'dan veya fallback olarak appsettings'den al
var jwtSettings = new JwtSettings
{
    Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? configuration["JwtSettings:Issuer"],
    Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? configuration["JwtSettings:Audience"],
    SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? configuration["JwtSettings:SecretKey"],
    ExpiryMinutes = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES"), out var expiry) ? expiry : configuration.GetValue<int>("JwtSettings:ExpiryMinutes")
};

// JWT ayarlarını DI container'a ekle
builder.Services.Configure<JwtSettings>(opts =>
{
    opts.Issuer = jwtSettings.Issuer;
    opts.Audience = jwtSettings.Audience;
    opts.SecretKey = jwtSettings.SecretKey;
    opts.ExpiryMinutes = jwtSettings.ExpiryMinutes;
});

// Admin user ayarlarını environment variables'dan veya fallback olarak appsettings'den al
var adminUserSettings = new AdminUserSettings
{
    FullName = Environment.GetEnvironmentVariable("ADMIN_FULLNAME") ?? configuration["AdminUser:FullName"],
    Email = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? configuration["AdminUser:Email"],
    Password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? configuration["AdminUser:Password"]
};

// Admin user ayarlarını DI container'a ekle
builder.Services.Configure<AdminUserSettings>(opts =>
{
    opts.FullName = adminUserSettings.FullName;
    opts.Email = adminUserSettings.Email;
    opts.Password = adminUserSettings.Password;
});

// 3) Authentication (JWT Bearer) yapılandırması
var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Prod’da true yapmalısınız
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            
            // Role claim mapping - this is crucial for role-based authorization
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
            NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"
        };
    });

// 4) Authorization politikalarını ekleyin (isteğe bağlı)
// Örneğin sadece “Admin” rolüne özel politika:
// Sadece "Seller" rolüne özel politika
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("RequireSellerRole", policy =>
        policy.RequireRole("Seller"));
});

// AutoMapper configuration
builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<MockECommerce.BusinessLayer.Mapping.CategoryMapping>();
    cfg.AddProfile<MockECommerce.BusinessLayer.Mapping.ProductMapping>();
    cfg.AddProfile<MockECommerce.BusinessLayer.Mapping.OrderMapping>();
});

builder.Services.AddScoped<IAuthService, AuthManager>();
builder.Services.AddScoped<IApiKeyService, ApiKeyManager>();

builder.Services.AddScoped<ICategoryService, CategoryManager>();
builder.Services.AddScoped<ICategoryDal, CategoryRepository>();

builder.Services.AddScoped<IProductService, ProductManager>();
builder.Services.AddScoped<IProductDal, ProductRepository>();

builder.Services.AddScoped<IOrderService, OrderManager>();
builder.Services.AddScoped<IOrderDal, OrderRepository>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen(options
    =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "Mock E-Commerce API",
        Description = "An ASP.NET Core Mock E-Commerce API",
    });

    // 🛡️ JWT için Swagger'a "Bearer" şeması ekle
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    });

    // 🔑 API Key için Swagger'a "ApiKey" şeması ekle
    options.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "X-API-Key",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "API Key for external endpoints. Enter your seller API key here."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        },
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("VercelDev");
app.UseMiddleware<ExceptionHandlingMiddleware>();

// app.UseHttpsRedirection();
app.UseRateLimiter();

app.UseAuthentication();
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
app.UseAuthorization();

app.MapControllers();

// Seed the database
if (app.Environment.IsDevelopment())
{
    await app.Services.SeedDatabaseAsync();
}

app.Run();
