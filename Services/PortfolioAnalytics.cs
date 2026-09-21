using System;
using System.Collections.Generic;
using System.Linq;
using InvestmentPortfolio.Models;
namespace InvestmentPortfolio.Services
{
 public class PortfolioAnalytics
 {
  public decimal Xirr(IEnumerable<PortfolioTransaction> transactions,decimal currentValue,DateTime now)
  {
   var flows=transactions.Where(IsExternalCashFlow).Select(t=>new CashFlow{Date=t.Date,Amount=CashFlowAmount(t)}).Where(x=>x.Amount!=0).ToList();
   flows.Add(new CashFlow{Date=now,Amount=currentValue}); if(flows.Count<2||!flows.Any(x=>x.Amount<0)||!flows.Any(x=>x.Amount>0))return 0;
   double rate=0.1; for(int i=0;i<100;i++){double f=0,df=0;var d0=flows[0].Date;foreach(var c in flows){double years=(c.Date-d0).TotalDays/365.0;double den=Math.Pow(1+rate,years);f+=(double)c.Amount/den;df+=-years*(double)c.Amount/Math.Pow(1+rate,years+1);}if(Math.Abs(df)<1e-12)break;var next=rate-f/df;if(next<=-0.9999||double.IsNaN(next)||double.IsInfinity(next))break;if(Math.Abs(next-rate)<1e-8)return (decimal)next;rate=next;}return (decimal)rate;
  }
  public bool IsDividend(PortfolioTransaction t){var s=(t.TransactionType??"").ToLowerInvariant();return s.Contains("dywid")||s.Contains("dividend");}
  public bool IsDeposit(PortfolioTransaction t){var s=(t.TransactionType??"").ToLowerInvariant();return s.Contains("wpłat")||s.Contains("wplat")||s.Contains("deposit")||s.Contains("zasil");}
  public bool IsWithdrawal(PortfolioTransaction t){var s=(t.TransactionType??"").ToLowerInvariant();return s.Contains("wypłat")||s.Contains("wyplat")||s.Contains("withdraw");}
  public bool IsExternalCashFlow(PortfolioTransaction t)=>IsDeposit(t)||IsWithdrawal(t);
  public decimal CashFlowAmount(PortfolioTransaction t){var v=Math.Abs(t.TotalPln);return IsDeposit(t)?-v:v;}
  public decimal NetDeposits(IEnumerable<PortfolioTransaction> tx){return tx.Where(IsExternalCashFlow).Sum(t=>IsDeposit(t)?Math.Abs(t.TotalPln):-Math.Abs(t.TotalPln));}
  public decimal AverageMonthlyDeposit(IEnumerable<PortfolioTransaction> tx)
  {
   var flows=tx.Where(IsExternalCashFlow).OrderBy(t=>t.Date).ToList();if(flows.Count==0)return 0;
   var months=Math.Max(1,(int)Math.Ceiling((flows.Last().Date-flows.First().Date).TotalDays/30.4375));return NetDeposits(flows)/months;
  }
  public List<Point> DividendSeries(IEnumerable<PortfolioTransaction> tx){return tx.Where(IsDividend).GroupBy(t=>new DateTime(t.Date.Year,t.Date.Month,1)).OrderBy(g=>g.Key).Select(g=>new Point{Date=g.Key,Value=g.Sum(x=>Math.Abs(x.TotalPln))}).ToList();}
  public List<Point> NetDepositSeries(IEnumerable<PortfolioTransaction> tx){decimal sum=0;return tx.Where(IsExternalCashFlow).OrderBy(t=>t.Date).GroupBy(t=>t.Date.Date).Select(g=>{sum+=g.Sum(CashFlowAmount)*-1;return new Point{Date=g.Key,Value=sum};}).ToList();}
  public List<Point> DrawdownSeries(IEnumerable<PortfolioSnapshot> snapshots){decimal peak=0;return snapshots.OrderBy(x=>x.Timestamp).Select(x=>{peak=Math.Max(peak,x.ValuePln);return new Point{Date=x.Timestamp,Value=peak==0?0:(x.ValuePln/peak-1)*100};}).ToList();}
  public List<Point> Forecast(decimal currentValue,decimal xirr,decimal monthlyDeposit,int months){var r=(double)xirr;var mr=Math.Pow(1+r,1.0/12.0)-1;var v=(double)currentValue;var list=new List<Point>();for(int i=1;i<=months;i++){v=v*(1+mr)+(double)monthlyDeposit;list.Add(new Point{Date=DateTime.Today.AddMonths(i),Value=(decimal)v});}return list;}
  public class CashFlow{public DateTime Date;public decimal Amount;}
  public class Point{public DateTime Date;public decimal Value;}
 }
}