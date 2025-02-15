﻿using NuGet.Protocol.Core.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TomPIT.Compilation
{
	internal class LocalResourceProvider : ResourceProvider
	{
		public LocalResourceProvider(Guid blob) : base(typeof(DownloadResource))
		{
			Blob = blob;
		}

		public override string Name => "Blob Provider";
		private Guid Blob { get; }
		public override async Task<Tuple<bool, INuGetResource>> TryCreate(SourceRepository source, CancellationToken token)
		{
			await Task.CompletedTask;

			return new Tuple<bool, INuGetResource>(true, new LocalDownloadResource(Blob));
		}
	}
}
