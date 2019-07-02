using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.Events;
using TomPIT.Services.Context;

namespace TomPIT.Services
{
	public abstract class Operation : OperationBase, IOperation
	{
		protected Operation(IDataModelContext context) : base(context)
		{

		}

		public void Invoke()
		{
			Validate();
			OnInvoke();
		}

		protected virtual void OnInvoke()
		{

		}
	}
}
