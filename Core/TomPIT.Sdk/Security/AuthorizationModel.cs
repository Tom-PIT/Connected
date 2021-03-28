using System;
using TomPIT.Middleware.Interop;

namespace TomPIT.Security
{
	public abstract class AuthorizationModel : MiddlewareProxy, IAuthorizationModel
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

		protected override object ProxyTarget => AuthorizationTarget;
	}
}
