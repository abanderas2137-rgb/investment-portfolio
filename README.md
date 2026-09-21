# Investment Portfolio

WPF desktop application for tracking an investment portfolio.

## Technology
C#, WPF, .NET Framework 4.7.1.

## MVP features
- stocks, bonds, ETFs, deposits, cryptocurrencies, funds and other assets;
- PLN, EUR, USD, GBP, CHF and JPY;
- quantity, purchase price, current price and purchase date;
- automatic position value and profit/loss calculation;
- summaries by investment type and currency;
- add, edit and delete investments;
- local XML persistence.

Important: multi-currency values are not converted to PLN yet. The main total therefore sums nominal amounts across currencies and should be treated as an MVP. A future FX module can provide normalized portfolio valuation.

## Run
Open InvestmentPortfolio.csproj in Visual Studio with WPF/.NET Framework 4.7.1 support and run the project.

Data is stored in %LOCALAPPDATA%/InvestmentPortfolio/portfolio.xml.
