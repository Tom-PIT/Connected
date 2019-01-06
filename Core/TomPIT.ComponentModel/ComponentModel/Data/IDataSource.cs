namespace TomPIT.ComponentModel.Data
{
	/// <summary>
	/// Represents DataSource component for querying physical
	/// data sources. 
	/// </summary>
	public interface IDataSource : IDataElement, IConfiguration
	{
		/// <summary>
		/// Proposed schema which should be used when calling query
		/// methods on the physical data sources.
		/// </summary>
		ListItems<IDataField> Fields { get; }
	}
}