using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace TomPIT.ComponentModel.Apis
{
	public class OperationManifest : IManifest
	{
		private List<OperationParameter> _parameters = null;
		private JContainer _schema = null;

		public List<OperationParameter> Parameters
		{
			get
			{
				if (_parameters == null)
					_parameters = new List<OperationParameter>();

				return _parameters;
			}
		}

		public bool IsDataSource { get; set; }
		public JContainer Schema
		{
			get
			{
				return _schema;
			}
			set
			{
				_schema = value;

				if (!IsDataSource && value != null)
					IsDataSource = true;
			}
		}

		public string MicroService { get; set; }
		public string SchemaName { get; set; }
		public string Name { get; set; }

		public void BindParameters<T>()
		{

		}

		public void BindDataList<T>()
		{

		}

		public void BindDataItem<T>()
		{
			SchemaName = typeof(T).ShortName();
			Schema = new JObject();

			var properties = typeof(T).GetProperties();

			foreach(var property in properties)
			{
				if (property.IsIndexer())
					continue;

				var accessors = property.GetAccessors();
				var getter = accessors.FirstOrDefault(f => f.Name.StartsWith("get"));

				if (getter == null || getter.IsStatic || !getter.IsPublic)
					continue;

				if (property.FindAttribute<JsonIgnoreAttribute>() != null)
					continue;

				if (property.IsPrimitive())
					Schema.Add(new JProperty(property.Name, property.PropertyType.TypeName()));
				else if (property.IsCollection())
				{
					//TODO: implement collection
				}
				else
				{
					//TODO: implement object
				}
			}
		}
	}
}