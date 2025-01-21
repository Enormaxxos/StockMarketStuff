namespace StockMarket;

public static class DataLoader
{
	public static IList<InputDataFormat> LoadDataFromFile(string fileName)
	{
		if (!File.Exists(fileName)) throw new FileNotFoundException();

		List<InputDataFormat> result = new();

		foreach(string line in File.ReadAllLines(fileName))
		{
			string[] lineSplit = line.Split(',');
			result.Add(new InputDataFormat() 
			{ 
				Year = int.Parse(lineSplit[0]),
				SP500 = decimal.Parse(lineSplit[1]),
				USBond = decimal.Parse(lineSplit[2]),
				CorpBond = decimal.Parse(lineSplit[3]),
				TBill = decimal.Parse(lineSplit[4]),
				Inflation = decimal.Parse(lineSplit[5])
			});
		}

		return result;
	}
}
