using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Exceptions;

namespace TomPIT.Reflection
{
	public static class TypeExtensions
	{
		private static Dictionary<Type, string> Types = new Dictionary<Type, string>
		{
			{typeof(int), "int"},
			{typeof(uint), "uint"},
			{typeof(long), "long"},
			{typeof(ulong), "ulong"},
			{typeof(short), "short"},
			{typeof(ushort), "ushort"},
			{typeof(byte), "byte"},
			{typeof(sbyte), "sbyte"},
			{typeof(bool), "bool"},
			{typeof(float), "float"},
			{typeof(double), "double"},
			{typeof(decimal), "decimal"},
			{typeof(char), "char"},
			{typeof(string), "string"},
			{typeof(object), "object"},
			{typeof(void), "void"}
		};
		public static T FindAttribute<T>(this PropertyInfo info) where T : Attribute
		{
			var atts = info.GetCustomAttributes<T>(true);

			if (atts == null || atts.Count() == 0)
				return null;

			return atts.ElementAt(0);
		}

		public static List<T> FindAttributes<T>(this PropertyInfo info) where T : Attribute
		{
			var atts = info.GetCustomAttributes<T>(true);

			if (atts == null || atts.Count() == 0)
				return new List<T>();

			return atts.ToList<T>();
		}

		public static List<T> FindAttributes<T>(this Type type) where T : Attribute
		{
			var r = type.GetCustomAttributes<T>(true);

			if (r == null)
				return new List<T>();

			return r.ToList<T>();
		}

		public static T FindAttribute<T>(this Type type) where T : Attribute
		{
			var atts = type.GetCustomAttributes<T>(true);

			if (atts == null || atts.Count() == 0)
				return null;

			return atts.ElementAt(0);
		}

		public static bool IsPrimitive(this PropertyInfo pi)
		{
			if (pi == null)
				return false;

			return pi.PropertyType.IsTypePrimitive();
		}

		public static bool IsTypePrimitive(this Type type)
		{
			if (type == null)
				return false;

			return type.IsPrimitive
						|| type == typeof(string)
						|| type.IsEnum
						|| type.IsValueType;
		}

		public static bool IsCollection(this Type type)
		{
			var interfaces = type.GetInterfaces();

			if (interfaces == null || interfaces.Length == 0)
				return false;

			return interfaces.Contains(typeof(IEnumerable));
		}

		public static bool IsCollection(this PropertyInfo pi)
		{
			if (pi == null)
				return false;

			return IsCollection(pi.PropertyType);
		}

		public static bool IsIndexer(this PropertyInfo pi)
		{
			return pi.GetIndexParameters().Length > 0;
		}

		public static object CreateInstance(this Type type, object[] ctorArgs)
		{
			if (type == null)
				return null;

			if (ctorArgs == null)
				return CreateInstanceInternal(type);
			else
				return CreateInstanceInternal(type, BindingFlags.CreateInstance, ctorArgs);
		}

		public static T CreateInstance<T>(this Type type, object[] ctorArgs) where T : class
		{
			if (type == null)
				return default;

			T instance = null;

			if (ctorArgs == null)
				instance = CreateInstanceInternal(type) as T;
			else
				instance = CreateInstanceInternal(type, BindingFlags.CreateInstance, ctorArgs) as T;

			if (instance == null)
				throw new TomPITException(string.Format("{0} ({1})", SR.ErrInvalidInterface, typeof(T).Name));

			return instance;
		}

		public static object CreateInstance(this Type type)
		{
			return CreateInstanceInternal(type);
		}

		public static T CreateInstance<T>(this Type type) where T : class
		{
			return CreateInstance<T>(type, null);
		}

		private static object CreateInstanceInternal(this Type type, BindingFlags bindingFlags, object[] ctorArgs)
		{
			if (type.IsTypePrimitive())
				return type.GetDefaultValue();

			return Activator.CreateInstance(type, bindingFlags, null, ctorArgs, CultureInfo.InvariantCulture);
		}

		private static object CreateInstanceInternal(this Type type)
		{
			if (type.IsTypePrimitive())
				return type.GetDefaultValue();

			return Activator.CreateInstance(type);
		}

		public static object GetDefaultValue(this Type type)
		{
			if (type == null || type == typeof(void))
				return null;

			if (type == typeof(string))
				return string.Empty;
			else if (type == typeof(decimal))
				return decimal.Zero;

			if (!type.IsValueType)
				return null;

			if (type.ContainsGenericParameters)
				throw new TomPITException(string.Format("{{0}} {1} ({2})", MethodBase.GetCurrentMethod(), SR.ErrDefaultGeneric, type));

			if (type.IsPrimitive || !type.IsNotPublic)
			{
				try
				{
					return Activator.CreateInstance(type);
				}
				catch (Exception e)
				{
					throw new TomPITException(string.Format("{{0}} {1} ({2})", MethodBase.GetCurrentMethod(), SR.ErrActivatorCreateInstance, e.Message));
				}
			}

