using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using InvestmentPortfolio.Models;
namespace InvestmentPortfolio.Services
{
 public class MarketDataService
 {
  private static readonly HttpClient Client = new HttpClient();
  public async Task<MarketQuote> GetQuoteAsync(string symbol)
  {
   if(string.IsNullOrWhiteSpace(symbol)) return null;
   var url="https://query1.finance.yahoo.com/v8/finance/chart/"+Uri.EscapeDataString(symbol.Trim())+"?range=1d&interval=1d";
   using(var request=new HttpRequestMessage(HttpMethod.Get,url))
   {
    request.Headers.UserAgent.ParseAdd("Mozilla/5.0");
    var response=await Client.SendAsync(request).ConfigureAwait(false);
    response.EnsureSuccessStatusCode();
    var json=await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    var price=ExtractDecimal(json,"regularMarketPrice");
    if(price==null) price=ExtractDecimal(json,""close":\[([0-9.\-]+)");
    if(price==null) throw new InvalidOperationException("Brak ceny dla symbolu "+symbol);
    var ts=ExtractLong(json,"regularMarketTime");
    return new MarketQuote{Symbol=symbol,Price=price.Value,Timestamp=ts.HasValue?DateTimeOffset.FromUnixTimeSeconds(ts.Value).LocalDateTime:DateTime.Now,Source="Yahoo Finance"};
   }
  }
  private static decimal? ExtractDecimal(string json,string key){var m=Regex.Match(json,key+"[^0-9-]*(-?[0-9]+(?:\.[0-9]+)?)");decimal v;return m.Success&&decimal.TryParse(m.Groups[1].Value,NumberStyles.Any,CultureInfo.InvariantCulture,out v)?(decimal?)v:null;}
  private static long? ExtractLong(string json,string key){var m=Regex.Match(json,key+"[^0-9]*(\d+)");long v;return m.Success&&long.TryParse(m.Groups[1].Value,out v)?(long?)v:null;}
 }
}