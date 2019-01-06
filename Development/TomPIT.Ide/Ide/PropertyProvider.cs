using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Design.Validation;

namespace TomPIT.Ide
{
	internal class PropertyProvider : EnvironmentClient, IPropertyProvider
	{
		private List<IProperty> _props = null;

		public PropertyProvider(IEnvironment environment) : base(environment)
		{

		}

		public List<string> Categories
		{
			get
			{
				if (Properties == null)
					return null;

				var r = Properties.Select(f => f.Category).Distinct().ToList();

				r.Sort();

				return r;
			}
		}

		public List<IProperty> QueryProperties(string category)
		{
			if (Properties == null)
				return null;

			if (string.IsNullOrWhiteSpace(category))
				return Properties;
			else
				return Properties.Where(f => string.Compare(f.Category, category, true) == 0).ToList();

		}

		private List<IProperty> Properties
		{
			get
			{
				if (_props == null)
				{
					if (Environment.Selected() == null)
						return null;

					_props = QueryPropertyValues();
				}

				return _props;
			}
		}

		private List<IProperty> QueryPropertyValues()
		{
			if (Environment.Selected().Property != null
				&& !Environment.Selected().Property.ChildrenBrowsable())
				return null;

			var r = new List<IProperty>();

			if (Environment.Selection.Element is IPropertySource source)
			{
				var instances = source.PropertySources;

				if (instances != null)
				{
					foreach (var i in instances)
						QueryPropertyValues(i, r);
				}
			}
			else
				QueryPropertyValues(Environment.Selection.Element.Value, r);

			if (r.Count > 0)
				return r.OrderBy(f => f.Ordinal).ThenBy(f => f.Category).ThenBy(f => f.Name).ToList();

			return r;
		}

		private void QueryPropertyValues(object instance, List<IProperty> items)
		{
			if (instance == null)
				return;

			var element = instance as IElement;
			var props = GetProperties(instance, false);

			if (props == null)
				return;

			foreach (var i in props)
			{
				if (!i.CanWrite)
				{
					var isBrowsable = i.FindAttribute<BrowsableAttribute>();

					if (isBrowsable == null || !isBrowsable.Browsable)
						continue;
				}

				var editable = i.IsEditable();

				if (!editable)
					continue;

				if (instance.GetType().IsSuppressed(i))
					continue;

				if (Environment.Selection.Designer != null
					&& !Environment.Selection.Designer.IsPropertyEditable(i.Name))
					continue;

				var v = new Property(Environment, Environment.Selected());

				DiscoverProperty(instance, v, i);

				var sysValue = i.GetValue(instance);

				ResolveValue(element == null ? Guid.Empty : element.Id, v, i.Name, sysValue);

				items.Add(v);
			}
		}

		private void ResolveValue(Guid id, Property value, string propertyName, object val)
		{
			if (value.IsLocalizable && Environment.Globalization.LanguageToken != Guid.Empty)
			{
				var ss = Shell.GetService<IMicroServiceService>().SelectString(Environment.Context.MicroService(), Environment.Globalization.LanguageToken, id, propertyName);

				if (ss != null)
					value.Value = ss;
			}

			if (value.Value == null)
				value.Value = val;
		}

		private void DiscoverProperty(object instance, Property val, PropertyInfo property)
		{
			var cat = property.FindAttribute<PropertyCategoryAttribute>();

			if (cat != null)
				val.Category = cat.GetValue();

			var display = property.FindAttribute<DisplayAttribute>();

			if (display != null)
			{
				val.Text = display.Name;
				val.Ordinal = display.Order;
				val.Description = display.Description;
			}
			else
				val.Text = StringUtils.InsertSpaces(property.Name);

			var displayFormat = property.FindAttribute<DisplayFormatAttribute>();

			if (displayFormat != null)
				val.Format = displayFormat.DataFormatString;

			if (string.IsNullOrWhiteSpace(val.Category))
				val.Category = SR.CatMisc;

			val.Name = property.Name;
			val.Type = property.PropertyType;

			SetupPropertyEditor(property, val);

			var latt = property.FindAttribute<LocalizableAttribute>();

			val.IsLocalizable = latt != null && latt.IsLocalizable;

			DiscoverValidation(val, property);

			var helpLink = property.FindAttribute<HelpLinkAttribute>();

			if (helpLink != null && !string.IsNullOrWhiteSpace(helpLink.Url))
				val.HelpLink = helpLink.Url;

			var obsolete = property.FindAttribute<ObsoleteAttribute>();

			if (obsolete != null)
				val.Obsolete = true;

			var tz = property.FindAttribute<SupportsTimezone>();

			if (tz != null)
				val.SupportsTimezone = true;
		}

