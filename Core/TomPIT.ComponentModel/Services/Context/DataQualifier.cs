using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Exceptions;

namespace TomPIT.Services.Context
{
	public class DataQualifier : Qualifier
	{
		public DataQualifier(IExecutionContext context, string value) : base(context, value)
		{
		}

		protected override void Initialize()
		{
			if (Context is IApiExecutionScope)
				ParseMicroService(((IApiExecutionScope)Context).Api.MicroService(Context.Connection()));
			else
				ParseMicroService();

			DataSource = Segments[0];
		}

		protected override void Validate()
		{
			if (Segments.Count > 2)
				throw ExecutionException.InvalidQualifier(Context, CreateDescriptor(ExecutionEvents.Runtime), Value, "[microService]/dataSource|transaction");
		}

		public string DataSource { get; private set; }
	}
}
