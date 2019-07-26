using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.Events;
using TomPIT.Services;

namespace TomPIT.Application.Apis
{
	[DomElement("TomPIT.Application.Design.Dom.ApiOperationElement, TomPIT.Application.Design")]
	[DomDesigner(DomDesignerAttribute.PermissionsDesigner, Mode = EnvironmentMode.Runtime)]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	[ComponentCreatedHandler("TomPIT.Development.Handlers.ApiOperationCreateHandler, TomPIT.Development")]
	public class Operation : ConfigurationElement, IApiOperation
	{
		private OperationProtocolOptions _protocols = null;
		private IMetricConfiguration _metric = null;

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

		[EnvironmentVisibility(Services.EnvironmentMode.Runtime)]
		public IMetricConfiguration Metrics
		{
			get
			{
				if (_metric == null)
					_metric = new MetricConfiguration { Parent = this };

				return _metric;
			}
		}

		[Browsable(false)]
		public Guid TextBlob { get; set; }
	}
}
