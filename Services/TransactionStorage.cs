using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using InvestmentPortfolio.Models;
namespace InvestmentPortfolio.Services
{
 public class TransactionStorage
 {
  private readonly string _path;
  public TransactionStorage(string path){_path=path;}
  public List<PortfolioTransaction> Load(){if(!File.Exists(_path))return new List<PortfolioTransaction>();try{using(var s=File.OpenRead(_path))return (List<PortfolioTransaction>)new XmlSerializer(typeof(List<PortfolioTransaction>)).Deserialize(s);}catch{return new List<PortfolioTransaction>();}}
  public void Save(IEnumerable<PortfolioTransaction> items){var d=Path.GetDirectoryName(_path);if(!string.IsNullOrEmpty(d))Directory.CreateDirectory(d);using(var s=File.Create(_path))new XmlSerializer(typeof(List<PortfolioTransaction>)).Serialize(s,new List<PortfolioTransaction>(items));}
 }
}