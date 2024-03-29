﻿using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.MicroServices.BigData.Design;
using TomPIT.Runtime;

namespace TomPIT.MicroServices.BigData
{
	[DomDesigner(DomDesignerAttribute.TextDesigner, Mode = EnvironmentMode.Design)]
	[DomDesigner(DesignUtils.BigDataPartitionDesigner, Mode = EnvironmentMode.Runtime)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ClassRequired]
	public class Partition : TextConfiguration, IPartitionConfiguration
	{
		private ListItems<IBigDataQuery> _queries = null;

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Namespace { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		[DefaultValue(SchemaSynchronizationMode.Manual)]
		public SchemaSynchronizationMode SchemaSynchronization { get; set; } = SchemaSynchronizationMode.Manual;

		[Items(DesignUtils.QueryItems)]
		public ListItems<IBigDataQuery> Queries
		{
			get
			{
				if (_queries == null)
					_queries = new ListItems<IBigDataQuery> { Parent = this };

				return _queries;
			}
		}
		[Browsable(false)]
		public override string FileName => $"{ToString()}.csx";
	}
}
