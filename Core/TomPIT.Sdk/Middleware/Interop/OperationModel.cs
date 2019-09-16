using TomPIT.Reflection;

namespace TomPIT.Middleware.Interop
{
	public abstract class OperationModel<TModel> : Operation, IOperationModel<TModel> where TModel : class
	{
		private TModel _model = default;

		protected TModel Model
		{
			get
			{
				if (_model == default)
					_model = typeof(TModel).CreateInstance<TModel>(new object[] { Context });

				return _model;
			}
		}
	}
}
