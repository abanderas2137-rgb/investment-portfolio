using System.Collections.ObjectModel;
using InvestmentPortfolio.Models;
namespace InvestmentPortfolio
{
 public class Portfolio { public ObservableCollection<Investment> Investments { get; } = new ObservableCollection<Investment>(); }
}