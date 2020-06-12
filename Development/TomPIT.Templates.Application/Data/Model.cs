using System;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Data
{
	[Create(DesignUtils.ComponentModel)]
	[Syntax(SyntaxAttribute.CSharp)]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	public class Model : SourceCodeConfiguration, IModelConfiguration
	{
		private ListItems<IModelOperation> _operations = null;

		[Items(DesignUtils.ModelOperationItems)]
		public ListItems<IModelOperation> Operations
		{
			get
			{
				if (_operations == null)
					_operations = new ListItems<IModelOperation> { Parent = this };

				return _operations;
			}
		}

		[Items(DesignUtils.ConnectionListItems)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		public Guid Connection { get; set; }
	}
}
