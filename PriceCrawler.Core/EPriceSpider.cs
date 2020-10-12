using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Parser.Formatters;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using DotnetSpider.Scheduler;
using DotnetSpider.Scheduler.Component;
using DotnetSpider.Selector;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PriceCrawler.Core
{

    public class EPriceSpider : Spider
    {
        public EPriceSpider(IOptions<SpiderOptions> options, SpiderServices services, ILogger<Spider> logger) : base(options, services, logger)
        {
        }

		public static async Task RunAsync()
		{
			var builder = Builder.CreateDefaultBuilder<EPriceSpider>(options =>
			{
				options.Speed = 1;
			});
			//builder.UseDownloader<HttpClientDownloader>();
			//builder.UseSerilog();
			//builder.IgnoreServerCertificateError();
			builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
			await builder.Build().RunAsync();
		}

		protected override async Task InitializeAsync(CancellationToken stoppingToken)
        {
			AddDataFlow(new DataParser<ProductEntry>());
			//AddDataFlow(new ConsoleStorage());
			AddDataFlow(new MyConsoleStorage());
			await AddRequestsAsync(
				new Request(
					"https://www.eprice.it/sa/?qs=cassa%2520bluetooth%2520portatile"));
		}


		[EntitySelector(Expression = ".//a[@class='ep_prodListing']", Type = SelectorType.XPath)]
		//[GlobalValueSelector(Expression = ".//a[@class='current']", Name = "类别", Type = SelectorType.XPath)]
		//[GlobalValueSelector(Expression = "//title", Name = "Title", Type = SelectorType.XPath)]
		//[FollowRequestSelector(Expressions = new[] { "//div[@class='pager']" })]
		public class ProductEntry : EntityBase<ProductEntry>
		{
			protected override void Configure()
			{
				HasIndex(x => x.ProductName);
				//HasIndex(x => new { x.WebSite, x.Guid }, true);
			}

			public int Id { get; set; }

			//[Required]
			//[StringLength(200)]
			//[ValueSelector(Expression = "类别", Type = SelectorType.Environment)]
			//public string Category { get; set; }

			//[Required]
			//[StringLength(200)]
			//[ValueSelector(Expression = "网站", Type = SelectorType.Environment)]
			//public string WebSite { get; set; }

			//[StringLength(200)]
			//[ValueSelector(Expression = "Title", Type = SelectorType.Environment)]
			//[ReplaceFormatter(NewValue = "", OldValue = " - 博客园")]
			//public string Title { get; set; }

			//[StringLength(40)]
			//[ValueSelector(Expression = "GUID", Type = SelectorType.Environment)]
			//public string Guid { get; set; }

			[ValueSelector(Expression = ".//p[@class='ep_prodName']")]
			public string ProductName { get; set; }

			[ValueSelector(Expression = ".//span[@class='ep_itemPrice']")]
			public string Price { get; set; }

			//[ValueSelector(Expression = ".//div[@class='entry_summary']")]
			//[TrimFormatter]
			//public string PlainText { get; set; }

			//[ValueSelector(Expression = "DATETIME", Type = SelectorType.Environment)]
			//public DateTime CreationTime { get; set; }
		}

		protected class MyConsoleStorage : StorageBase
		{

			protected override Task StoreAsync(DataContext context)
			{
				var typeName = typeof(ProductEntry).FullName;
				var data = context.GetData(typeName);
				if (data is ProductEntry product)
				{
					Console.WriteLine($"NAME: {product.ProductName}, PRICE: {product.Price}");
				}
				return Task.CompletedTask;
			}
		}
	}
}
