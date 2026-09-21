using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using InvestmentPortfolio.Models;

namespace InvestmentPortfolio.Services
{
 public class MarketDataService
 {
  private static readonly HttpClientHandler Handler = CreateHandler();
  private static readonly HttpClient Client = CreateClient();

  private static HttpClientHandler CreateHandler()
  {
   ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
   return new HttpClientHandler
   {
    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
   };
  }

  private static HttpClient CreateClient()
  {
   var client = new HttpClient(Handler);
   client.Timeout = TimeSpan.FromSeconds(15);
   client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/140 Safari/537.36");
   client.DefaultRequestHeaders.Accept.ParseAdd("application/json,text/plain,*/*");
   return client;
  }

  public async Task<MarketQuote> GetQuoteAsync(string symbol)
  {
   if(string.IsNullOrWhiteSpace(symbol)) return null;

   var normalizedSymbol = NormalizeSymbol(symbol);
   var url = "https://query1.finance.yahoo.com/v8/finance/chart/" +
             Uri.EscapeDataString(normalizedSymbol) + "?range=1d&interval=1d";

   try
   {
    using(var response = await Client.GetAsync(url).ConfigureAwait(false))
    {
     var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

     if(!response.IsSuccessStatusCode)
      throw new InvalidOperationException("Yahoo Finance HTTP " + (int)response.StatusCode +
       " (" + response.ReasonPhrase + ") dla symbolu " + normalizedSymbol + ".");

     if(json.IndexOf("\"result\":null", StringComparison.OrdinalIgnoreCase) >= 0)
      throw new InvalidOperationException("Yahoo Finance nie znalazł symbolu " + normalizedSymbol + ".");

     var price = ExtractDecimal(json, "regularMarketPrice");
     if(price == null)
      price = ExtractDecimal(json, @"""close"":\[([0-9.\-]+)");

     if(price == null)
      throw new InvalidOperationException("Yahoo Finance nie zwrócił ceny dla symbolu " + normalizedSymbol + ".");

     var ts = ExtractLong(json, "regularMarketTime");

     return new MarketQuote
     {
      Symbol = symbol.Trim(),
      Price = price.Value,
      Timestamp = ts.HasValue ? DateTimeOffset.FromUnixTimeSeconds(ts.Value).LocalDateTime : DateTime.Now,
      Source = "Yahoo Finance"
     };
    }
   }
   catch(HttpRequestException ex)
   {
    throw new InvalidOperationException("Błąd HTTP podczas pobierania " + normalizedSymbol + ": " + ex.Message, ex);
   }
   catch(TaskCanceledException ex)
   {
    throw new InvalidOperationException("Przekroczono limit czasu podczas pobierania " + normalizedSymbol + ".", ex);
   }
  }

  private static string NormalizeSymbol(string symbol)
  {
   var s = symbol.Trim().ToUpperInvariant();
   if(!s.Contains(".") && !s.Contains("=") && s.Length <= 6)
    return s + ".WA";
   return s;
  }

  private static decimal? ExtractDecimal(string json, string key)
  {
   var m = Regex.Match(json, key + @"[^0-9-]*(-?[0-9]+(?:\.[0-9]+)?)");
   decimal v;
   return m.Success && decimal.TryParse(m.Groups[1].Value, NumberStyles.Any,
    CultureInfo.InvariantCulture, out v) ? (decimal?)v : null;
  }

  private static long? ExtractLong(string json, string key)
  {
   var m = Regex.Match(json, key + @"[^0-9]*(\d+)");
   long v;
   return m.Success && long.TryParse(m.Groups[1].Value, out v) ? (long?)v : null;
  }
 }
}