using TomPIT.ComponentModel;

namespace TomPIT.Runtime.ApplicationContextServices
{
	public class DataQualifier : Qualifier
	{
		public DataQualifier(IApplicationContext context, string value) : base(context, value)
		{
		}

		protected override void Initialize()
		{
			if (Context is IApiExecutionScope)
				ParseMicroService(((IApiExecutionScope)Context).Api.MicroService(Context.GetServerContext()));
			else
				ParseMicroService();

			DataSource = Segments[0];
		}

		protected override void Validate()
		{
			if (Segments.Count > 2)
				throw RuntimeException.InvalidQualifier(Context, CreateDescriptor(RuntimeEvents.Runtime), Value, "[microService]/dataSource|transaction");
		}

		public string DataSource { get; private set; }
	}
}
