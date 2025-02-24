using System.Runtime.ConstrainedExecution;
using System.Threading;

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
	public int TripleCount { get; set; }
	public IOutputter Output { get; set; }
}

internal class Program
{
	static int currentCompleted = 0;

	static object syncRoot = new object();
	static (decimal A, decimal B, decimal C, decimal perc99, decimal perc95, decimal perc50, decimal rangeFrom, decimal rangeTo) currentMaximum = (0, 0, 0, 0, 0, 0, 0, 0);

	//static IOutputter output = new FileOutputter("out.txt");
	static IOutputter output = new ConsoleOutputter();

	static void ThreadWorker(object obj)
{
		// range 30+ yrs
		// vezmu si A/B/C (A >= 40, C <= 40, B <= 40 - pro pripad zefektivneni)
		// vezmu vsechny 30+range
		// simulace pro bruteforce procento vyberu
		// pro ten jeden range najdu takovy procento, kde v_1 == v_n
		// vsechny tyhle procenta hodit na hromadu, najit 99% perc (sort podle velikosti, najit prvek [n//100])
		// pro ten range vypsat vsechny ty procenta a tu reprezentativni hodnotu (procenta[n//100])

		TSI tsi = (TSI)obj;

		int yearSpan = 30;

		(decimal From, decimal To) initWithdrawPercRange = (0, 32); // used for interval halving algorithm, initial percentages

		decimal epsilon = 0.01m; // how much can initial and ending perc differ to still accept it

		IList<(decimal a, decimal b, decimal c)> myTriples = TripleGenerator.GetMyTriples(tsi.TripleCount);
		int tripleRepeatCount = 1000;

		for (int tripleIndex = 0; tripleIndex < myTriples.Count; tripleIndex++)
		{
			(decimal aPerc, decimal bPerc, decimal cPerc) = myTriples[tripleIndex];

			if (aPerc == 0 && bPerc == 0 && cPerc == 0) continue;

			List<decimal> correctPercentages = new List<decimal>();

			sPortfolio portfolio = new sPortfolio(aPerc, bPerc, cPerc, 100000);

			for (int tripleRepeatIndex = 0; tripleRepeatIndex < tripleRepeatCount; ++tripleRepeatIndex)
			{
				IList<int> years = YearListGenerator.GetYears(yearSpan);

				decimal minInitialWithdrawPerc = initWithdrawPercRange.From;
				decimal maxInitialWithdrawPerc = initWithdrawPercRange.To;

				while (true)
				{
					decimal currInitialWithdrawPerc = (minInitialWithdrawPerc + maxInitialWithdrawPerc) / 2;

					var result = portfolio.GetSimulation(years, currInitialWithdrawPerc);

					decimal lastWithdrawPerc;
					try
					{
						lastWithdrawPerc = result[result.Count - 1].WithdrawalValue / result[result.Count - 1].PortValue * 100;
					}
					catch (DivideByZeroException e)
					{
						output.WriteLine("!!!!!!!!!!!!!!!!!!! ERROR !!!!!!!!!!!!!!!!!!!!!!!");
						output.WriteLine($"(aPerc: {aPerc}, bPerc: {bPerc}, cPerc: {cPerc}, )");
						return;
					}

					if (decimal.Abs(lastWithdrawPerc - currInitialWithdrawPerc) <= epsilon)
					{
						correctPercentages.Add(currInitialWithdrawPerc);
						break;
					}

					if (lastWithdrawPerc > currInitialWithdrawPerc || lastWithdrawPerc < 0) { maxInitialWithdrawPerc = currInitialWithdrawPerc; continue; }
					else { minInitialWithdrawPerc = currInitialWithdrawPerc; continue; }
				}
			}

			correctPercentages.Sort();

			decimal fiftiethPerc = correctPercentages[correctPercentages.Count / 2];

			lock (syncRoot)
			{
				output.Write(
						$"[{((double)currentCompleted * 100 / TripleGenerator.ALL_POSSIBLE_TRIPLE_COUNT).ToString("0.0000")}%] For percentages: ({aPerc.ToString("00.00")}, {bPerc.ToString("00.00")}, {cPerc.ToString("00.00")}): range ({correctPercentages[0].ToString("00.00000000")}, {correctPercentages[correctPercentages.Count - 1].ToString("00.00000000")}), 99th percentile: {correctPercentages[correctPercentages.Count / 100].ToString("00.00000000")}, 95th percentile: {correctPercentages[correctPercentages.Count / 20].ToString("00.00000000")}, 50th percentile: {fiftiethPerc.ToString("00.00000000")}");

				// TODO: nevypisovat dvakrat
				Console.WriteLine($"[{((double)currentCompleted * 100 / TripleGenerator.ALL_POSSIBLE_TRIPLE_COUNT).ToString("0.0000")}%]");

				if (fiftiethPerc > currentMaximum.perc50)
				{
					currentMaximum = (aPerc, bPerc, cPerc, correctPercentages[correctPercentages.Count / 100], correctPercentages[correctPercentages.Count / 20], fiftiethPerc, correctPercentages[0], correctPercentages[correctPercentages.Count - 1]);
					output.WriteLine("           NEW MAXIMUM");
				}
				else
				{
					output.WriteLine("");
				}
			}


			currentCompleted++;
		}

		Console.WriteLine($"THREAD FINISHED");
	}

	static void Main(string[] args)
	{
		Thread[] threads = new Thread[32];

		for (int i = 0; i < threads.Length; ++i)
		{
			threads[i] = new Thread((obj) => ThreadWorker(obj));

			threads[i].Start(new TSI { TripleCount = (TripleGenerator.ALL_POSSIBLE_TRIPLE_COUNT / threads.Length) + 1 });
		}

		for (int i = 0; i < threads.Length; ++i)
		{
			threads[i].Join();
		}

		Console.WriteLine("ALL THREADS FINISHED");
		Console.WriteLine($"Maximum: (({currentMaximum.A}, {currentMaximum.B}, {currentMaximum.C}), 99th perc:{currentMaximum.perc99}, 95th perc:{currentMaximum.perc95}, ranging from: {currentMaximum.rangeFrom} to {currentMaximum.rangeTo})");
	}
}
