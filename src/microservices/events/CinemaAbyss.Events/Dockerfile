FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY CinemaAbyss.Events.csproj .
RUN dotnet restore

# Copy everything else and build
COPY . .
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


