using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using InvestmentPortfolio.Models;
namespace InvestmentPortfolio.Services
{
 public class PortfolioStorage
 {
  private readonly string _filePath;
  public PortfolioStorage(string filePath) { _filePath = filePath; }
  public List<Investment> Load()
  {
   if (!File.Exists(_filePath)) return new List<Investment>();
   var serializer = new XmlSerializer(typeof(List<Investment>));
   using (var stream = File.OpenRead(_filePath)) return (List<Investment>)serializer.Deserialize(stream);
  }
  public void Save(IEnumerable<Investment> investments)
  {
   var directory = Path.GetDirectoryName(_filePath);
   if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
   var serializer = new XmlSerializer(typeof(List<Investment>));
   using (var stream = File.Create(_filePath)) serializer.Serialize(stream, new List<Investment>(investments));
  }
 }
}