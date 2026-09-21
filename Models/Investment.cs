using System;
namespace InvestmentPortfolio.Models
{
 public enum InvestmentType { Stock, Bond, ETF, Deposit, Cryptocurrency, Fund, Other }
 public class Investment
 {
  public Guid Id { get; set; } = Guid.NewGuid();
  public string Name { get; set; }
  public string Ticker { get; set; }
  public InvestmentType Type { get; set; }
  public string Currency { get; set; }
  public decimal Quantity { get; set; }
  public decimal PurchasePrice { get; set; }
  public decimal CurrentPrice { get; set; }
  public DateTime PurchaseDate { get; set; }
  public DateTime? LastPriceTimestamp { get; set; }
  public string PriceSource { get; set; }
  public decimal InvestedValue => Quantity * PurchasePrice;
  public decimal CurrentValue => Quantity * CurrentPrice;
  public decimal ProfitLoss => CurrentValue - InvestedValue;
  public decimal ProfitLossPercent => InvestedValue == 0 ? 0 : ProfitLoss / InvestedValue * 100;
  public bool IsMarketPriced => Type==InvestmentType.Stock||Type==InvestmentType.Bond||Type==InvestmentType.ETF;
 }
}