namespace StockMarket;

public class InputDataFormat
{
	public int Year { get; set; }
	public decimal SP500 { get; set; }
	public decimal USBond { get; set; }
	public decimal CorpBond { get; set; }
	public decimal SimulatedBond 
	{ 
		get
		{
			decimal USBondShare = 0.5m;
			decimal CorpBondShare = 1m - USBondShare; // dont touch

			if (CorpBondShare > 1m || CorpBondShare < 0m) throw new InvalidDataException("Incorrect shares set on us bond and corporate bond, tweak usbondshare so that its between 0 and 1.");

			return USBondShare * USBond + CorpBondShare * CorpBond;
		} 
	}
	public decimal TBill { get; set; }
	public decimal Inflation { get; set; }
}
