
using Microsoft.Data.SqlClient;
using Cflashsoft.Framework.Data;
using Cflashsoft.Framework.Http;
using System.Net.Http.Headers;

namespace ConsoleApp1
{
    public class MyClass
    {

    }

    internal class Program
    {
        private static string ConnectionString { get; set; } = "";
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

        }

        static async Task Bla()
        {

            //Could use DbContext.Database.Connection as well in a partial class
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

            var httpClient = new HttpClient();
            var authHeader = new AuthenticationHeaderValue("");

            //HttpApiResult contains meta data about the call along with the result in the Value property

            //Returns HttpApiResult<MyClass>
            var apiResult = await httpClient.ApiAsAsync<MyClass>(HttpVerb.Get, "http://www.somendpoint.com/etc", authHeader);

            //Returns a Newtonsoft JToken root object where the properties can be accessed in a dictionary hierarchy.
            //Helpful when creating a concrete class for JSON is just overkill.
            var apiResult2 = await httpClient.ApiAsJTokenAsync(HttpVerb.Get, "http://www.somendpoint.com/etc", authHeader);


        }
    }
}