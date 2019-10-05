using Newtonsoft.Json;
using TomPIT.Reflection;

namespace TomPIT.Middleware.Interop
{
	public abstract class ModelOperation<TModel, TReturnValue> : Operation<TReturnValue>, IModelOperation<TModel> where TModel : class
	{
		private TModel _model = default;
		protected ModelOperation()
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
