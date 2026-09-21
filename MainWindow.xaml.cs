using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using InvestmentPortfolio.Models;
using InvestmentPortfolio.Services;

namespace InvestmentPortfolio
{
 public partial class MainWindow : Window
 {
  private readonly Portfolio _portfolio=new Portfolio();
  private readonly PortfolioStorage _storage;
  private readonly TransactionStorage _transactionStorage;
  private readonly SnapshotStorage _snapshotStorage;
  private readonly MarketDataService _market=new MarketDataService();
  private readonly MarketDataCache _cache;
  private readonly PortfolioAnalytics _analytics=new PortfolioAnalytics();
  private readonly Dictionary<string,decimal> _fx=new Dictionary<string,decimal>(StringComparer.OrdinalIgnoreCase){{"PLN",1m}};

  public MainWindow()
  {
   InitializeComponent();
   var dir=System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"InvestmentPortfolio");
   _storage=new PortfolioStorage(System.IO.Path.Combine(dir,"portfolio.xml"));
   _transactionStorage=new TransactionStorage(System.IO.Path.Combine(dir,"transactions.xml"));
   _snapshotStorage=new SnapshotStorage(System.IO.Path.Combine(dir,"snapshots.xml"));
   _cache=new MarketDataCache(System.IO.Path.Combine(dir,"market-cache.xml"));
   foreach(var x in _storage.Load())_portfolio.Investments.Add(x);
   foreach(var x in _transactionStorage.Load())_portfolio.Transactions.Add(x);
   foreach(var x in _snapshotStorage.Load())_portfolio.Snapshots.Add(x);
   Refresh();
   Loaded+=async (s,e)=>await RefreshMarketDataAsync();
  }

  private async Task RefreshMarketDataAsync()
  {
   bool online=true;DateTime? latestCache=null;string lastMarketError=null;
   foreach(var inv in _portfolio.Investments.Where(x=>x.IsMarketPriced&&!string.IsNullOrWhiteSpace(x.Ticker)))
   {
    try{var q=await _market.GetQuoteAsync(inv.Ticker);inv.CurrentPrice=q.Price;inv.LastPriceTimestamp=q.Timestamp;inv.PriceSource=q.Source;_cache.Put(q);}
    catch(Exception ex){online=false;lastMarketError=ex.Message;var q=_cache.Get(inv.Ticker);if(q!=null){inv.CurrentPrice=q.Price;inv.LastPriceTimestamp=q.Timestamp;inv.PriceSource=q.Source;latestCache=latestCache.HasValue&&latestCache.Value>q.Timestamp?latestCache:q.Timestamp;}}
   }
   foreach(var c in _portfolio.Investments.Select(x=>x.Currency).Distinct(StringComparer.OrdinalIgnoreCase))
   {
    if(string.Equals(c,"PLN",StringComparison.OrdinalIgnoreCase))continue;
    try{var q=await _market.GetQuoteAsync(c+"PLN=X");_fx[c]=q.Price;_cache.Put(q);}
    catch(Exception ex){online=false;lastMarketError=ex.Message;var q=_cache.Get(c+"PLN=X");if(q!=null){_fx[c]=q.Price;latestCache=latestCache.HasValue&&latestCache.Value>q.Timestamp?latestCache:q.Timestamp;}}
   }
   _storage.Save(_portfolio.Investments);
   var value=CurrentValuePln();_portfolio.Snapshots.Add(new PortfolioSnapshot{Timestamp=DateTime.Now,ValuePln=value,NetDepositsPln=_analytics.NetDeposits(_portfolio.Transactions)});
   while(_portfolio.Snapshots.Count>2000)_portfolio.Snapshots.RemoveAt(0);
   _snapshotStorage.Save(_portfolio.Snapshots);
   if(!online){if(!latestCache.HasValue)latestCache=_portfolio.Investments.Where(x=>x.LastPriceTimestamp.HasValue).Select(x=>x.LastPriceTimestamp.Value).OrderByDescending(x=>x).FirstOrDefault();ConnectionText.Text=latestCache.HasValue&&latestCache.Value!=default(DateTime)?"Brak aktualnych danych z internetu. Ostatnie dane z "+latestCache.Value.ToString("dd.MM.yyyy HH:mm"):(string.IsNullOrWhiteSpace(lastMarketError)?"Brak danych rynkowych.":"Błąd pobierania danych: "+lastMarketError);ConnectionBanner.Visibility=Visibility.Visible;}
   else ConnectionBanner.Visibility=Visibility.Collapsed;
   Refresh();
  }

