
dotnet tool install --global dotnet-ef

dotnet ef migrations add InitialVincallContext -c VincallDBContext -o Data/Migrations/VincallDb

dotnet ef database update --context VincallDBContext