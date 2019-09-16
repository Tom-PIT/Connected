using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.Diagnostics;
using TomPIT.Diagnostics;
using TomPIT.MicroServices.Design;
using TomPIT.Reflection;
using TomPIT.Runtime;

namespace TomPIT.MicroServices.Apis
{
	[DomElement(DesignUtils.ApiOperationElement)]
	[DomDesigner(DomDesignerAttribute.PermissionsDesigner, Mode = EnvironmentMode.Runtime)]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ComponentCreatedHandler(DesignUtils.ApiOperationCreateHandler)]
	public class Operation : ConfigurationElement, IApiOperation
	{
		private OperationProtocolOptions _protocols = null;
		private IMetricOptions _metric = null;

		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Name { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name) ? GetType().ShortName() : Name;
		}

		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public IOperationProtocolOptions Protocols
		{
			get
			{
				if (_protocols == null)
					_protocols = new OperationProtocolOptions { Parent = this };

				return _protocols;
			}
		}

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[DefaultValue(ElementScope.Public)]
		public ElementScope Scope { get; set; } = ElementScope.Public;

		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public IMetricOptions Metrics
		{
			get
			{
				if (_metric == null)
					_metric = new MetricOptions { Parent = this };

				return _metric;
			}
		}

		[Browsable(false)]
		public Guid TextBlob { get; set; }
	}
}
