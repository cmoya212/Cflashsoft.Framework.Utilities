
cd Cflashsoft.Framework.Optimization
nuget pack -prop Configuration=Release

cd ..

cd Cflashsoft.Framework.Security
nuget pack -prop Configuration=Release

cd ..

cd Cflashsoft.Framework.SecurityCore
nuget pack -prop Configuration=Release

rem cd ..

rem cd Cflashsoft.Framework.SqlUtility
rem nuget pack -prop Configuration=Release

cd ..

cd Cflashsoft.Framework.Data
nuget pack -prop Configuration=Release

cd ..

cd Cflashsoft.Framework.Http
nuget pack -prop Configuration=Release

rem cd ..

rem cd Cflashsoft.Framework.Web.WebUtility
rem nuget pack -prop Configuration=Release

cd ..

cd Cflashsoft.Framework.AspNetCore.Identity
nuget pack -prop Configuration=Release

cd ..

cd Cflashsoft.Framework.Logging
nuget pack -prop Configuration=Release

cd ..

cd Cflashsoft.Framework.S3
nuget pack -prop Configuration=Release

cd ..
