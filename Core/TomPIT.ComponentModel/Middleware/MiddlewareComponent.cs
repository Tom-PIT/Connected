using Newtonsoft.Json;
using TomPIT.Annotations;
using TomPIT.Services;

namespace TomPIT.Middleware
{
	public abstract class MiddlewareComponent : IMiddlewareComponent
	{
		[JsonIgnore]
		[SkipValidation]
		public IDataModelContext Context { get; set; }

		public MiddlewareComponent()
		{

		}

		public MiddlewareComponent(IDataModelContext context)
		{
			Context = context;
		}
	}
}
