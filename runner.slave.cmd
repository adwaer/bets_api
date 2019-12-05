cd Bets.HandlersHost
set ASPNETCORE_ENVIRONMENT=Development
dotnet run --migrate-db
dotnet run --server.urls=http://localhost:8002/
PAUSE