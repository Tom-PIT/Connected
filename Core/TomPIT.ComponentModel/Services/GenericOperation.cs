﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.Annotations;

namespace TomPIT.Services
{
	public abstract class Operation<TReturnValue> : OperationBase, IOperation<TReturnValue>
	{
		protected Operation(IDataModelContext context) : base(context)
		{

		}

		public string Extender { get; set; }

		public T Invoke<T>()
		{
			var r = Invoke();

			if (r == default)
				return default;

			if (r.GetType().IsCollection())
			{
				var listResult = (IList)r;
				var genericArguments = typeof(T).GenericTypeArguments;
				var result = (IList)typeof(List<>).MakeGenericType(genericArguments).CreateInstance();

				foreach (var item in listResult)
					result.Add(item);

				return (T)result;
			}
			else
				return (T)Convert.ChangeType(r, typeof(T));
		}

		public TReturnValue Invoke()
		{
			ValidateExtender();
			Validate();

			var result = OnInvoke();

			if (result == null)
				return result;

			if (string.IsNullOrWhiteSpace(Extender))
				return result;

			var ext = ResolveExtenderType();
			var extenderInstance = ext.CreateInstance(new object[] { Context });
			var arguments = extenderInstance.GetType().BaseType.GetGenericArguments();


			Type inputType = arguments[0];
			var list = typeof(List<>);
			var genericList = list.MakeGenericType(new Type[] { inputType });
			var method = extenderInstance.GetType().GetMethod("Extend", new Type[] { genericList });

			if (result.GetType().IsCollection())
			{
				var listResult = (IList)result;
				var extenderResult = method.Invoke(extenderInstance, new object[] { result }) as IList;

				listResult.Clear();

				foreach (var i in extenderResult)
					listResult.Add(i);

				return result;
			}
			else
			{
				var listInstance = (IList)genericList.CreateInstance();

				listInstance.Add(result);

				var listResult = (IList)method.Invoke(extenderInstance, new object[] { listInstance });

				return (TReturnValue)listResult[0];
			}
		}

		protected virtual TReturnValue OnInvoke()
		{
			return default;
		}

		private void ValidateExtender()
		{
			if (string.IsNullOrWhiteSpace(Extender))
				return;

			ResolveExtenderType();
		}

		private Type ResolveExtenderType()
		{
			var extenders = GetType().FindAttributes<ExtenderAttribute>();

			if (extenders == null)
				throw new RuntimeException($"{SR.ErrExtenderNotSupported} ({Extender})");

			var extender = extenders.FirstOrDefault(f => string.Compare(f.Extender.ShortName(), Extender, true) == 0);

			if (extender == null)
				throw new RuntimeException($"{SR.ErrExtenderNotSupported} ({Extender})");

			return extender.Extender;
		}
	}
}