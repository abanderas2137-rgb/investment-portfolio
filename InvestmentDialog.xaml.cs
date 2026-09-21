using System;
using System.Globalization;
using System.Windows;
using InvestmentPortfolio.Models;
namespace InvestmentPortfolio
{
 public partial class InvestmentDialog : Window
 {
  public Investment Investment { get; private set; }
  public InvestmentDialog(Investment existing = null)
  {
   InitializeComponent(); TypeBox.ItemsSource=Enum.GetValues(typeof(InvestmentType)); CurrencyBox.ItemsSource=new[]{"PLN","EUR","USD","GBP","CHF","JPY"};
   Investment=existing ?? new Investment{PurchaseDate=DateTime.Today,Currency="PLN"}; if(existing!=null) Title="Edytuj inwestycję";
   NameBox.Text=Investment.Name; TickerBox.Text=Investment.Ticker; TypeBox.SelectedItem=Investment.Type; CurrencyBox.SelectedItem=Investment.Currency; QuantityBox.Text=Investment.Quantity.ToString(CultureInfo.InvariantCulture); PurchaseBox.Text=Investment.PurchasePrice.ToString(CultureInfo.InvariantCulture); CurrentBox.Text=Investment.CurrentPrice.ToString(CultureInfo.InvariantCulture); DateBox.SelectedDate=Investment.PurchaseDate;
  }
  private void Save_Click(object sender,RoutedEventArgs e)
  {
   decimal q,p,c; if(string.IsNullOrWhiteSpace(NameBox.Text)||!decimal.TryParse(QuantityBox.Text.Replace(',','.'),NumberStyles.Any,CultureInfo.InvariantCulture,out q)||!decimal.TryParse(PurchaseBox.Text.Replace(',','.'),NumberStyles.Any,CultureInfo.InvariantCulture,out p)||!decimal.TryParse(CurrentBox.Text.Replace(',','.'),NumberStyles.Any,CultureInfo.InvariantCulture,out c)||TypeBox.SelectedItem==null||CurrencyBox.SelectedItem==null){MessageBox.Show("Uzupełnij poprawnie wymagane pola.","Błąd",MessageBoxButton.OK,MessageBoxImage.Warning);return;}
   Investment.Name=NameBox.Text.Trim(); Investment.Ticker=TickerBox.Text.Trim(); Investment.Type=(InvestmentType)TypeBox.SelectedItem; Investment.Currency=CurrencyBox.SelectedItem.ToString(); Investment.Quantity=q; Investment.PurchasePrice=p; Investment.CurrentPrice=c; Investment.PurchaseDate=DateBox.SelectedDate??DateTime.Today; DialogResult=true;
  }
  private void Cancel_Click(object sender,RoutedEventArgs e){DialogResult=false;}
 }
}