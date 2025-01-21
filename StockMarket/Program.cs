namespace StockMarket;

static class MarketData
{
	private static readonly IList<InputDataFormat> DATA;
	public static readonly int DATA_MIN_YEAR;
	public static readonly int DATA_MAX_YEAR;

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

internal class TSI
{
	public decimal FromA { get; set; }
	public decimal ToA { get; set; }
}

internal class Program
{
	static void Main(string[] args)
	{
		Thread[] threads = new Thread[20];
		decimal aShare = 60m / threads.Length; // (40 <= A <= 100)

		object syncRoot = new object();
		(decimal A, decimal B, decimal C, decimal value, decimal rangeFrom, decimal rangeTo) currentMaximum = (0, 0, 0, 0, 0, 0);

		for (int i = 0; i < threads.Length; ++i)
		{
#if DEBUG
			Console.WriteLine($"FromA = {(i * aShare) + 40m}, ToA = {((i + 1) * aShare) - (i < threads.Length - 1 ? 1 : 0) + 40m}");
			continue;
#endif

			threads[i] = new Thread((obj) =>
			{
				// range 30+ yrs
				// vezmu si A/B/C (A >= 40, C <= 40, B <= 40 - pro pripad zefektivneni)
				// vezmu vsechny 30+range
				// simulace pro bruteforce procento vyberu
				// pro ten jeden range najdu takovy procento, kde v_1 == v_n
				// vsechny tyhle procenta hodit na hromadu, najit 99% perc (sort podle velikosti, najit prvek [n//100])
				// pro ten range vypsat vsechny ty procenta a tu reprezentativni hodnotu (procenta[n//100])

				TSI tsi = (TSI)obj;

				(decimal From, decimal To) aRange = (tsi.FromA, tsi.ToA);
				(decimal From, decimal To) bRange = (0, 100);
				(decimal From, decimal To) cRange = (0, 100);

				(int From, int To) yearspanRange = (30, -1); // use -1 in 'To' to not restrict maximal yearspan

				(decimal From, decimal To) initWithdrawPercRange = (0, 32); // used for interval halving algorithm, initial percentages

				decimal epsilon = 0.01m; // how much can initial and ending perc differ to still accept it

				for (decimal aPerc = aRange.From; aPerc <= aRange.To; aPerc++)
				{
					if (100m - aPerc < bRange.From) continue;

					for (decimal bPerc = bRange.From; bPerc <= (100m - aPerc); bPerc++)
					{
						if (aPerc + bPerc > 100m) break;

						decimal cPerc = 100 - aPerc - bPerc;

						// pro jistotu, jsem kokot
						if (aPerc < 0m || aPerc > 100m || bPerc < 0m || bPerc > 100m || cPerc < 0m || cPerc > 100m)
							throw new InvalidDataException("nekde jsi to posral petre");

						if (cPerc < cRange.From || cPerc > cRange.To) continue;

						List<decimal> correctPercentages = new List<decimal>();

						sPortfolio portfolio = new sPortfolio(aPerc, bPerc, cPerc, 100000);

						for (int yearspan = yearspanRange.From; /* NO CHECK, CARE FOR INFINITE CYCLE */ ; yearspan++)
						{
							if ((yearspanRange.To != -1 && yearspan > yearspanRange.To) || yearspan > (MarketData.DATA_MAX_YEAR - MarketData.DATA_MIN_YEAR))
								break;

							for (int yearFrom = MarketData.DATA_MIN_YEAR; yearFrom < MarketData.DATA_MAX_YEAR; yearFrom++)
							{
								int yearTo = yearFrom + yearspan;
								if (yearTo > MarketData.DATA_MAX_YEAR) { break; }

								decimal minInitialWithdrawPerc = initWithdrawPercRange.From;
								decimal maxInitialWithdrawPerc = initWithdrawPercRange.To;

								while (true)
								{
									decimal currInitialWithdrawPerc = (minInitialWithdrawPerc + maxInitialWithdrawPerc) / 2;

									var result = portfolio.GetSimulation(yearFrom, yearTo, currInitialWithdrawPerc);

									decimal lastWithdrawPerc = result[result.Count - 1].WithdrawalValue / result[result.Count - 1].PortValue * 100;

									if (decimal.Abs(lastWithdrawPerc - currInitialWithdrawPerc) <= epsilon)
									{
										correctPercentages.Add(currInitialWithdrawPerc);
										break;
									}

									if (lastWithdrawPerc > currInitialWithdrawPerc || lastWithdrawPerc < 0) { maxInitialWithdrawPerc = currInitialWithdrawPerc; continue; }
									else { minInitialWithdrawPerc = currInitialWithdrawPerc; continue; }
								}
							}
						}

						correctPercentages.Sort();

						decimal ninetyninethPerc = correctPercentages[correctPercentages.Count / 100];

						lock (syncRoot)
						{
							if (ninetyninethPerc > currentMaximum.value)
							{
								currentMaximum = (aPerc, bPerc, cPerc, ninetyninethPerc, correctPercentages[0], correctPercentages[correctPercentages.Count - 1]);
								Console.WriteLine(
									$"\n\nFor percentages: ({aPerc}, {bPerc}, {cPerc}): range ({correctPercentages[0]}, {correctPercentages[correctPercentages.Count - 1]}), 99th percentile: {ninetyninethPerc}, 95th percentile: {correctPercentages[correctPercentages.Count / 20]}    NEW MAXIMUM\n\n");
							}
							else
							{
								Console.WriteLine(
									$"For percentages: ({aPerc}, {bPerc}, {cPerc}): range ({correctPercentages[0]}, {correctPercentages[correctPercentages.Count - 1]}), 99th percentile: {ninetyninethPerc}, 95th percentile: {correctPercentages[correctPercentages.Count / 20]}");
							}
						}
					}
				}

				Console.WriteLine($"THREAD ({aRange.From}, {aRange.To}) FINISHED");

			});

			threads[i].Start(new TSI { FromA = (i * aShare) + 40m, ToA = ((i + 1) * aShare) - (i < threads.Length - 1 ? 1 : 0) + 40m });
		}
#if !DEBUG
		for (int i = 0; i < threads.Length; ++i)
		{
			threads[i].Join();
		}

		Console.WriteLine("ALL THREADS FINISHED");
#endif
	}
}