  private decimal Fx(string c)=>_fx.ContainsKey(c)?_fx[c]:1m;
  private decimal CurrentValuePln()=>_portfolio.Investments.Sum(x=>x.CurrentValue*Fx(x.Currency));
  private decimal InvestedValuePln()=>_portfolio.Investments.Sum(x=>x.InvestedValue*Fx(x.Currency));

  private void Refresh()
  {
   InvestmentsGrid.ItemsSource=null;InvestmentsGrid.ItemsSource=_portfolio.Investments.ToList();
   var current=CurrentValuePln();var deposits=_analytics.NetDeposits(_portfolio.Transactions);var invested=_portfolio.Transactions.Count>0?deposits:InvestedValuePln();
   TotalCurrentText.Text=current.ToString("N2");TotalInvestedText.Text=invested.ToString("N2");TotalProfitText.Text=(current-invested).ToString("N2");
   var xirr=_analytics.Xirr(_portfolio.Transactions,current,DateTime.Now);XirrText.Text=xirr==0?"—":(xirr*100).ToString("N2")+" %";
   SummaryGrid.ItemsSource=_portfolio.Investments.GroupBy(x=>x.Type).Select(g=>new SummaryRow{Type=g.Key.ToString(),Count=g.Count(),Invested=g.Sum(x=>x.InvestedValue),Current=g.Sum(x=>x.CurrentValue),Profit=g.Sum(x=>x.ProfitLoss)}).ToList();
   CurrencyGrid.ItemsSource=_portfolio.Investments.GroupBy(x=>x.Currency).Select(g=>new SummaryRow{Currency=g.Key,Count=g.Count(),Current=g.Sum(x=>x.CurrentValue),CurrentPln=g.Sum(x=>x.CurrentValue*Fx(x.Currency))}).ToList();
   DrawCharts(xirr);
  }

  private async void RefreshMarket_Click(object sender,RoutedEventArgs e){await RefreshMarketDataAsync();}
  private void Add_Click(object sender,RoutedEventArgs e){var d=new InvestmentDialog{Owner=this};if(d.ShowDialog()==true){_portfolio.Investments.Add(d.Investment);_portfolio.Transactions.Add(new PortfolioTransaction{Date=d.Investment.PurchaseDate,Ticker=d.Investment.Ticker,Currency=d.Investment.Currency,Name=d.Investment.Name,Type=d.Investment.Type,TransactionType="Kupno",Quantity=d.Investment.Quantity,Price=d.Investment.PurchasePrice,TotalPln=d.Investment.Currency=="PLN"?d.Investment.InvestedValue:0});SaveAndRefresh();}}
  private void Edit_DoubleClick(object sender,MouseButtonEventArgs e){var x=InvestmentsGrid.SelectedItem as Investment;if(x==null)return;var d=new InvestmentDialog(x){Owner=this};if(d.ShowDialog()==true)SaveAndRefresh();}
  private void Delete_Click(object sender,RoutedEventArgs e){var x=InvestmentsGrid.SelectedItem as Investment;if(x==null)return;if(MessageBox.Show("Usunąć zaznaczoną inwestycję?","Potwierdzenie",MessageBoxButton.YesNo,MessageBoxImage.Question)==MessageBoxResult.Yes){_portfolio.Investments.Remove(x);SaveAndRefresh();}}
  private void SaveAndRefresh(){_storage.Save(_portfolio.Investments);_transactionStorage.Save(_portfolio.Transactions);Refresh();}

