using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Services
{
	public abstract class OperationModel<TModel> : Operation , IOperationModel<TModel> where TModel : class
	{
		private TModel _model = default;
		protected OperationModel(IDataModelContext context) : base(context)
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
