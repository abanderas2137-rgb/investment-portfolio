using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using InvestmentPortfolio.Models;
using InvestmentPortfolio.Services;
namespace InvestmentPortfolio
{
 public partial class MainWindow : Window
 {
  private readonly Portfolio _portfolio=new Portfolio(); private readonly PortfolioStorage _storage;
  public MainWindow()
  {
   InitializeComponent(); _storage=new PortfolioStorage(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"InvestmentPortfolio","portfolio.xml"));
   foreach(var x in _storage.Load()) _portfolio.Investments.Add(x); Refresh();
  }
  private void Refresh()
  {
   InvestmentsGrid.ItemsSource=null; InvestmentsGrid.ItemsSource=_portfolio.Investments.ToList();
   TotalCurrentText.Text=_portfolio.Investments.Sum(x=>x.CurrentValue).ToString("N2"); TotalInvestedText.Text=_portfolio.Investments.Sum(x=>x.InvestedValue).ToString("N2"); TotalProfitText.Text=_portfolio.Investments.Sum(x=>x.ProfitLoss).ToString("N2"); CountText.Text=_portfolio.Investments.Count.ToString();
   SummaryGrid.ItemsSource=_portfolio.Investments.GroupBy(x=>x.Type).Select(g=>new SummaryRow{Type=g.Key.ToString(),Count=g.Count(),Invested=g.Sum(x=>x.InvestedValue),Current=g.Sum(x=>x.CurrentValue),Profit=g.Sum(x=>x.ProfitLoss)}).ToList();
   CurrencyGrid.ItemsSource=_portfolio.Investments.GroupBy(x=>x.Currency).Select(g=>new SummaryRow{Currency=g.Key,Count=g.Count(),Invested=g.Sum(x=>x.InvestedValue),Current=g.Sum(x=>x.CurrentValue),Profit=g.Sum(x=>x.ProfitLoss)}).ToList();
  }
  private void Add_Click(object sender,RoutedEventArgs e){var d=new InvestmentDialog{Owner=this};if(d.ShowDialog()==true){_portfolio.Investments.Add(d.Investment);SaveAndRefresh();}}
  private void Edit_DoubleClick(object sender,MouseButtonEventArgs e){var x=InvestmentsGrid.SelectedItem as Investment;if(x==null)return;var d=new InvestmentDialog(x){Owner=this};if(d.ShowDialog()==true)SaveAndRefresh();}
  private void Delete_Click(object sender,RoutedEventArgs e){var x=InvestmentsGrid.SelectedItem as Investment;if(x==null)return;if(MessageBox.Show("Usunąć zaznaczoną inwestycję?","Potwierdzenie",MessageBoxButton.YesNo,MessageBoxImage.Question)==MessageBoxResult.Yes){_portfolio.Investments.Remove(x);SaveAndRefresh();}}
  private void SaveAndRefresh(){_storage.Save(_portfolio.Investments);Refresh();}
  public class SummaryRow { public string Type{get;set;} public string Currency{get;set;} public int Count{get;set;} public decimal Invested{get;set;} public decimal Current{get;set;} public decimal Profit{get;set;} public decimal ProfitPercent=>Invested==0?0:Profit/Invested*100; }
 }
}