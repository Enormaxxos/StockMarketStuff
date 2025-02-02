using System.Security.Cryptography;

namespace StockMarket;

public struct sPortfolio
{
	private decimal PercA { get; set; }
	private decimal PercB { get; set; }
	private decimal PercC { get; set; }

	private decimal InitialMoney { get; set; }

	public sPortfolio(decimal percA, decimal percB, decimal percC, decimal initialMoney)
	{
		PercA = percA / 100m;
		PercB = percB / 100m;
		PercC = percC / 100m;
		InitialMoney = initialMoney;
	}

	public IList<YearValueBag> GetSimulation(IList<int> years, decimal initialWithdrawPerc)
	{
		var firstYearData = MarketData.GetDataFromYear(years[0]);

		var result = new List<YearValueBag>();

		var firstYearBag = new YearValueBag()
		{
			Year = years[0],
			PortValue = InitialMoney * (PercA * (1 + firstYearData.SP500) + PercB * (1 + firstYearData.SimulatedBond) + PercC * (1 + firstYearData.TBill))
		};
		firstYearBag.WithdrawalValue = firstYearBag.PortValue * (initialWithdrawPerc / 100m);
		firstYearBag.PortValue -= firstYearBag.WithdrawalValue;

		result.Add(firstYearBag);

		for (int i = 1; i < years.Count; ++i)
		{
			var prevYear = result[result.Count - 1];
			var nextYearData = MarketData.GetDataFromYear(years[i]);
			var newYear = new YearValueBag()
			{
				Year = nextYearData.Year,
				WithdrawalValue = prevYear.WithdrawalValue * (1 + nextYearData.Inflation),
			};
			newYear.PortValue = prevYear.PortValue * (PercA * (1 + nextYearData.SP500) + PercB * (1 + nextYearData.SimulatedBond) + PercC * (1 + nextYearData.TBill)) - newYear.WithdrawalValue;

			result.Add(newYear);
		}

		return result;
	}

	public IList<YearValueBag> GetSimulation(int fromYear, int toYear, decimal initialWithdrawPerc)
	{
		int[] years = new int[toYear - fromYear + 1];
		for (int i = 0; i < years.Length; ++i)
		{
			years[i] = i + fromYear;
		}

		return GetSimulation(years, initialWithdrawPerc);
	}
}
