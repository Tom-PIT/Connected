using TomPIT.ComponentModel.Apis;

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
				throw new RuntimeException(string.Format("{0} ({1}). {2}: {3}.", SR.ErrInvalidQualifier, Value, SR.ErrInvalidQualifierExpected, "[microService]/dataSource|transaction")).WithMetrics(Context);
		}

		public string DataSource { get; private set; }
	}
}
