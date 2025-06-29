# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /PrescriptoAI

# Copy project file and restore dependencies
COPY PrescriptoAI.csproj ./
RUN dotnet restore

# Copy remaining files and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /PrescriptoAI

# Copy published files
COPY --from=build /app/publish .

# Create directories for wwwroot/Uploads and AIModels
RUN mkdir -p /PrescriptoAI/wwwroot/Uploads /PrescriptoAI/AIModels
RUN chown -R 1000:1000 /PrescriptoAI/wwwroot/Uploads /PrescriptoAI/AIModels

# Set environment
ENV ASPNETCORE_URLS=http://+:8080

# Expose port
EXPOSE 8080

# Run as non-root user for security
USER 1000

ENTRYPOINT ["dotnet", "PrescriptoAI.dll"]