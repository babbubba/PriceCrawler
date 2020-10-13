using System;

namespace PriceCrawler.Core
{
    public interface IProductEntry
    {
        DateTime CreationTime { get; set; }
        int Id { get; set; }
        string Index { get; set; }
        string Price { get; set; }
        string ProductId { get; set; }
        string ProductName { get; set; }
        string ProductUrl { get; set; }
    }
}