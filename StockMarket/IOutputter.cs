namespace StockMarket
{
	public interface IOutputter : IDisposable
	{
		public void Write(string str);
		public void WriteLine(string str);
	}

	public class ConsoleOutputter : IOutputter
	{
		public void Write(string str)
		{
			Console.Write(str);
		}

		public void WriteLine(string str)
		{
			Console.WriteLine(str);
		}

		public void Dispose()
		{

		}
	}

	public class FileOutputter : IOutputter
	{
		private StreamWriter file;
		public FileOutputter(string filename)
		{
			file = new StreamWriter(filename, false);
		}

		public void Dispose()
		{
			file.Dispose();
		}

		public void Write(string str)
		{
			file.Write(str);
		}

		public void WriteLine(string str)
		{
			file.WriteLine(str);
		}
	}

}
