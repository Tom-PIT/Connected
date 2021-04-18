using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Data
{
	[Create(DesignUtils.ComponentModel)]
	[Syntax(SyntaxAttribute.CSharp)]
	[DomDesigner(DomDesignerAttribute.TextDesigner, AmbientProvider = "TomPIT.MicroServices.Design.Designers.ModelAmbientProvider, TomPIT.MicroServices.Design")]
	[ComponentCreatedHandler("TomPIT.MicroServices.Design.CreateHandlers.Model, TomPIT.MicroServices.Design")]
	[ClassRequired]
	public class Model : TextConfiguration, IModelConfiguration
	{
		private ListItems<IModelOperation> _operations = null;
		private ListItems<IModelOperation> _views = null;

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

		[Items(DesignUtils.ModelViewItems)]
		public ListItems<IModelOperation> Views
		{
			get
			{
				if (_views == null)
					_views = new ListItems<IModelOperation> { Parent = this };

				return _views;
			}
		}

		[Items(DesignUtils.ConnectionListItems)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		public Guid Connection { get; set; }
		[Browsable(false)]
		public override string FileName => $"{ToString()}.csx";
	}
}
