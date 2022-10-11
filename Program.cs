using BenchmarkDotNet.Running;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.IO;

namespace OptimizeMePlease
{

    /// <summary>
    /// Steps: 
    /// 
    /// 1. Create a database with name "OptimizeMePlease"
    /// 2. Run application Debug/Release mode for the first time. IWillPopulateData method will get the script and populate
    /// created db.
    /// 3. Comment or delete IWillPopulateData() call from Main method. 
    /// 4. Go to BenchmarkService.cs class
    /// 5. Start coding within GetAuthors_Optimized method
    /// GOOD LUCK! :D 
    /// </summary>
    public static class Program
    {
        private static void Main(string[] args)
        {
            //Debugging 
            var benchmarkService = new BenchmarkService();
            benchmarkService.GetAuthors_Optimized();

            //Comment me after first execution, please.
            //WillPopulateData();

            BenchmarkRunner.Run<BenchmarkService>();
        }

        private static void WillPopulateData()
        {
            const string sqlConnectionString = @"Server=.;Database=OptimizeMePlease;Trusted_Connection=True;Integrated Security=true;MultipleActiveResultSets=true";

            var workingDirectory = Environment.CurrentDirectory;
            var path = Path.Combine(Directory.GetParent(workingDirectory)!.Parent!.Parent!.FullName, @"script.sql");
            var script = File.ReadAllText(path);

            var conn = new SqlConnection(sqlConnectionString);

            var server = new Server(new ServerConnection(conn));

            server.ConnectionContext.ExecuteNonQuery(script);
        }
    }
}
