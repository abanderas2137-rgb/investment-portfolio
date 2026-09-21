using System;
namespace InvestmentPortfolio.Models
{
 public class PortfolioSnapshot
 {
  public DateTime Timestamp { get; set; }
  public decimal ValuePln { get; set; }
  public decimal NetDepositsPln { get; set; }
 }
}