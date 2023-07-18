using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TomPIT.Environment;

namespace TomPIT.Management.Environment
{
	public class InstanceEndpoint : IInstanceEndpoint
	{
		public string Url { get; set; }

		public InstanceStatus Status { get; set; } = InstanceStatus.Enabled;

		public string Name { get; set; }

		[JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
		public InstanceFeatures Features { get; set; }

		[JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
		public InstanceVerbs Verbs { get; set; } = InstanceVerbs.All;

		public string ReverseProxyUrl { get; set; }

		public virtual Guid Token => throw new NotSupportedException();
	}
}
