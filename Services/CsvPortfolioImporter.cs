using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using InvestmentPortfolio.Models;
namespace InvestmentPortfolio.Services
{
 public class CsvPortfolioImporter
 {
  public List<PortfolioTransaction> Import(string path)
  {
   var lines=File.ReadAllLines(path,Encoding.UTF8);if(lines.Length<2)throw new InvalidDataException("CSV nie zawiera danych.");
   var delimiter=lines[0].Count(c=>c==';')>=lines[0].Count(c=>c==',')?';':',';
   var headers=Parse(lines[0],delimiter);
   string[] required={"Konto","Data","Ticker","Waluta","Nazwa","Klasa aktywów","Rodzaj transakcji","Liczba","Cena","Prowizje","Kurs PLN transakcji","Cena nominalna","Total PLN","Komentarz"};
   foreach(var h in required)if(!headers.Contains(h,StringComparer.OrdinalIgnoreCase))throw new InvalidDataException("Brak kolumny: "+h);
   var ix=headers.Select((h,i)=>new{h,i}).ToDictionary(x=>x.h,StringComparer.OrdinalIgnoreCase);
   var result=new List<PortfolioTransaction>();
   for(int n=1;n<lines.Length;n++){if(string.IsNullOrWhiteSpace(lines[n]))continue;var c=Parse(lines[n],delimiter);string Get(string h)=>c[ix[h].i];
    result.Add(new PortfolioTransaction{Account=Get("Konto"),Date=ParseDate(Get("Data")),Ticker=Get("Ticker"),Currency=Get("Waluta"),Name=Get("Nazwa"),Type=ParseType(Get("Klasa aktywów")),TransactionType=Get("Rodzaj transakcji"),Quantity=ParseDecimal(Get("Liczba")),Price=ParseDecimal(Get("Cena")),Fees=ParseDecimal(Get("Prowizje")),TransactionFxRate=ParseDecimal(Get("Kurs PLN transakcji")),NominalPrice=ParseDecimal(Get("Cena nominalna")),TotalPln=ParseDecimal(Get("Total PLN")),Comment=Get("Komentarz")});
   } return result;
  }
  private static string[] Parse(string line,char d){var r=new List<string>();var sb=new StringBuilder();bool q=false;foreach(char ch in line){if(ch=='"'){q=!q;continue;}if(ch==d&&!q){r.Add(sb.ToString());sb.Clear();}else sb.Append(ch);}r.Add(sb.ToString());return r.ToArray();}
  private static decimal ParseDecimal(string s){decimal v;return decimal.TryParse(s.Replace(" ","").Replace(",", "."),NumberStyles.Any,CultureInfo.InvariantCulture,out v)?v:0;}
  private static DateTime ParseDate(string s){DateTime d;if(DateTime.TryParse(s,CultureInfo.GetCultureInfo("pl-PL"),DateTimeStyles.None,out d)||DateTime.TryParse(s,out d))return d;throw new InvalidDataException("Nieprawidłowa data: "+s);}
  private static InvestmentType ParseType(string s){var x=s.ToLowerInvariant();if(x.Contains("akcj"))return InvestmentType.Stock;if(x.Contains("oblig"))return InvestmentType.Bond;if(x.Contains("etf"))return InvestmentType.ETF;if(x.Contains("krypto"))return InvestmentType.Cryptocurrency;if(x.Contains("fundusz")||x.Contains("fund"))return InvestmentType.Fund;if(x.Contains("lokat"))return InvestmentType.Deposit;return InvestmentType.Other;}
 }
}