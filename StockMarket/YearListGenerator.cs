using System.Dynamic;

namespace StockMarket;

public static class YearListGenerator
{
	private static Random RAND;
	
	static YearListGenerator()
	{
		RAND = new Random();
	}

	public static IList<int> GetYears(int count, bool repeatable = false)
	{
		if (repeatable) return GetYearsRepeatable(count);

		if (count > MarketData.YEAR_COUNT) throw new ArgumentOutOfRangeException("Tolik let nemam bruh");

		IList<int> result = new List<int>();
		IList<int> years = new List<int>();
		for (int i = MarketData.DATA_MIN_YEAR; i < MarketData.DATA_MAX_YEAR + 1; i++)
		{
			years.Add(i);
		}

		for (int i = 0; i < count; i++)
		{
			int yearIndex = RAND.Next(years.Count);

			result.Add(years[yearIndex]);
			years.RemoveAt(yearIndex);
		}

		return result;
	}

	private static IList<int> GetYearsRepeatable(int count)
	{
		IList<int> result = new List<int>();

		for (int i = 0; i < count; i++)
		{
			result.Add(RAND.Next(MarketData.DATA_MIN_YEAR, MarketData.DATA_MAX_YEAR + 1));
		}

		return result;
	}
}
