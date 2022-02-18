## Store configuration data for your .NET applications in PostgreSQL database

Storing configuration data in a central place such as PostgreSQL is ideal for distributed applicatioins or web applications hosting in a web farm environment. The package can help to achieve the objective.

To use the package, you need to create a table in your PostgreSQL database. If you want to enable "ReloadOnChange" feature, you need to create a function and a trigger. The creation script for these objects are included in postgresql.sql file.

After you create database objects, you can use it by adding below code in your application. Please refer to the test project for details.

```
IConfiguration config = new ConfigurationBuilder()
                              .AddPostgreSQLConfiguration("Host=myServer;Database=myDatabase;Username=myUserId;Password=myPassword", reloadOnChange: true)
                              .Build();
```

