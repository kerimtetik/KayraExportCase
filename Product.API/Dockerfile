# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["KayraExportCase.slnx", "./"]
COPY ["Product.API/Product.API.csproj", "Product.API/"]
COPY ["Product.Domain/Product.Domain.csproj", "Product.Domain/"]
COPY ["Product.Application/Product.Application.csproj", "Product.Application/"]
COPY ["Product.Infrastructure/Product.Infrastructure.csproj", "Product.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Product.API/Product.API.csproj"

# Copy source code
COPY . .

# Build
RUN dotnet build "Product.API/Product.API.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "Product.API/Product.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "Product.API.dll"]
