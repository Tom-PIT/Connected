using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Reflection;
using TomPIT.Data;
using TomPIT.Middleware;

namespace TomPIT.Dynamic
{
	public abstract class IoCObject : DynamicObject, IDynamicMetaObjectProvider, IMiddlewareComponent
	{
		private MiddlewareValidator _validator = null;
		private object _instance;
		private Type _instanceType;
		private PropertyInfo[] _instancePropertyInfo;
		public Dictionary<string, object> _properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

		protected IoCObject()
		{
			Initialize(this);
		}

		protected IoCObject(object instance)
		{
			Initialize(instance);
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

		public IMiddlewareContext Context { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		protected virtual void Initialize(object instance)
		{
			_instance = instance;

			if (instance != null)
				_instanceType = instance.GetType();
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
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
				catch (KeyNotFoundException)
				{
					if (GetProperty(_instance, key, out object result))
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

		protected virtual void OnValidating(List<ValidationResult> results)
		{

		}

		protected virtual List<object> OnProvideUniqueValues(string propertyName)
		{
			return null;
		}

		bool IUniqueValueProvider.IsUnique(IMiddlewareContext context, string propertyName)
		{
			return IsValueUnique(propertyName);
		}

		protected virtual bool IsValueUnique(string propertyName)
		{
			return true;
		}

		public void Validate()
		{
			Validator.Validate();
		}

		protected void Validate(object instance)
		{
			Validator.Validate(instance, false);
		}

		private MiddlewareValidator Validator
		{
			get
			{
				if (_validator == null)
				{
					_validator = new MiddlewareValidator(this);
					_validator.SetContext(Context);

					_validator.Validating += OnValidating;
				}

				return _validator;
			}
		}

		private void OnValidating(object sender, List<ValidationResult> results)
		{
			OnValidating(results);
		}
	}
}
