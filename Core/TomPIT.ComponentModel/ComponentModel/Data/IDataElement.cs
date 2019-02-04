using System;
using System.Data;
using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel.Data
{
	/// <summary>
	/// This is a common interface for <code>IDataSource</code>
	/// and <code>ITransaction</code> components.
	/// </summary>
	public interface IDataElement : IElement
	{
		/// <summary>
		/// The actual command text used when performing data source
		/// operation. This value can vary by <code>IDataProvider</code>
		/// used.
		/// </summary>
		string CommandText { get; }
		/// <summary>
		/// Command type used by a database call.
		/// </summary>
		CommandType CommandType { get; }
		/// <summary>
		/// This is a reference to the <code>IConnection</code> component and
		/// defined connection parameters and <code>IDataProvider</code> used
		/// for an operation.
		/// </summary>
		Guid Connection { get; }
		/// <summary>
		/// A maximum time used before a command attempt is marked as unsuccessful. Some
		/// <code>IDataProvider</code>s don't support this attribute.
		/// </summary>
		int CommandTimeout { get; }
		/// <summary>
		/// List of parameters used by a database call.
		/// </summary>
		ListItems<IDataParameter> Parameters { get; }
		/// <summary>
		/// This event the first event which is called when performing a database operation. Its purpose
		/// is to set default parameter values, modify proposed schema and similar activities.
		/// </summary>
		IServerEvent Preparing { get; }
		/// <summary>
		/// This is is second event which is called when performing a database operation. Implementators should
		/// validate parameter values and should cancel a call if parameters values are incorrect.
		/// </summary>
		IServerEvent Validating { get; }
		/// <summary>
		/// This is a last event called before a database operation is performed. It is past validation process and
		/// implementators should not change the call state at this time.
		/// </summary>
		IServerEvent Executing { get; }
		/// <summary>
		/// This event is triggered before execution is returned to the caller. Implementators can modify result sets at
		/// this point, for example perform calculations on the fields and filter records.
		/// </summary>
		IServerEvent Executed { get; }
		IMetricConfiguration Metrics { get; }
	}
}
