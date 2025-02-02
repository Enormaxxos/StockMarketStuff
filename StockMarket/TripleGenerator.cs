using System.Drawing;

namespace StockMarket;

public static class TripleGenerator
{
	private static (decimal From, decimal To) A_RANGE = (0,100);
	private static (decimal From, decimal To) B_RANGE = (0,100);
	private static (decimal From, decimal To) C_RANGE = (0,100);
	private static decimal STEP = 1m;

	private static IList<(decimal a, decimal b, decimal c)> POSSIBLE_TRIPLES;
	public static readonly int ALL_POSSIBLE_TRIPLE_COUNT;

	private static object _syncRoot;

	static TripleGenerator()
	{
		POSSIBLE_TRIPLES = new List<(decimal a, decimal b, decimal c)>();

		for (decimal a = A_RANGE.From; a < A_RANGE.To + 1; a += STEP)
		{
			for (decimal b = B_RANGE.From; b < B_RANGE.To + 1; b += STEP)
			{
				decimal c = 100m - a - b;

				if (c > C_RANGE.To) continue;
				if (c < C_RANGE.From) goto skip_rest_of_b;

				POSSIBLE_TRIPLES.Add((a, b, c));
			}

	skip_rest_of_b:;

		}

		_syncRoot = new object();

		ALL_POSSIBLE_TRIPLE_COUNT = POSSIBLE_TRIPLES.Count;
	}

	public static IList<(decimal a, decimal b, decimal c)> GetMyTriples(int count)
	{
		(decimal a, decimal b, decimal c)[] result = new (decimal a, decimal b, decimal c)[count];

		for (int i = 0; i < count; i++)
		{
			if (POSSIBLE_TRIPLES.Count == 0) break;

			lock (_syncRoot)
			{
				result[i] = POSSIBLE_TRIPLES[POSSIBLE_TRIPLES.Count - 1];
				POSSIBLE_TRIPLES.RemoveAt(POSSIBLE_TRIPLES.Count - 1);
			}
		}

		return result;
	}
}
