# Build the combined server + WebAssembly app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY VotingApp.Server/VotingApp.Server.csproj VotingApp.Server/
COPY VotingApp/VotingApp.csproj VotingApp/
COPY VotingApp.Shared/VotingApp.Shared.csproj VotingApp.Shared/
RUN dotnet restore VotingApp.Server/VotingApp.Server.csproj

COPY VotingApp.Server/ VotingApp.Server/
COPY VotingApp/ VotingApp/
COPY VotingApp.Shared/ VotingApp.Shared/
RUN dotnet publish VotingApp.Server/VotingApp.Server.csproj -c Release -o /app/publish /p:UseAppHost=false

# Run the ASP.NET Core server hosting the Blazor client and API
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "VotingApp.Server.dll"]
