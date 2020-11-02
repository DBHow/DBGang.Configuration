using System;
using Microsoft.Extensions.Configuration;

namespace TestSecuredJson
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                                        .SetBasePath(Environment.CurrentDirectory)
                                        .AddSecuredJsonFile("sample.json")
                                        // Optionally you can provide your own Passphrase to encrypt the configuration data.
                                        //.AddSecuredJsonFile("sample.json", "myPassphrase") 
                                        .Build();

            Console.WriteLine("---------- simple object ---------");
            Console.WriteLine($"StringKey1={config["StringKey1"]}");
            Console.WriteLine($"IntegerKey2={config["IntegerKey2"]}");
            Console.WriteLine($"BooleanKey3={config["BooleanKey3"]}");
            Console.WriteLine($"ConnectionString [MyDB]={config.GetConnectionString("MyDB")}");
            Console.WriteLine();

            Console.WriteLine("---------- array object ---------");
            var array1 = config.GetSection("ArrayKey").GetChildren();
            foreach (var item in array1)
            {
                Console.WriteLine($"{item.Path}={item.Value}");
            }
            Console.WriteLine();

            Console.WriteLine("---------- complex object ---------");
            Console.WriteLine($"ObjKey:SubStringKey1={config["ObjKey:SubStringKey1"]}");
            Console.WriteLine($"ObjKey:SubIntegerKey2={config["ObjKey:SubIntegerKey2"]}");
            Console.WriteLine($"ObjKey:SubBooleanKey3={config["ObjKey:SubBooleanKey3"]}");
            var array2 = config.GetSection("ObjKey:SubArray1").GetChildren();
            foreach (var item in array2)
            {
                Console.WriteLine($"{item.Path}={item.Value}");
            }

        }
    }
}
