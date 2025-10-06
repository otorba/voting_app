FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["VotingResults.Server/VotingResults.Server.csproj", "VotingResults.Server/"]
COPY ["VotingResults.Client/VotingResults.Client.csproj", "VotingResults.Client/"]
COPY ["VotingResults.Shared/VotingResults.Shared.csproj", "VotingResults.Shared/"]
COPY ["Voting.Shared/Voting.Shared.csproj", "Voting.Shared/"]
COPY ["Voting.Storage/Voting.Storage.csproj", "Voting.Storage/"]
RUN dotnet restore "VotingResults.Server/VotingResults.Server.csproj"
COPY . .
WORKDIR "/src/VotingResults.Server"
RUN dotnet build "./VotingResults.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./VotingResults.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VotingResults.Server.dll"]
