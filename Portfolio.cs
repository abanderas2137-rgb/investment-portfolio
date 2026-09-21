using System.Collections.ObjectModel;
using InvestmentPortfolio.Models;
namespace InvestmentPortfolio
{
 public class Portfolio
 {
  public ObservableCollection<Investment> Investments { get; } = new ObservableCollection<Investment>();
  public ObservableCollection<PortfolioTransaction> Transactions { get; } = new ObservableCollection<PortfolioTransaction>();
  public ObservableCollection<PortfolioSnapshot> Snapshots { get; } = new ObservableCollection<PortfolioSnapshot>();
 }
}