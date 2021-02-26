using TomPIT.Collections;

namespace TomPIT.ComponentModel.Apis
{
	/// <summary>
	/// This is a main business logic component. It serves as a proxy between
	/// clients (User interfaces, REST calls etc.) and the actual data sources.
	/// Additional to the data sources, which perform only read write operations
	/// on the physical data sources this components implements additional business
	/// logic and often performs multiple data source operations inside a single call.
	/// </summary>
	public interface IApiConfiguration : IConfiguration, IText
	{
		/// <summary>
		/// List of operations which contains the actual business logic.
		/// </summary>
		ListItems<IApiOperation> Operations { get; }
		/// <summary>
		/// Protocol settings used by each <code>IApi</code> component.
		/// </summary>
		IApiProtocolOptions Protocols { get; }
		/// <summary>
		/// Defines access modifiers for restricting calls by a context.
		/// </summary>
		ElementScope Scope { get; }
	}
}
