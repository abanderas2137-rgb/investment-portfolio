using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using InvestmentPortfolio.Models;
namespace InvestmentPortfolio.Services
{
 public class MarketDataCache
 {
  private readonly string _path;
  public MarketDataCache(string path){_path=path;}
  public List<MarketQuote> Load(){if(!File.Exists(_path))return new List<MarketQuote>();try{using(var s=File.OpenRead(_path))return (List<MarketQuote>)new XmlSerializer(typeof(List<MarketQuote>)).Deserialize(s);}catch{return new List<MarketQuote>();}}
  public void Save(IEnumerable<MarketQuote> quotes){var d=Path.GetDirectoryName(_path);if(!string.IsNullOrEmpty(d))Directory.CreateDirectory(d);using(var s=File.Create(_path))new XmlSerializer(typeof(List<MarketQuote>)).Serialize(s,quotes.ToList());}
  public MarketQuote Get(string symbol)=>Load().FirstOrDefault(x=>string.Equals(x.Symbol,symbol,StringComparison.OrdinalIgnoreCase));
  public void Put(MarketQuote quote){var all=Load();all.RemoveAll(x=>string.Equals(x.Symbol,quote.Symbol,StringComparison.OrdinalIgnoreCase));all.Add(quote);Save(all);}
 }
}