
cd Cflashsoft.Framework.Optimization
dotnet pack  -p:NuspecFile=Cflashsoft.Framework.Optimization.nuspec --configuration Release

cd ..

cd Cflashsoft.Framework.Security
dotnet pack  -p:NuspecFile=Cflashsoft.Framework.Security.nuspec --configuration Release

cd ..

cd Cflashsoft.Framework.SecurityCore
dotnet pack  -p:NuspecFile=Cflashsoft.Framework.SecurityCore.nuspec --configuration Release

rem cd ..

rem cd Cflashsoft.Framework.SqlUtility
rem dotnet pack --configuration Release

cd ..

cd Cflashsoft.Framework.Data
dotnet pack  -p:NuspecFile=Cflashsoft.Framework.Data.nuspec --configuration Release

cd ..

cd Cflashsoft.Framework.Http
dotnet pack  -p:NuspecFile=Cflashsoft.Framework.Http.nuspec --configuration Release

rem cd ..

rem cd Cflashsoft.Framework.Web.WebUtility
rem dotnet pack --configuration Release

cd ..

cd Cflashsoft.Framework.AspNetCore.Identity
dotnet pack  -p:NuspecFile=Cflashsoft.Framework.AspNetCore.Identity.nuspec --configuration Release

cd ..

cd Cflashsoft.Framework.Logging
dotnet pack  -p:NuspecFile=Cflashsoft.Framework.Logging.nuspec --configuration Release

cd ..

cd Cflashsoft.Framework.S3
dotnet pack  -p:NuspecFile=Cflashsoft.Framework.S3.nuspec --configuration Release

cd ..

cd Cflashsoft.Framework.Redis
dotnet pack  -p:NuspecFile=Cflashsoft.Framework.Redis.nuspec --configuration Release

cd ..
