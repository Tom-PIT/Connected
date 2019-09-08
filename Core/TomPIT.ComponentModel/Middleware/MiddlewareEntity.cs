using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using TomPIT.Annotations;
using TomPIT.Services;

namespace TomPIT.Middleware
{
	public abstract class MiddlewareEntity : DynamicObject, IMiddlewareEntity, IDynamicMetaObjectProvider
	{
		private object _instance;
		private Type _instanceType;
		private PropertyInfo[] _instancePropertyInfo;
		public Dictionary<string, object> _properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

		public MiddlewareEntity()
		{
			Initialize(this);
		}

		public MiddlewareEntity(object instance)
		{
			Initialize(instance);
		}

		T IMiddlewareEntity.GetValue<T>(string propertyName)
		{
			var property = ResolveProperty(this, propertyName.Split('.'));

			return (T)property.Item1.GetValue(property.Item2);
		}

		void IMiddlewareEntity.Invoke(string methodName, params object[] args)
		{
			var method = ResolveMethod(methodName);

			method.Item1.Invoke(method.Item2, args);
		}

		T IMiddlewareEntity.Invoke<T>(string methodName, params object[] args)
		{
			var method = ResolveMethod(methodName);

			var r = method.Item1.Invoke(method.Item2, args);

			return Types.Convert<T>(r);
		}

		void IMiddlewareEntity.SetValue<T>(string propertyName, T value)
		{
			var property = ResolveProperty(this, propertyName.Split('.'));

			if (property.Item1 == null)
				return;

			property.Item1.SetValue(property.Item2, value);
		}

		private (MethodInfo, object) ResolveMethod(string methodName)
		{
			var tokens = methodName.Split('.');
			object target;
			MethodInfo method;

			if (tokens.Length == 1)
			{
				method = GetType().GetMethod(methodName);
				target = this;
			}
			else
			{
				var property = ResolveProperty(this, tokens.SkipLast(1).ToArray());
				var propertyValue = property.Item1.GetValue(property.Item2);

				if (propertyValue == null)
					throw new RuntimeException($"{SR.ErrPropertyValueNull} ({property.Item1.Name})");

				method = propertyValue.GetType().GetMethod(tokens.Last());
				target = propertyValue;
			}

			if (method == null)
				throw new RuntimeException($"{SR.ErrMethodNotFound} ({tokens[tokens.Length - 1]})");

			return (method, target);
		}

		private (PropertyInfo, object) ResolveProperty(object instance, string[] tokens)
		{
			var property = instance.GetType().GetProperty(tokens[0]);

			if (property == null)
				throw new RuntimeException($"{SR.ErrPropertyNotFound} ({tokens[0]})");

			if (tokens.Length == 1)
				return (property, instance);

			var value = property.GetValue(instance);

			if (value == null)
				throw new RuntimeException($"{SR.ErrPropertyValueNull} ({tokens[0]})");

			return ResolveProperty(value, tokens.Skip(1).ToArray());
		}

		private PropertyInfo[] InstancePropertyInfo
		{
			get
			{
				if (_instancePropertyInfo == null && _instance != null)
					_instancePropertyInfo = _instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
				return _instancePropertyInfo;
			}
		}

		[JsonIgnore]
		[SkipValidation]
		public IDataModelContext Context { get; set; }

		protected virtual void Initialize(object instance)
		{
			_instance = instance;

			if (instance != null)
				_instanceType = instance.GetType();
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = null;

			if (_properties.ContainsKey(binder.Name))
			{
				result = _properties[binder.Name];
				return true;
			}

			if (_instance != null)
			{
				try
				{
					return GetProperty(_instance, binder.Name, out result);
				}
				catch { }
			}

			result = null;

			return false;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			if (_instance != null)
			{
				try
				{
					bool result = SetProperty(_instance, binder.Name, value);
					if (result)
						return true;
				}
				catch { }
			}

			_properties[binder.Name] = value;

			return true;
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			if (_instance != null)
			{
				try
				{
					if (InvokeMethod(_instance, binder.Name, args, out result))
						return true;
				}
				catch { }
			}

			result = null;
			return false;
		}

		protected bool GetProperty(object instance, string name, out object result)
		{
			if (instance == null)
				instance = this;

			var miArray = _instanceType.GetMember(name, BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance);
			if (miArray != null && miArray.Length > 0)
			{
				var mi = miArray[0];
				if (mi.MemberType == MemberTypes.Property)
				{
					result = ((PropertyInfo)mi).GetValue(instance, null);
					return true;
				}
			}

			result = null;
			return false;
		}

		protected bool SetProperty(object instance, string name, object value)
		{
			if (instance == null)
				instance = this;

			var miArray = _instanceType.GetMember(name, BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance);
			if (miArray != null && miArray.Length > 0)
			{
				var mi = miArray[0];
				if (mi.MemberType == MemberTypes.Property)
				{
					((PropertyInfo)mi).SetValue(_instance, value, null);
					return true;
				}
			}
			return false;
		}

		protected bool InvokeMethod(object instance, string name, object[] args, out object result)
		{
			if (instance == null)
				instance = this;

			// Look at the instanceType
			var miArray = _instanceType.GetMember(name,
											BindingFlags.InvokeMethod |
											BindingFlags.Public | BindingFlags.Instance);

			if (miArray != null && miArray.Length > 0)
			{
				var mi = miArray[0] as MethodInfo;
				result = mi.Invoke(_instance, args);
				return true;
			}

			result = null;
			return false;
		}

		public object this[string key]
		{
			get
			{
				try
				{
					return _properties[key];
				}
				catch (KeyNotFoundException ex)
				{
					object result = null;
					if (GetProperty(_instance, key, out result))
						return result;

					throw;
				}
			}
			set
			{
				if (_properties.ContainsKey(key))
				{
					_properties[key] = value;
					return;
				}

				var miArray = _instanceType.GetMember(key, BindingFlags.Public | BindingFlags.GetProperty);

				if (miArray != null && miArray.Length > 0)
					SetProperty(_instance, key, value);
				else
					_properties[key] = value;
			}
		}


		public IEnumerable<KeyValuePair<string, object>> GetProperties(bool includeInstanceProperties = false)
		{
			if (includeInstanceProperties && _instance != null)
			{
				foreach (var prop in this.InstancePropertyInfo)
					yield return new KeyValuePair<string, object>(prop.Name, prop.GetValue(_instance, null));
			}

			foreach (var key in this._properties.Keys)
				yield return new KeyValuePair<string, object>(key, _properties[key]);

		}

		public bool Contains(KeyValuePair<string, object> item, bool includeInstanceProperties = false)
		{
			bool res = _properties.ContainsKey(item.Key);
			if (res)
				return true;

			if (includeInstanceProperties && _instance != null)
			{
				foreach (var prop in this.InstancePropertyInfo)
				{
					if (prop.Name == item.Key)
						return true;
				}
			}

			return false;
		}
	}
}