			throw new TomPITException(string.Format("{{0}}, {1} ({2})", MethodBase.GetCurrentMethod(), SR.ErrTypeInternal, type));
		}

		public static bool ImplementsInterface(this Type type, Type itf)
		{
			return type.GetInterface(itf.FullName) != null;
		}

		public static bool ImplementsInterface<T>(this Type type)
		{
			return type.GetInterface(typeof(T).FullName) != null;
		}

		public static bool IsDesignable(this PropertyInfo pi)
		{
			if (pi.IsIndexer())
				return false;

			if (IsPrimitive(pi) || IsEditable(pi))
				return false;

			if (pi.PropertyType.ImplementsInterface<IEvent>())
				return false;

			return true;
		}

		public static bool IsEditable(this PropertyInfo pi)
		{
			return IsBrowsable(pi) && (IsPrimitive(pi) || pi.PropertyType.IsValueType);
		}

		public static bool IsBrowsable(this PropertyInfo pi)
		{
			if (pi == null)
				return false;

			var atts = pi.GetCustomAttributes(typeof(BrowsableAttribute), true);

			if (atts == null || atts.Length == 0)
				return true;

			foreach (object i in atts)
			{
				var a = i as BrowsableAttribute;

				if (a == null)
					continue;

				if (!a.Browsable)
					return false;
			}

			return true;
		}

		public static bool IsText(this PropertyInfo pi)
		{
			return typeof(IText).IsAssignableFrom(pi.PropertyType) ||
				 ImplementsInterface(typeof(IText), pi.PropertyType);
		}

		public static bool IsText(this Type type)
		{
			return typeof(IText).IsAssignableFrom(type) ||
				 ImplementsInterface(typeof(IText), type);
		}

		public static string Encode(this Type type)
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(type.AssemblyQualifiedName));
		}

		public static string ShortName(this Type type)
		{
			var r = type.Name;

			if (r.Contains("."))
				r = r.Substring(r.LastIndexOf(".") + 1);

			return r;
		}

		public static string TypeName(this Type type)
		{
			if (type == null)
				return null;

			return string.Format("{0}, {1}", type.FullName, type.Assembly.GetName().Name);
		}

		public static string FullTypeName(this Type type)
		{
			if (type == null)
				return null;

			return string.Format("{0}, {1}", type.FullName, type.Assembly.GetName().FullName);
		}

		public static string ScriptTypeName(this Type type)
		{
			if (!type.FullName.Contains("+"))
				return type.ShortName();

			var tokens = type.FullName.Split('+');

			return tokens[tokens.Length - 1];
		}

		public static Assembly LoadAssembly(string type)
		{
			var tokens = type.Split(',');
			var libraryName = string.Empty;
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			if (tokens.Length == 1)
				libraryName = string.Format("{0}.dll", tokens[0].Trim());
			else if (tokens.Length > 1)
				libraryName = string.Format("{0}.dll", tokens[1].Trim());

			var file = string.Format("{0}\\{1}", path, libraryName);

			if (!File.Exists(file))
				return null;

			return AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(file));
		}

		public static string ToFriendlyName(this Type type)
		{
			if (Types.ContainsKey(type))
				return Types[type];
			else if (type.IsGenericType)
				return type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(x => ToFriendlyName(x)).ToArray()) + ">";
			else
				return type.ShortName();
		}

		public static Type GetType(string type)
		{
			if (Types.ContainsValue(type))
				return Types.FirstOrDefault(f => string.Compare(f.Value, type, false) == 0).Key;

			var t = Type.GetType(type);

			if (t != null)
				return t;

			var asm = LoadAssembly(type);

			if (asm == null)
				return null;

			var typeName = type.Split(',')[0];

			return asm.GetType(typeName);
		}

		public static string InvariantTypeName(this Assembly assembly, string fullName)
		{
			string[] tokens = fullName.Split(',');

			if (tokens == null || tokens.Length == 0)
				return null;

			return string.Format("{0}, {1}", tokens[0].Trim(), assembly.FullName);
		}

		public static string ShortName(this Assembly assembly)
		{
			return assembly.FullName.Split(',')[0];
		}

		public static bool IsNumericType(this Type type)
		{
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Single:
					return true;
				default:
					return false;
			}
		}

		public static object DefaultValue(this Type type)
		{
			if (type.IsValueType)
				return Activator.CreateInstance(type);

			return null;
		}

		public static T Closest<T>(this IElement instance)
		{
			if (instance == null)
				return default(T);

			if (instance is T || instance.GetType().IsAssignableFrom(typeof(T)))
				return (T)instance;

			if (!(instance is IElement e))
				return default;

			return Closest<T>(e.Parent);
		}
	}
}