		private void DiscoverValidation(Property val, PropertyInfo property)
		{
			DiscoverRequired(val, property);
			DiscoverMaxLength(val, property);
			DiscoverMinValue(val, property);
			DiscoverMaxValue(val, property);
		}

		private void DiscoverRequired(Property val, PropertyInfo property)
		{
			var vs = val.Validation as PropertyValidation;
			var rq = property.FindAttribute<RequiredAttribute>();

			if (rq != null)
			{
				var rv = vs.Required as RequiredValidation;

				rv.IsRequired = true;
				rv.ErrorText = rq.FormatErrorMessage(val.Text);
			}
		}

		private void DiscoverMaxLength(Property val, PropertyInfo property)
		{
			var vs = val.Validation as PropertyValidation;

			var mla = property.FindAttribute<MaxLengthAttribute>();

			if (mla != null)
			{
				var mlva = vs.MaxLength as MaxLengthValidation;

				mlva.MaxLength = mla.Length;
				mlva.ErrorText = mla.FormatErrorMessage(val.Text);
			}
		}

		private void DiscoverMinValue(Property val, PropertyInfo property)
		{
			var vs = val.Validation as PropertyValidation;
			var ml = property.FindAttribute<MinValueAttribute>();
			var mlv = vs.MinValue as MinValueValidation;

			if (ml != null)
			{
				mlv.Value = ml.Value;
				mlv.ErrorText = ml.FormatErrorMessage(val.Text);
			}
			else
				mlv.Value = double.NaN;
		}

		private void DiscoverMaxValue(Property val, PropertyInfo property)
		{
			var vs = val.Validation as PropertyValidation;
			var ml = property.FindAttribute<MaxValueAttribute>();
			var mlv = vs.MaxValue as MaxValueValidation;

			if (ml != null)
			{
				mlv.Value = ml.Value;
				mlv.ErrorText = ml.FormatErrorMessage(val.Text);
			}
			else
				mlv.Value = double.NaN;
		}

		private void SetupPropertyEditor(PropertyInfo pi, Property value)
		{
			IPropertyEditor editor = null;

			var readOnly = pi.FindAttribute<ReadOnlyAttribute>();

			if (!pi.CanWrite || (readOnly != null && readOnly.IsReadOnly))
				editor = FindEditorByName("Label");
			else
			{
				var att = pi.FindAttribute<PropertyEditorAttribute>();

				if (att == null || string.IsNullOrWhiteSpace(att.Name))
					editor = FindEditorByType(pi);
				else
				{
					editor = Connection.GetService<IDesignerService>().GetPropertyEditor(att.Name);

					if (editor == null)
						editor = FindDefaultEditor();
				}

				if (editor == null)
					editor = FindDefaultEditor();
			}

			value.Editor = editor.View;
		}

		private IPropertyEditor FindEditorByName(string name)
		{
			return Connection.GetService<IDesignerService>().GetPropertyEditor(name);
		}

		private IPropertyEditor FindDefaultEditor()
		{
			return FindEditorByName("Text");
		}

		private IPropertyEditor FindEditorByType(PropertyInfo pi)
		{
			if (pi.PropertyType == typeof(string) || pi.PropertyType == typeof(char))
				return FindEditorByName("Text");
			else if (pi.PropertyType == typeof(int) || pi.PropertyType == typeof(uint)
					  || pi.PropertyType == typeof(byte) || pi.PropertyType == typeof(sbyte)
					  || pi.PropertyType == typeof(short) || pi.PropertyType == typeof(ushort)
					  || pi.PropertyType == typeof(float) || pi.PropertyType == typeof(float)
					  || pi.PropertyType == typeof(long) || pi.PropertyType == typeof(long)
					  || pi.PropertyType == typeof(double) || pi.PropertyType == typeof(double)
					  || pi.PropertyType == typeof(decimal) || pi.PropertyType == typeof(decimal))
				return FindEditorByName("Number");
			if (pi.PropertyType == typeof(bool))
				return FindEditorByName("Check");
			else if (pi.PropertyType.IsEnum)
				return FindEditorByName("Select");
			else if (pi.PropertyType == typeof(DateTime))
				return FindEditorByName("Date");
			else if (pi.PropertyType == typeof(Color))
				return FindEditorByName("Color");
			else
				return FindEditorByName("Text");
		}

		private PropertyInfo[] GetProperties(object instance)
		{
			return GetProperties(instance, true);
		}

		private PropertyInfo[] GetProperties(object instance, bool writableOnly)
		{
			if (writableOnly)
				return instance.GetType().GetProperties(BindingFlags.Public
						  | BindingFlags.SetProperty
						  | BindingFlags.Instance);
			else
				return instance.GetType().GetProperties(BindingFlags.Public
						  | BindingFlags.Instance);
		}
	}
}