  private void ImportCsv_Click(object sender,RoutedEventArgs e)
  {
   var d=new OpenFileDialog{Filter="CSV (*.csv)|*.csv|Wszystkie pliki (*.*)|*.*"};if(d.ShowDialog()!=true)return;
   try{var rows=new CsvPortfolioImporter().Import(d.FileName);foreach(var row in rows){_portfolio.Transactions.Add(row);ApplyTransaction(row);} _transactionStorage.Save(_portfolio.Transactions);_storage.Save(_portfolio.Investments);Refresh();MessageBox.Show("Zaimportowano "+rows.Count+" transakcji.","Import CSV",MessageBoxButton.OK,MessageBoxImage.Information);}
   catch(Exception ex){MessageBox.Show(ex.Message,"Błąd importu CSV",MessageBoxButton.OK,MessageBoxImage.Error);}
  }

  private void ApplyTransaction(PortfolioTransaction t)
  {
   var s=(t.TransactionType??"").ToLowerInvariant();if(t.Type==InvestmentType.Deposit||s.Contains("wpłat")||s.Contains("wplat")||s.Contains("deposit")||s.Contains("wypłat")||s.Contains("wyplat")||s.Contains("withdraw")||s.Contains("dywid"))return;
   if(string.IsNullOrWhiteSpace(t.Ticker))return;
   var inv=_portfolio.Investments.FirstOrDefault(x=>string.Equals(x.Ticker,t.Ticker,StringComparison.OrdinalIgnoreCase)&&string.Equals(x.Currency,t.Currency,StringComparison.OrdinalIgnoreCase));
   bool buy=s.Contains("kup")||s.Contains("buy")||s.Contains("naby");bool sell=s.Contains("sprzed")||s.Contains("sell");
   if(inv==null&&buy){inv=new Investment{Name=t.Name,Ticker=t.Ticker,Type=t.Type,Currency=t.Currency,PurchaseDate=t.Date,PurchasePrice=t.Price,CurrentPrice=t.Price};_portfolio.Investments.Add(inv);}
   if(inv==null)return;
   if(buy){var total=inv.Quantity+t.Quantity;inv.PurchasePrice=total==0?inv.PurchasePrice:((inv.Quantity*inv.PurchasePrice)+(t.Quantity*t.Price))/total;inv.Quantity=total;}
   else if(sell)inv.Quantity=Math.Max(0,inv.Quantity-t.Quantity);
  }

  private void DrawCharts(decimal xirr)
  {
   DrawBars(CurrencyChart,_portfolio.Investments.GroupBy(x=>x.Currency).Select(g=>new KeyValuePair<string,decimal>(g.Key,g.Sum(x=>x.CurrentValue*Fx(x.Currency)))).ToList(),true);
   DrawBars(DividendChart,_analytics.DividendSeries(_portfolio.Transactions).Select(x=>new KeyValuePair<string,decimal>(x.Date.ToString("MM.yyyy"),x.Value)).ToList(),false);
   var account=_portfolio.Snapshots.OrderBy(x=>x.Timestamp).Select(x=>new PortfolioAnalytics.Point{Date=x.Timestamp,Value=x.ValuePln}).ToList();
   var deposits=_portfolio.Snapshots.OrderBy(x=>x.Timestamp).Select(x=>new PortfolioAnalytics.Point{Date=x.Timestamp,Value=x.NetDepositsPln}).ToList();
   DrawTwoLines(AccountChart,account,deposits);
   DrawBars(ProfitChart,_portfolio.Investments.GroupBy(x=>x.Type).Select(g=>new KeyValuePair<string,decimal>(g.Key.ToString(),g.Sum(x=>x.ProfitLoss*Fx(x.Currency)))).ToList(),false);
   DrawLine(DrawdownChart,_analytics.DrawdownSeries(_portfolio.Snapshots));
   var monthly=_analytics.AverageMonthlyDeposit(_portfolio.Transactions);ForecastTitle.Text="Prognoza na 5 lat • XIRR "+(xirr*100).ToString("N2")+"% • średnie wpłaty "+monthly.ToString("N0")+" PLN/mies.";DrawLine(ForecastChart,_analytics.Forecast(CurrentValuePln(),xirr,monthly,60));
  }

