using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Data;
using TomPIT.Design;
using TomPIT.Ide;
using TomPIT.Runtime;

namespace TomPIT.Dom
{
	internal class DataManagementItemElement : Element
	{
		private IDomDesigner _designer = null;
		private ExistingItems _ds = null;
		private PropertyInfo _property = null;

		private class ExistingItems
		{
			private List<DataManagementRecord> _items = null;

			//[Items(typeof(MicroServicesCollection))]
			[Browsable(false)]
			public List<DataManagementRecord> Items
			{
				get
				{
					if (_items == null)
						_items = new List<DataManagementRecord>();

					return _items;
				}
			}
		}

		public DataManagementItemElement(IEnvironment environment, IDomElement parent, IDataManagementItem item) : base(environment, parent)
		{
			Title = item.Name;
			Id = item.Id.ToString();
			Item = item;
		}

		private IDataManagementItem Item { get; }

		public override PropertyInfo Property
		{
			get
			{
				if (_property == null)
					_property = DataManagementRecords.GetType().GetProperty("Items");

				return _property;
			}
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
				{
					if (Item is IDataManagementDescriptor)
						_designer = new DataManagementItemDesigner(Environment, this);
				}

				return _designer;
			}
		}

		private ExistingItems DataManagementRecords
		{
			get
			{
				if (_ds == null)
				{
					_ds = new ExistingItems();

					var i = Item as IDataManagementDescriptor;
					var ctx = ApplicationContext.NonHttpContext(SysContext.Url, "Management", null, DomQuery.Closest<IMicroServiceScope>(this).MicroService.Token.AsString());

					//var items = ctx.Services.Data.Read<JObject>(i.DataSource).Value<JArray>("data");

					//foreach (var j in items)
					//{
					//	var r = new DataManagementRecord
					//	{
					//		Component = j as JObject,
					//		Text = "Hello"
					//	};

					//	_ds.Items.Add(r);
					//}
				}

				return _ds;
			}
		}

		public List<DataManagementRecord> Existing { get { return DataManagementRecords.Items; } }
	}
}
