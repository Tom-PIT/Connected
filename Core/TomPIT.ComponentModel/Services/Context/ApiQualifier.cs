using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;

namespace TomPIT.Services.Context
{
	public class ApiQualifier : Qualifier
	{
		public ApiQualifier(IExecutionContext context, string api) : base(context, api)
		{
		}


		public ApiQualifier(IExecutionContext context, IMicroService microService, string api) : base(context, microService, api)
		{

		}

		public bool ExplicitIdentifier { get; private set; }

		protected override void Initialize()
		{
			if (Segments.Count > 2)
			{
				ExplicitIdentifier = true;
				ParseMicroService(Segments[0]);
			}
			else
			{
				if (Context is IApiExecutionScope)
					ParseMicroService(((IApiExecutionScope)Context).Api.MicroService(Context.Connection()));
				else
					ParseMicroService();
			}

			if (Segments.Count == 3)
			{
				Api = Segments[1];
				Operation = Segments[2];
			}
			else
			{
				if (Segments.Count == 1)
				{
					if (Context is IApiExecutionScope)
					{
						Api = ((IApiExecutionScope)Context).Api.ComponentName(Context);

						Operation = Segments[0];
					}
				}
				else
				{
					Api = Segments[0];
					Operation = Segments[1];
				}
			}
		}

		protected override void Validate()
		{
			if (Segments.Count > 3 || Segments.Count < 1)
				throw new RuntimeException(string.Format("{0} ({1}). {2}: {3}.", SR.ErrInvalidQualifier, Value, SR.ErrInvalidQualifierExpected, "[microService]/api/operation"));
		}

		public string Api { get; private set; }
		public string Operation { get; private set; }
	}
}
