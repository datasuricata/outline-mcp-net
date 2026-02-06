FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Outline.Mcp.Shared/Outline.Mcp.Shared.csproj", "Outline.Mcp.Shared/"]
COPY ["src/Outline.Mcp.Server/Outline.Mcp.Server.csproj", "Outline.Mcp.Server/"]

# Restore dependencies
RUN dotnet restore "Outline.Mcp.Server/Outline.Mcp.Server.csproj"

# Copy source code
COPY ["src/Outline.Mcp.Shared/", "Outline.Mcp.Shared/"]
COPY ["src/Outline.Mcp.Server/", "Outline.Mcp.Server/"]

# Copy skills directory for embedded resources
COPY ["skills/", "/skills/"]

# Build and publish
RUN dotnet build "Outline.Mcp.Server/Outline.Mcp.Server.csproj" -c Release -o /app/build
RUN dotnet publish "Outline.Mcp.Server/Outline.Mcp.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Skills are now embedded resources - no file copies needed

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Expose SSE port
EXPOSE 8080

# Set default SSE port
ENV MCP_SSE_PORT=8080

ENTRYPOINT ["dotnet", "Outline.Mcp.Server.dll"]
