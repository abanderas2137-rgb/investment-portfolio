using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using InvestmentPortfolio.Models;
namespace InvestmentPortfolio.Services
{
 public class SnapshotStorage
 {
  private readonly string _path;
  public SnapshotStorage(string path){_path=path;}
  public List<PortfolioSnapshot> Load(){if(!File.Exists(_path))return new List<PortfolioSnapshot>();try{using(var s=File.OpenRead(_path))return (List<PortfolioSnapshot>)new XmlSerializer(typeof(List<PortfolioSnapshot>)).Deserialize(s);}catch{return new List<PortfolioSnapshot>();}}
  public void Save(IEnumerable<PortfolioSnapshot> items){var d=Path.GetDirectoryName(_path);if(!string.IsNullOrEmpty(d))Directory.CreateDirectory(d);using(var s=File.Create(_path))new XmlSerializer(typeof(List<PortfolioSnapshot>)).Serialize(s,new List<PortfolioSnapshot>(items));}
 }
}