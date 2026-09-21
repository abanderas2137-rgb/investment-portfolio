using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InvestmentPortfolio.Services;
namespace InvestmentPortfolio.Tests
{
 [TestClass]
 public class CsvPortfolioImporterTests
 {
  [TestMethod]
  public void Import_ReadsRequiredColumnsAndPolishValues()
  {
   var path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".csv");
   File.WriteAllText(path,"Konto;Data;Ticker;Waluta;Nazwa;Klasa aktywów;Rodzaj transakcji;Liczba;Cena;Prowizje;Kurs PLN transakcji;Cena nominalna;Total PLN;Komentarz\nMakler;2025-01-10;PKO.WA;PLN;PKO;Akcje;Kupno;10;50,50;5;1;0;510;test");
   try{var rows=new CsvPortfolioImporter().Import(path);Assert.AreEqual(1,rows.Count);Assert.AreEqual("PKO.WA",rows[0].Ticker);Assert.AreEqual(10m,rows[0].Quantity);Assert.AreEqual(510m,rows[0].TotalPln);}
   finally{if(File.Exists(path))File.Delete(path);}
  }
 }
}