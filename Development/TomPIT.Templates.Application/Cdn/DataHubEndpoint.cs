using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Cdn
{
	[Create("Endpoint", nameof(Name))]
	public class DataHubEndpoint : SourceCodeElement, IDataHubEndpoint
	{
		private ListItems<IDataHubEndpointPolicy> _policies = null;

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[Required]
		public string Name { get; set; }

		[Items(DesignUtils.DataHubEndpointPolicyItems)]
		public ListItems<IDataHubEndpointPolicy> Policies
		{
			get
			{
				if (_policies == null)
					_policies = new ListItems<IDataHubEndpointPolicy> { Parent = this };

				return _policies;
			}
		}

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? base.ToString()
				: Name;
		}
	}
}
