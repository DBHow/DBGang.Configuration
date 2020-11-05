using System;
using Microsoft.Extensions.Configuration;
using DBGang.Configuration.PostgreSQL;

namespace TestPostgreSQL
{
    class Program
    {

        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                                        .AddPostgreSQLConfiguration("Host=myServer;Database=myDatabase;Username=myUserId;Password=myPassword", reloadOnChange: true)
                                        .Build();
            
            while (true)
            {
                Console.WriteLine("----------- print -------------");
                foreach (var item in config.GetChildren())
                {
                    Console.WriteLine($"{item.Key}: {item.Value}");
                }
                Console.WriteLine();
                System.Threading.Thread.Sleep(10000);
            }

        }

    }
}
