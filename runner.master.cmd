dotnet restore
cd Bets.Web
set ASPNETCORE_ENVIRONMENT=Development
dotnet run --server.urls=http://localhost:8001/
PAUSE