using Newtonsoft.Json;
using TomPIT.Reflection;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Interop
{
	public abstract class DistributedModelOperation<TModel> : DistributedOperation, IModelOperation<TModel> where TModel : class
	{
		private TModel _model = default;
		protected DistributedModelOperation([CIP(CIP.ApiOperationProvider)]string callbackPath) : base(callbackPath)
		{
		}
		[JsonIgnore]
		protected TModel Model
		{
			get
			{
				if (_model == default)
				{
					_model = typeof(TModel).CreateInstance<TModel>();

					if (_model is IMiddlewareObject mo)
						mo.SetContext(Context);
				}

				return _model;
			}
		}
	}
}
