FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY CinemaAbyss.Events/CinemaAbyss.Events.csproj CinemaAbyss.Events/
RUN dotnet restore CinemaAbyss.Events/CinemaAbyss.Events.csproj

# Copy everything else and build
COPY . .
WORKDIR /src/CinemaAbyss.Events
RUN dotnet publish -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8082
ENV PORT=8082

EXPOSE 8082

ENTRYPOINT ["dotnet", "CinemaAbyss.Events.dll"]

