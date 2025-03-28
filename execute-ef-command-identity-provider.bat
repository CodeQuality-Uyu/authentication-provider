@echo off
set /p input= Execute ef command: 
set /p env= Enter environment (default: Local): 
if "%env%"=="" set env=Local
set /p databaseEngine= Enter Database Engine (default: Sql): 
if "%databaseEngine%"=="" set databaseEngine=Sql
cd CQ.AuthProvider.WebApi
set ASPNETCORE_ENVIRONMENT=%env%
set DatabaseEngine__Identity=%databaseEngine%
dotnet ef %input% --verbose --context IdentityDbContext -p ../CQ.AuthProvider.%databaseEngine%.Migrations
pause
