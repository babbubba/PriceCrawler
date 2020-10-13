using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DotnetSpider;
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

	public class AmazonSpider : Spider
    {
		readonly Uri _baseUri = new Uri("https://www.amazon.it");
		static string _searchPattern;

		string GetSearchRelativeUrlString()
		{
			RegexOptions options = RegexOptions.None;
			Regex regex = new Regex("[ ]{2,}", options);
			var searchPattern = regex.Replace(_searchPattern, " ");
			return $"s?k={searchPattern.Trim().Replace(" ","+")}";
		}
		public AmazonSpider(IOptions<SpiderOptions> options, SpiderServices services, ILogger<Spider> logger) : base(options, services, logger)
        {
        }

		public static async Task RunAsync(string searchPattern)
		{
			if (string.IsNullOrWhiteSpace(searchPattern))
			{
				throw new ArgumentException("Il pattern di ricerca è obbigatorio", nameof(searchPattern));
			}

			_searchPattern = searchPattern;


			var builder = Builder.CreateDefaultBuilder<AmazonSpider>(options =>
			{
				options.Speed = 1;
			});

			builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
			await builder.Build().RunAsync();
		}

		protected override void ConfigureRequest(Request request)
		{
			request.SetHeader("user-agent", "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.121 Safari/537.36");
			request.SetHeader("accept-language", "it");
			request.SetHeader("accept-ecoding", "gzip, deflate, br");

			if(!request.RequestUri.IsAbsoluteUri)
			{
				request.RequestUri = new Uri(_baseUri, request.RequestUri);
			}
			base.ConfigureRequest(request);
		}

		protected override (string Id, string Name) GetIdAndName()
		{
			return (Guid.NewGuid().ToString(), "AmazonProduct");
		}

		protected override async Task InitializeAsync(CancellationToken stoppingToken)
        {
			AddDataFlow(new DataParser<AmazonProductEntry>());
			AddDataFlow(new WriteDelimitedFileStorage<AmazonProductEntry>());
			//var completeUri = new Uri(_baseUri, "s?k=cassa+bluetooth+portatile");
			var completeUri = new Uri(_baseUri,GetSearchRelativeUrlString());
			await AddRequestsAsync(new Request(completeUri));
		}


	}

	[EntitySelector(Expression = "//div[@data-component-type='s-search-result']", Type = SelectorType.XPath)]
	[FollowRequestSelector(Expressions = new[] { ".//ul[@class='a-pagination']/li[@class='a-last']" })]
	public class AmazonProductEntry : EntityBase<AmazonProductEntry>, IProductEntry
	{
		protected override void Configure()
		{
			HasIndex(x => x.ProductId, true);
		}

		public int Id { get; set; }

		[ValueSelector(Expression = "//div[@data-component-type='s-search-result']/@data-index", Type = SelectorType.XPath)]
		public string Index { get; set; }
		[ValueSelector(Expression = "//div[@data-component-type='s-search-result']/@data-uuid", Type = SelectorType.XPath)]
		public string ProductId { get; set; }


		[ValueSelector(Expression = "//span[@class='a-size-base-plus a-color-base a-text-normal']", Type = SelectorType.XPath)]
		public string ProductName { get; set; }

		[ValueSelector(Expression = "//h2[@class='a-size-mini a-spacing-none a-color-base s-line-clamp-4']/a/@href", Type = SelectorType.XPath)]
		public string ProductUrl { get; set; }

		[ValueSelector(Expression = "//span[@class='a-price-whole']")]
		public string Price { get; set; }

		[ValueSelector(Expression = "DATETIME", Type = SelectorType.Environment)]
		public DateTime CreationTime { get; set; }
	}
}
