# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ApiPortfolioProject.csproj", "./"]
RUN dotnet restore "./ApiPortfolioProject.csproj"
COPY . .
RUN dotnet publish "ApiPortfolioProject.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ApiPortfolioProject.dll"]