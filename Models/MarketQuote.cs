using System;
namespace InvestmentPortfolio.Models
{
 public class MarketQuote
 {
  public string Symbol { get; set; }
  public decimal Price { get; set; }
  public DateTime Timestamp { get; set; }
  public string Source { get; set; }
 }
}