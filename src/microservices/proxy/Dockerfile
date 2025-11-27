FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY src/CinemaAbyss.Proxy/CinemaAbyss.Proxy.csproj src/CinemaAbyss.Proxy/
RUN dotnet restore src/CinemaAbyss.Proxy/CinemaAbyss.Proxy.csproj

# Copy everything else and build
COPY . .
WORKDIR /src/src/CinemaAbyss.Proxy
RUN dotnet publish -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Copy configuration files
COPY --from=build /src/src/CinemaAbyss.Proxy/Configurations ./Configurations/

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV PORT=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "CinemaAbyss.Proxy.dll"]

