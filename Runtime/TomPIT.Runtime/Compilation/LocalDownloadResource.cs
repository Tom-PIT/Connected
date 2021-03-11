using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using TomPIT.Connectivity;
using TomPIT.Storage;

namespace TomPIT.Compilation
{
	internal class LocalDownloadResource : DownloadResource
	{
		public LocalDownloadResource(ITenant tenant, Guid blob)
		{
			Tenant = tenant;
			Blob = blob;
		}

		private ITenant Tenant { get; }
		private Guid Blob { get; }
		public override async Task<DownloadResourceResult> GetDownloadResourceResultAsync(PackageIdentity identity, PackageDownloadContext downloadContext, string globalPackagesFolder, ILogger logger, CancellationToken token)
		{
			var content = Tenant.GetService<IStorageService>().Download(Blob);

			if (content == null || content.Content?.Length == 0)
				return null;

			var ms = new MemoryStream(content.Content);
			var reader = new PackageArchiveReader(ms);
			var result = new DownloadResourceResult(ms, reader, "local");

			var folder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), ".tompit", "packages");
			var packagePathResolver = new PackagePathResolver(folder, false);
			
			var packageExtractionContext = new PackageExtractionContext(PackageSaveMode.Defaultv3, XmlDocFileSaveMode.None, null, logger);
			
			await PackageExtractor.ExtractPackageAsync("local", ms, packagePathResolver, packageExtractionContext, CancellationToken.None);
			await Task.CompletedTask;

			return result;
		}
	}
}
