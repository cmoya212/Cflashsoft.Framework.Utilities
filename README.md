# Cfx Utilities

Cfx Utilities is a set of extensions to make some of the more verbose .NET undertakings more concise and easy to use.

## Examples

Here's an example of ADO.NET code (yeah, even with EF we should know how to perform actions directly on the database) that is normally way more verbose and unweildy. The extensions were inspired by the elegant MySQL commands for Node.JS.
```C#
//Note: Could use DbContext.Database.Connection as well in a partial class
//for your Entity Framework DbContext such as quick Deletes, SP's, or 
//any situation where EF is overkill and probably innefficient (like for
//deletes). 
using (var cn = new SqlConnection(ConnectionString))
{
    //no need to open the connection first.

    //Returns List<Dictionary<string, object>>
    var data1 = await (await cn.ExecuteQueryAsync("select Field1, Field2 from MyTable where Field1 = @Field1",
        ("Field1", "SomeValue")))
        .ToListAsync();

    //Returns List of anonymous objects or concrete class
    var data12 = await (await cn.ExecuteQueryAsync("select Field1, Field2 from MyTable where Field1 = @Field1",
        ("Field1", "SomeValue")))
        .ToListAsync((reader) => new
        {
            Field1 = reader.GetNullableString(0), //by key works too
            Field2 = reader.GetNullableInt32(1), //by key works too
        });
}
```

Here's an example of a simple, concise HttpClient GET request.
```C#
//HttpApiResult contains meta data about the call along with the result in the Value property

//Returns HttpApiResult<MyClass>
var apiResult = await httpClient.ApiAsAsync<MyClass>(HttpVerb.Get, "http://www.somendpoint.com/etc", authHeader);

//Returns a Newtonsoft JToken root object where the properties can be accessed in a dictionary hierarchy.
//Helpful when creating a concrete class for JSON is just overkill.
var apiResult2 = await httpClient.ApiAsJTokenAsync(HttpVerb.Get, "http://www.somendpoint.com/etc", authHeader);
```

## API Documentation

- **[Cfx Utilities Documentation](http://riverfront.solutions/docs/cfxutilities/index.html)**

## NuGet Packages

- **[Cflashsoft.Framework.Data](https://www.nuget.org/packages/Cflashsoft.Framework.Data/)** (.NET Standard 2.0)
- **[Cflashsoft.Framework.Http](https://www.nuget.org/packages/Cflashsoft.Framework.Http/)** (.NET Standard 2.0)
- **[Cflashsoft.Framework.Logging](https://www.nuget.org/packages/Cflashsoft.Framework.Logging/)** (.NET Standard 2.0)
- **[Cflashsoft.Framework.Optimization](https://www.nuget.org/packages/Cflashsoft.Framework.Optimization/)** (.NET Standard 2.0)
- **[Cflashsoft.Framework.Security](https://www.nuget.org/packages/Cflashsoft.Framework.Security/)** (.NET Framework 4.6.1)
- **[Cflashsoft.Framework.SecurityCore](https://www.nuget.org/packages/Cflashsoft.Framework.SecurityCore/)** (.NET Core 2.1)

