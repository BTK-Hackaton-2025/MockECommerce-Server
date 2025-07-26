using System.Text;
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

// Build connection string from environment variables or fallback to appsettings.json
var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbUsername = Environment.GetEnvironmentVariable("DB_USERNAME");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUsername};Password={dbPassword}";

// Add services to the container.

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
});

builder.Services.AddScoped<IAuthService, AuthManager>();
builder.Services.AddScoped<IApiKeyService, ApiKeyManager>();

builder.Services.AddScoped<ICategoryService, CategoryManager>();
builder.Services.AddScoped<ICategoryDal, CategoryRepository>();

builder.Services.AddScoped<IProductService, ProductManager>();
builder.Services.AddScoped<IProductDal, ProductRepository>();

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
app.UseMiddleware<ExceptionHandlingMiddleware>();

// app.UseHttpsRedirection();

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
