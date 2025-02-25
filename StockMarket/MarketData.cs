namespace StockMarket
{
	static class MarketData
	{
		private static readonly IList<InputDataFormat> DATA;
		public static readonly int DATA_MIN_YEAR;
		public static readonly int DATA_MAX_YEAR;
		public static int YEAR_COUNT => DATA.Count;

		static MarketData()
		{
			DATA = DataLoader.LoadDataFromFile("test.csv");
			DATA_MIN_YEAR = DATA[0].Year;
			DATA_MAX_YEAR = DATA[DATA.Count - 1].Year;
		}

		public static InputDataFormat GetDataFromYear(int year)
		{
			return DATA.FirstOrDefault((y) => y.Year == year);
		}
	}
}
