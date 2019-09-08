using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Services;

namespace TomPIT.Middleware
{
	public interface IMiddlewareEntity : IMiddlewareComponent
	{
		T GetValue<T>(string propertyName);
		void SetValue<T>(string propertyName, T value);

		void Invoke(string methodName, params object[] args);
		T Invoke<T>(string methodName, params object[] args);
	}
}
