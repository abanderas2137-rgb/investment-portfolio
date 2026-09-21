using System;
namespace InvestmentPortfolio.Models
{
 public class PortfolioTransaction
 {
  public DateTime Date { get; set; }
  public string Account { get; set; }
  public string Ticker { get; set; }
  public string Currency { get; set; }
  public string Name { get; set; }
  public InvestmentType Type { get; set; }
  public string TransactionType { get; set; }
  public decimal Quantity { get; set; }
  public decimal Price { get; set; }
  public decimal Fees { get; set; }
  public decimal TransactionFxRate { get; set; }
  public decimal NominalPrice { get; set; }
  public decimal TotalPln { get; set; }
  public string Comment { get; set; }
 } 
}