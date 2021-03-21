using System;
using TomPIT.Middleware;

namespace TomPIT.Security
{
	public abstract class AuthorizationModel : MiddlewareObject, IAuthorizationModel
	{
		private object _target = null;

		public object AuthorizationTarget
		{
			get => _target;
			set
			{
				if (_target == value)
					return;

				_target = value;
				
				OnTargetChanged();
			}
		}
		 
		protected virtual void OnTargetChanged()
		{

		}

		[Obsolete("Please use GetValue().")]
		public T GetValueFromTarget<T>(string propertyName)
		{
			return GetValue<T>(propertyName);
		}

		public T GetValue<T>(string propertyName)
		{
			return SecurityExtensions.GetProxyValue<T>(this, propertyName);
		}

		public bool IsDefined(string propertyName)
		{
			return SecurityExtensions.IsProxyPropertyDefined(this, propertyName);
		}

		public bool ContainsValue<T>(string propertyName)
		{
			return SecurityExtensions.ContainsProxyValue<T>(this, propertyName);
		}
	}
}
