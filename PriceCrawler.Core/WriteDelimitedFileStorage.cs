using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Storage;

namespace PriceCrawler.Core
{
    public class WriteDelimitedFileStorage<T> : StorageBase where T : IProductEntry
	{
		protected override Task StoreAsync(DataContext context)
		{
			var data = context.GetData(typeof(T));
			if (data is ICollection<T> list)
			{
				using (var sw = new StreamWriter(@"c:\temp\prodotti.txt", true))
				{
					foreach (var entity in list)
					{

						if (entity is T product)
						{
							sw.Write(product.ProductId);
							sw.Write("|");
							sw.Write(product.Index);
							sw.Write("|");
							sw.Write(product.ProductName);
							sw.Write("|");
							sw.Write(product.Price);
							sw.Write("|");
							sw.WriteLine(product.ProductUrl);
						}
					}
					sw.Flush();
				}
			}

			return Task.CompletedTask;
		}
	}
}
