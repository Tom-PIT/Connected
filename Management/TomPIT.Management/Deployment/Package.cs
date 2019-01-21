using Newtonsoft.Json;
using System.Collections.Generic;
using TomPIT.ComponentModel;

namespace TomPIT.Deployment
{
	public class Package
	{
		[JsonProperty(PropertyName = "metaData")]
		public PackageMetaData MetaData { get; set; }
		[JsonProperty(PropertyName = "microService")]
		public MicroService MicroService { get; set; }
		[JsonProperty(PropertyName = "strings")]
		public List<MicroServiceString> Strings { get; set; }
		[JsonProperty(PropertyName = "features")]
		public List<Feature> Features { get; set; }
		[JsonProperty(PropertyName = "components")]
		public List<Component> Components { get; set; }
		[JsonProperty(PropertyName = "blobs")]
		public List<Blob> Blobs { get; set; }
		[JsonProperty(PropertyName = "dependencies")]
		public List<Dependency> Dependencies { get; set; }
		[JsonProperty(PropertyName = "databases")]
		public List<Database> Databases { get; set; }

		public static Package Create(PackageCreateArgs e)
		{
			var r = new Package();

			r.CreatePackage(e);

			return r;
		}

		[JsonIgnore]
		private PackageCreateArgs Args { get; set; }

		internal void CreatePackage(PackageCreateArgs e)
		{
			Args = e;

			CreateMicroService();
		}

		private void CreateMicroService()
		{
			var ms = Args.Connection.GetService<IMicroServiceService>().Select(Args.MicroService);

			if (ms == null)
				throw new RuntimeException(SR.ErrMicroServiceNotFound);

			
		}
	}
}
