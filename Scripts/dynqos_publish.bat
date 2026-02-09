cd ../
dotnet publish -p:PublishDir=.\Publish
dotnet clean
rmdir /s /q bin
rmdir /s /q obj
