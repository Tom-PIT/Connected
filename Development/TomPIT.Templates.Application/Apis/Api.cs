using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Services;

namespace TomPIT.Application.Apis
{
	[Create("Api")]
	public class Api : ComponentConfiguration, IApi
	{
		public const string ComponentCategory = "Api";

		private ListItems<IApiOperation> _ops = null;
		private ApiProtocolOptions _protocols = null;

		[Items("TomPIT.Application.Design.Items.OperationCollection, TomPIT.Application.Design")]
		public ListItems<IApiOperation> Operations
		{
			get
			{
				if (_ops == null)
					_ops = new ListItems<IApiOperation> { Parent = this };

				return _ops;
			}
		}

		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public IApiProtocolOptions Protocols
		{
			get
			{
				if (_protocols == null)
					_protocols = new ApiProtocolOptions
					{
						Parent = this
					};

				return _protocols;
			}
		}

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[DefaultValue(ElementScope.Public)]
		public ElementScope Scope { get; set; } = ElementScope.Public;
	}
}
