# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["FinanceDashboard.Web.csproj", "."]
RUN dotnet restore "FinanceDashboard.Web.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "FinanceDashboard.Web.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "FinanceDashboard.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinanceDashboard.Web.dll"]
