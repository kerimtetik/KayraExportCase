# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["KayraExportCase.slnx", "./"]
COPY ["Auth.API/Auth.API.csproj", "Auth.API/"]
COPY ["Auth.Domain/Auth.Domain.csproj", "Auth.Domain/"]
COPY ["Auth.Application/Auth.Application.csproj", "Auth.Application/"]
COPY ["Auth.Infrastructure/Auth.Infrastructure.csproj", "Auth.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Auth.API/Auth.API.csproj"

# Copy source code
COPY . .

# Build
RUN dotnet build "Auth.API/Auth.API.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "Auth.API/Auth.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "Auth.API.dll"]
