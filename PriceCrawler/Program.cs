using PriceCrawler.Core;
using Serilog;
using Serilog.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PriceCrawler
{
    class Program
    {
        static async Task Main(string[] args)
        {
			ThreadPool.SetMaxThreads(255, 255);
			ThreadPool.SetMinThreads(255, 255);

			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Information()
				.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.MinimumLevel.Override("System", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Warning)
				.Enrich.FromLogContext()
				.CreateLogger();

			// // await DistributedSpider.RunAsync();
			await EPriceSpider.RunAsync();

			Console.WriteLine("Finito!");
			Console.ReadKey();
			Environment.Exit(0);
		}
    }
}
