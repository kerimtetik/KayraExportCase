# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["KayraExportCase.slnx", "./"]
COPY ["Log.API/Log.API.csproj", "Log.API/"]
COPY ["Log.Domain/Log.Domain.csproj", "Log.Domain/"]
COPY ["Log.Application/Log.Application.csproj", "Log.Application/"]
COPY ["Log.Infrastructure/Log.Infrastructure.csproj", "Log.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Log.API/Log.API.csproj"

# Copy source code
COPY . .

# Build
RUN dotnet build "Log.API/Log.API.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "Log.API/Log.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "Log.API.dll"]
