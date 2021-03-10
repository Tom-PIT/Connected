using System;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;
using TomPIT.Connectivity;

namespace TomPIT.Compilation
{
	internal class LocalResourceProvider : ResourceProvider
	{
		public LocalResourceProvider(ITenant tenant, Guid blob) : base(typeof(DownloadResource))
		{
			Tenant = tenant;
			Blob = blob;
		}

		public override string Name => "Blob Provider";
		private ITenant Tenant { get; }
		private Guid Blob { get; }
		public override async Task<Tuple<bool, INuGetResource>> TryCreate(SourceRepository source, CancellationToken token)
		{
			await Task.CompletedTask;

			return new Tuple<bool, INuGetResource>(true, new LocalDownloadResource(Tenant, Blob));
		}
	}
}
