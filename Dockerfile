# Use the official .NET 8.0 SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.sln ./
COPY MockECommerce.BusinessLayer/*.csproj ./MockECommerce.BusinessLayer/
COPY MockECommerce.DAL/*.csproj ./MockECommerce.DAL/
COPY MockECommerce.DtoLayer/*.csproj ./MockECommerce.DtoLayer/
COPY MockECommerce.WebAPI/*.csproj ./MockECommerce.WebAPI/
RUN dotnet restore

# Copy everything else and build
COPY . ./
WORKDIR /app/MockECommerce.WebAPI
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published app
COPY --from=build-env /app/MockECommerce.WebAPI/out .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "MockECommerce.WebAPI.dll"]
