using System;

namespace TomPIT.ComponentModel.Data
{
	/// <summary>
	/// Defines connection parameters to the physical data source
	/// </summary>
	public interface IConnectionConfiguration : IConfiguration, IText, INamespaceElement
	{
		/// <summary>
		/// This is mostly connection string. Its value depends on the provider used.
		/// User should have knowledge about DataProvider consuming this configuration
		/// </summary>
		string Value { get; }
		/// <summary>
		/// System administrator can disable this connection at any time and forcibly
		/// deny any subsequent connection requests.
		/// </summary>
		bool Enabled { get; }
		/// <summary>
		/// Data provider used with this connection. Data providers are dynamically loaded
		/// through the system configuration. <code>Property</code> value should provide
		/// enough information for data provider to be able to connect to the physical data
		/// source successfully.
		/// </summary>
		Guid DataProvider { get; }
	}
}
