using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InvestmentPortfolio.Models;
using InvestmentPortfolio.Services;
namespace InvestmentPortfolio.Tests
{
 [TestClass]
 public class PortfolioAnalyticsTests
 {
  private readonly PortfolioAnalytics _sut=new PortfolioAnalytics();

  [TestMethod]
  public void NetDeposits_CalculatesDepositsMinusWithdrawals()
  {
   var tx=new List<PortfolioTransaction>{
    new PortfolioTransaction{Date=new DateTime(2025,1,1),TransactionType="Wpłata",TotalPln=10000},
    new PortfolioTransaction{Date=new DateTime(2025,2,1),TransactionType="Wpłata",TotalPln=2000},
    new PortfolioTransaction{Date=new DateTime(2025,3,1),TransactionType="Wypłata",TotalPln=500}};
   Assert.AreEqual(11500m,_sut.NetDeposits(tx));
  }

  [TestMethod]
  public void DividendSeries_GroupsByMonth()
  {
   var tx=new List<PortfolioTransaction>{
    new PortfolioTransaction{Date=new DateTime(2025,1,10),TransactionType="Dywidenda",TotalPln=100},
    new PortfolioTransaction{Date=new DateTime(2025,1,20),TransactionType="Dywidenda",TotalPln=50},
    new PortfolioTransaction{Date=new DateTime(2025,2,1),TransactionType="Dywidenda",TotalPln=25}};
   var result=_sut.DividendSeries(tx);
   Assert.AreEqual(2,result.Count);Assert.AreEqual(150m,result[0].Value);Assert.AreEqual(25m,result[1].Value);
  }

  [TestMethod]
  public void Xirr_IsApproximatelyCorrectForOneYearDouble()
  {
   var tx=new[]{new PortfolioTransaction{Date=new DateTime(2025,1,1),TransactionType="Wpłata",TotalPln=1000}};
   var result=_sut.Xirr(tx,1100,new DateTime(2026,1,1));
   Assert.IsTrue(result>0.09m&&result<0.11m);
  }

  [TestMethod]
  public void Drawdown_IsZeroAtNewHigh()
  {
   var s=new[]{new PortfolioSnapshot{Timestamp=new DateTime(2025,1,1),ValuePln=1000},new PortfolioSnapshot{Timestamp=new DateTime(2025,2,1),ValuePln=800},new PortfolioSnapshot{Timestamp=new DateTime(2025,3,1),ValuePln=1200}};
   var result=_sut.DrawdownSeries(s);
   Assert.AreEqual(0m,result[0].Value);Assert.AreEqual(-20m,result[1].Value);Assert.AreEqual(0m,result[2].Value);
  }

  [TestMethod]
  public void Forecast_GrowsWithPositiveReturnAndDeposits()
  {
   var result=_sut.Forecast(1000,0.12m,100,12);
   Assert.AreEqual(12,result.Count);Assert.IsTrue(result[11].Value>1000m+1200m);
  }
 }
}