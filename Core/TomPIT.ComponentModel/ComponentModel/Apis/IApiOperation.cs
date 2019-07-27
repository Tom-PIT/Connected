using System;
using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel.Apis
{
	/// <summary>
	/// Each <code>IApiOperation</code> implements business logic which is based on the
	/// input parameters and can return data in any data structure.
	/// </summary>
	public interface IApiOperation : ISourceCode, IElement
	{
		/// <summary>
		/// The name of the operation. This is used as part of the identifier
		/// in the form of <code>MicroService/Api/Operation</code>.
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Access modifier for this operation. This is a second level modifier and stands
		/// behind <code>IApi.Scope</code> property.
		/// </summary>
		ElementScope Scope { get; }
		/// <summary>
		/// Protocol settings used by the <code>IApiOperation</code>.
		/// </summary>
		IOperationProtocolOptions Protocols { get; }
		IMetricConfiguration Metrics { get; }
	}
}
