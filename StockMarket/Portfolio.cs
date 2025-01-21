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

	public IList<YearValueBag> GetSimulation(int fromYear, int toYear, decimal initialWithdrawPerc)
	{
		var firstYearData = MarketData.GetDataFromYear(fromYear);

		var result = new List<YearValueBag>();

		var firstYearBag = new YearValueBag()
		{
			Year = fromYear,
			PortValue = InitialMoney * (PercA * (1 + firstYearData.SP500) + PercB * (1 + firstYearData.SimulatedBond) + PercC * (1 + firstYearData.TBill))
		};
		firstYearBag.WithdrawalValue = firstYearBag.PortValue * (initialWithdrawPerc / 100m);

		result.Add(firstYearBag);

		while (true)
		{
			var prevYear = result[result.Count - 1];
			var nextYearData = MarketData.GetDataFromYear(prevYear.Year + 1);
			var newYear = new YearValueBag()
			{
				Year = prevYear.Year + 1,
				WithdrawalValue = prevYear.WithdrawalValue * (1 + nextYearData.Inflation),
			};
			newYear.PortValue = prevYear.PortValue * (PercA * (1 + nextYearData.SP500) + PercB * (1 + nextYearData.SimulatedBond) + PercC * (1 + nextYearData.TBill)) - newYear.WithdrawalValue;

			result.Add(newYear);

			if (newYear.Year == toYear) 
				break;
		}

		return result;
	}
}