  private void DrawBars(Canvas c,IList<KeyValuePair<string,decimal>> data,bool percent){c.Children.Clear();if(data.Count==0)return;double w=Math.Max(400,c.ActualWidth),h=c.ActualHeight;decimal max=data.Max(x=>x.Value);if(max<=0)return;double bw=(w-40)/data.Count*.7;for(int i=0;i<data.Count;i++){double bh=(double)(data[i].Value/max)*(h-45);var r=new Rectangle{Width=Math.Max(8,bw),Height=Math.Max(1,bh),Fill=Brushes.SteelBlue};Canvas.SetLeft(r,20+i*(w-40)/data.Count+(w-40)/data.Count*.15);Canvas.SetTop(r,h-25-bh);c.Children.Add(r);var t=new TextBlock{Text=data[i].Key+" "+(percent?(data[i].Value/data.Sum(x=>x.Value)*100).ToString("N1")+"%":data[i].Value.ToString("N0")),FontSize=11};Canvas.SetLeft(t,20+i*(w-40)/data.Count);Canvas.SetTop(t,h-22);c.Children.Add(t);}}
  private void DrawLine(Canvas c,IList<PortfolioAnalytics.Point> data){c.Children.Clear();if(data.Count<1)return;double w=Math.Max(400,c.ActualWidth),h=c.ActualHeight;decimal min=data.Min(x=>x.Value),max=data.Max(x=>x.Value);if(max==min){max=min+1;}var p=new Polyline{Stroke=Brushes.SteelBlue,StrokeThickness=2};for(int i=0;i<data.Count;i++)p.Points.Add(new System.Windows.Point(20+(w-40)*i/Math.Max(1,data.Count-1),h-25-(double)((data[i].Value-min)/(max-min))*(h-45)));c.Children.Add(p);var label=new TextBlock{Text=(max).ToString("N2")};Canvas.SetLeft(label,2);Canvas.SetTop(label,2);c.Children.Add(label);}
  private void DrawTwoLines(Canvas c,IList<PortfolioAnalytics.Point> a,IList<PortfolioAnalytics.Point> b){c.Children.Clear();if(a.Count==0&&b.Count==0)return;double w=Math.Max(400,c.ActualWidth),h=c.ActualHeight;var all=a.Concat(b).ToList();decimal min=all.Min(x=>x.Value),max=all.Max(x=>x.Value);if(max==min)max=min+1;Func<IList<PortfolioAnalytics.Point>,Brush,Polyline> line=(data,brush)=>{var p=new Polyline{Stroke=brush,StrokeThickness=2};for(int i=0;i<data.Count;i++)p.Points.Add(new System.Windows.Point(20+(w-40)*i/Math.Max(1,data.Count-1),h-25-(double)((data[i].Value-min)/(max-min))*(h-45)));return p;};if(a.Count>0)c.Children.Add(line(a,Brushes.SteelBlue));if(b.Count>0)c.Children.Add(line(b,Brushes.DarkOrange));c.Children.Add(new TextBlock{Text="Niebieski: wartość • pomarańczowy: wpłaty netto"});}
  private void Chart_SizeChanged(object sender,SizeChangedEventArgs e){if(IsLoaded)DrawCharts(_analytics.Xirr(_portfolio.Transactions,CurrentValuePln(),DateTime.Now));}

  public class SummaryRow{public string Type{get;set;}public string Currency{get;set;}public int Count{get;set;}public decimal Invested{get;set;}public decimal Current{get;set;}public decimal CurrentPln{get;set;}public decimal Profit{get;set;}public decimal ProfitPercent=>Invested==0?0:Profit/Invested*100;}
 }
}