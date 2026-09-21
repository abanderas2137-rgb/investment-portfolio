# Investment Portfolio

WPF desktop application for tracking an investment portfolio.

## Technology
C#, WPF, .NET Framework 4.7.1.

## Features
- stocks, bonds, ETFs, deposits, cryptocurrencies, funds and other assets;
- PLN, EUR, USD, GBP, CHF and JPY;
- market prices for stocks, bonds and ETFs are fetched automatically when a ticker is supplied;
- last successful market quote is cached locally and used offline;
- offline banner with the timestamp of the last available quote;
- automatic FX conversion to PLN for portfolio valuation and currency allocation;
- CSV import with columns:
  `Konto, Data, Ticker, Waluta, Nazwa, Klasa aktywów, Rodzaj transakcji, Liczba, Cena, Prowizje, Kurs PLN transakcji, Cena nominalna, Total PLN, Komentarz`;
- summaries by investment type and currency;
- dividends, net deposits, XIRR, drawdown and future-value forecast;
- portfolio snapshots used for the account-value/drawdown charts;
- unit tests in `InvestmentPortfolio.Tests`.

## Market data
The proof of concept uses Yahoo Finance's chart endpoint for quote retrieval. It is an unofficial endpoint, so the market-data provider is isolated in `MarketDataService` and can be replaced later with a licensed provider. Tickers must use the provider's symbol format, for example `AAPL` or `PKO.WA`.

Market quotes and FX quotes are cached under:
`%LOCALAPPDATA%/InvestmentPortfolio/market-cache.xml`.

If a quote cannot be refreshed, the last cached quote is used.

## Run
Open `InvestmentPortfolio.sln` in Visual Studio with WPF/.NET Framework 4.7.1 support and run the project.

## Tests
The solution contains `InvestmentPortfolio.Tests`. CI runs the unit tests automatically on Windows.

## Known limitation
The account-value history chart starts collecting valuation snapshots when the application refreshes market data. It therefore becomes more useful as the application is used over time. Historical account valuation is not reconstructed from CSV because the CSV does not contain historical market prices for every date.
