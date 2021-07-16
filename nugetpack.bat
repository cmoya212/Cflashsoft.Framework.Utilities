
cd Cflashsoft.Framework.Optimization
nuget pack -prop Configuration=Release

cd ..

cd Cflashsoft.Framework.Security
nuget pack -prop Configuration=Release

cd ..

cd Cflashsoft.Framework.SecurityCore
nuget pack -prop Configuration=Release

cd ..

cd Cflashsoft.Framework.SqlUtility
nuget pack -prop Configuration=Release

cd ..

cd Cflashsoft.Framework.Data
nuget pack -prop Configuration=Release

cd ..

cd Cflashsoft.Framework.Http
nuget pack -prop Configuration=Release

cd ..

cd Cflashsoft.Framework.Web.WebUtility
nuget pack -prop Configuration=Release

cd ..

cd Cflashsoft.Framework.AspNetCore.Identity
nuget pack -prop Configuration=Release

cd ..
