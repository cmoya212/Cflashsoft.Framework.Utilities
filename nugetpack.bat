rem cd Cflashsoft.Framework.Optimization
rem nuget pack -prop Configuration=Release

cd Cflashsoft.Framework.Security
nuget pack -prop Configuration=Release

cd ..

cd Cflashsoft.Framework.SecurityCore
nuget pack -prop Configuration=Release

cd ..

cd Cflashsoft.Framework.SqlUtility
nuget pack -prop Configuration=Release

cd ..

cd Cflashsoft.Framework.Http
nuget pack -prop Configuration=Release

cd ..

cd Cflashsoft.Framework.Web.WebUtility
nuget pack -prop Configuration=Release


cd Cflashsoft.Framework.AspNetCore.Identity
nuget pack -prop Configuration=Release

cd ..
