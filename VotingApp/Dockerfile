# Build the Blazor WebAssembly assets
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY VotingApp/VotingApp.csproj VotingApp/
RUN dotnet restore VotingApp/VotingApp.csproj
COPY VotingApp/ VotingApp/
RUN dotnet publish VotingApp/VotingApp.csproj -c Release -o /app/publish

# Serve the published assets with nginx on port 8080
FROM nginx:1.27-alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot ./
COPY VotingApp/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]
