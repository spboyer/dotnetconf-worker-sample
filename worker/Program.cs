using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.WindowsAzure.Storage;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace worker
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            var logStorageAccount = CloudStorageAccount.Parse(Configuration.GetConnectionString("Storage"));

            Log.Logger = new LoggerConfiguration()
                //.WriteTo.Console()
                .WriteTo.AzureTableStorage(logStorageAccount, LogEventLevel.Verbose, null, "helloworker", false, null, null, null)
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .CreateLogger();

            try
            {
                Log.Information("Starting hello-worker");
                CreateHostBuilder(args).Build().Run();
                return;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Problem starting hello-worker");
                return;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                  .ConfigureServices((hostContext, services) =>
                  {
                      services.AddHostedService<Worker>();
                  })
                  .UseSerilog();
        }
    }
}
