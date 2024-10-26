# Cfx Utilities

Cfx Utilities is a set of extensions to make some of the more verbose .NET undertakings more concise and easy to use.

## Examples

For instance, simplified ADO.NET data access similar to Dapper but even simpler:
```C#
await using (var cn = new SqlConnection(ConnectionString))
{
    //no need to open the connection first.

    //Returns List<Dictionary<string, object>>
    var apples = await cn.ExecuteToListAsync("select FruitName, FruitKind, Notes from Fruits where FruitKind = @FruitKind",
        ("FruitKind", "Apple"));

    foreach (var apple in apples)
        Console.WriteLine(apple["FruitName"]);

    //Returns List of anonymous objects or concrete class
    var oranges = await cn.ExecuteToListAsync("select FruitName, FruitKind, Notes from Fruits where FruitKind = @FruitKind",
        reader => new
        {
            Name = reader.GetString(0), //by key works too
            Kind = reader.GetString(1), //by key works too
            Notes = reader.GetNullableString(2) //IsDBNull rigmarole, by key works too
        },
        ("FruitKind", "Orange"));

    foreach (var orange in oranges)
        Console.WriteLine(orange.Name);

    //Even return a tuple
    var delicious = await cn.ExecuteNullableFirstOrDefaultAsync("select FruitName, FruitKind, Notes from Fruits where FruitKind = @FruitKind and FruitName = @FruitName",
        reader =>
        (
            Name: reader.GetString(0), //by key works too
            Kind: reader.GetString(1), //by key works too
            Notes: reader.GetNullableString(2) //IsDBNull rigmarole, by key works too
        ),
        ("FruitKind", "Apple"),
        ("FruitName", "Delicious"));

    if (delicious.HasValue)
        Console.WriteLine(delicious.Value.Name);
}
```
And using a DbContext partial to delete a record without pulling it in first:
```C#
public partial class MyModel : DbContext
{
    public async Task DeleteSomeEntity(int entityId)
    {
        await this.Database.GetDbConnection()
            .ExecuteNonQueryAsync("delete from SomeTable where Id = @EntityId",
                ("EntityId", entityId));
	}
}
```

Here's an example of a simple, concise HttpClient GET request.
```C#
//HttpApiResult contains meta data about the call along with the result in the Value property

//Returns HttpApiResult<MyClass>
var apiResult = await httpClient.ApiAsAsync<MyClass>(HttpVerb.Get, "http://www.somendpoint.com/etc", authHeader);

if (apiResult.IsSuccessStatusCode)
    Console.WriteLine(apiResult.Value.SomeValue);

//Returns a Newtonsoft JToken root object where the properties can be accessed in a dictionary hierarchy.
//Helpful when creating a concrete class for JSON is just overkill.
var apiResult2 = await httpClient.ApiAsJTokenAsync(HttpVerb.Get, "http://www.somendpoint.com/etc", authHeader);

if (apiResult.IsSuccessStatusCode)
    Console.WriteLine((string)apiResult.Value["SomeValue"]);
```

Also included is a "HybridCache" that implements an L1-L2 memorycache + remotecache (Redis) strategy with configurable item expiration that only now seems to be making its way into .NET in .NET 9 in 2024 but has existed here since 2017.
```C#
var item = await hybridCache.InterlockedGetOrSetAsync(
    key, 
    async () => 
    { 
        //this would normally be a database call
        return new MyClass { MyProperty1 = "Property 1", MyProperty2 = "Property 2!" }; 
    },
    useMemoryCache: true,
    useRemoteCache: true,
    memoryItemExpirationSeconds: 30,
    remoteItemExpirationSeconds: 300,
    monitorRemoteItem: true);
//all parameters like useMemoryCache, etc. are optional and defaults can be set at the HybridCache level and omitted here.
```

## API Documentation

- **[Cfx Utilities Documentation](http://riverfront.solutions/docs/cfxutilities/index.html)**

## NuGet Packages

- **[Cflashsoft.Framework.Data](https://www.nuget.org/packages/Cflashsoft.Framework.Data/)** (.NET Standard 2.0)
- **[Cflashsoft.Framework.Http](https://www.nuget.org/packages/Cflashsoft.Framework.Http/)** (.NET Standard 2.0)
- **[Cflashsoft.Framework.Logging](https://www.nuget.org/packages/Cflashsoft.Framework.Logging/)** (.NET Standard 2.0)
- **[Cflashsoft.Framework.Optimization](https://www.nuget.org/packages/Cflashsoft.Framework.Optimization/)** (.NET Standard 2.0)
- **[Cflashsoft.Framework.Redis](https://www.nuget.org/packages/Cflashsoft.Framework.Redis/)** (.NET Standard 2.1)
- **[Cflashsoft.Framework.S3](https://www.nuget.org/packages/Cflashsoft.Framework.S3/)** (.NET Standard 2.0)
- **[Cflashsoft.Framework.Security](https://www.nuget.org/packages/Cflashsoft.Framework.Security/)** (.NET Framework 4.6.1)
- **[Cflashsoft.Framework.SecurityCore](https://www.nuget.org/packages/Cflashsoft.Framework.SecurityCore/)** (.NET Core 2.1)

