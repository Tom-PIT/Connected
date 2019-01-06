namespace TomPIT.ComponentModel.Data
{
	public interface IBoundField : IDataField
	{
		string Mapping { get; }
		bool SupportsLocalization { get; }
		bool SupportsTimeZone { get; }
	}
}
