cd Bets.ParserHost
set ASPNETCORE_ENVIRONMENT=Development
dotnet run --migrate-db
dotnet run --server.urls=http://localhost:8003/
PAUSE