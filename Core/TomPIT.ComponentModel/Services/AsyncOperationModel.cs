using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Services
{
	public abstract class AsyncOperationModel<TModel> : AsyncOperation, IOperationModel<TModel> where TModel : class
	{
		private TModel _model = default;
		protected AsyncOperationModel(IDataModelContext context, string asyncPath) : base(context, asyncPath)
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
