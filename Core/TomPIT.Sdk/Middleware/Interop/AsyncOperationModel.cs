using TomPIT.Reflection;

namespace TomPIT.Middleware.Interop
{
	public abstract class AsyncOperationModel<TModel> : DistributedOperation, IOperationModel<TModel> where TModel : class
	{
		private TModel _model = default;
		protected AsyncOperationModel(string asyncPath) : base(asyncPath)
		{
		}

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
