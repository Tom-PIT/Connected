using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	internal class DataManagementElement : Element
	{
		private IDataManagement _dataManagement = null;

		public DataManagementElement(IEnvironment environment, IDomElement parent, IComponent component) : base(environment, parent)
		{
			Title = component.Name;
			Id = component.Token.ToString();
			DataManagementComponent = component;
		}

		private IComponent DataManagementComponent { get; }
		public override bool HasChildren => DataManagement != null && DataManagement.Items.Count > 0;
		public override int ChildrenCount => DataManagement == null ? 0 : DataManagement.Items.Count;

		public override void LoadChildren()
		{
			if (DataManagement == null)
				return;

			foreach (var i in DataManagement.Items)
				Items.Add(new DataManagementItemElement(Environment, this, i));
		}

		public override void LoadChildren(string id)
		{
			var d = DataManagement.Items.FirstOrDefault(f => f.Id == id.AsGuid());

			if (d != null)
				Items.Add(new DataManagementItemElement(Environment, this, d));
		}

		public IDataManagement DataManagement
		{
			get
			{
				if (_dataManagement == null)
					_dataManagement = SysContext.GetService<IComponentService>().SelectConfiguration(DataManagementComponent.Token) as IDataManagement;

				return _dataManagement;
			}
		}
	}
}
