using Newtonsoft.Json.Linq;
using System.Data;
using TomPIT.Runtime;

namespace TomPIT.ComponentModel.Data
{
	/// <summary>
	/// This are event arguments for database preparing event. Its main purpose is to 
	/// prepare missing parameters and modify schema if necessary.
	/// </summary>
	public class PreparingArguments : CancelEventArguments
	{
		private JObject _arguments = null;
		/// <summary>
		/// Constructor which takes passed parameters and default schema.
		/// </summary>
		/// <remarks>
		/// Changing the schema can cause exceptions to be thrown in later stages if not handled correctly
		/// because reference to thid schema is used through the entire execution process.
		/// </remarks>
		/// <param name="sender">Current application context.</param>
		/// <param name="arguments">Passed arguments from the client.</param>
		/// <param name="schema">Default database schema as defined in Database configuration.</param>
		public PreparingArguments(IApplicationContext sender, JObject arguments, DataTable schema) : base(sender)
		{
			Arguments = arguments;
			Schema = schema;
		}
		/// <summary>
		/// The passed arguments. Can be null.
		/// </summary>
		/// <remarks>
		/// The passed arguments can be completely overwritten but the parameter set must match
		/// with the arguments schema in database configuration.
		/// </remarks>
		public JObject Arguments
		{
			get
			{
				if (_arguments == null)
					_arguments = new JObject();

				return _arguments;
			}
			private set
			{
				_arguments = value;
			}
		}
		/// <summary>
		/// The default schema as defined in the database configuration. can be changed but must be handled carefully because
		/// bound fields will be processed by the data provider.
		/// </summary>
		public DataTable Schema { get; set; }
	}
}
