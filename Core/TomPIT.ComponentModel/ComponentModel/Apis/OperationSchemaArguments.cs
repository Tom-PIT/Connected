using TomPIT.Services;

namespace TomPIT.ComponentModel.Apis
{
	public class OperationSchemaArguments : OperationArguments
	{
		private OperationSchema _schema = null;

		public OperationSchemaArguments(IExecutionContext sender, IApiOperation operation) : base(sender, operation, null)
		{
		}

		public OperationSchema Schema
		{
			get
			{
				if (_schema == null)
					_schema = new OperationSchema();

				return _schema;
			}
		}
	}
}
