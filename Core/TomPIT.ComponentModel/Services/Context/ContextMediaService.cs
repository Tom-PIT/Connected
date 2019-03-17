using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Storage;

namespace TomPIT.Services.Context
{
	internal class ContextMediaService : ContextClient, IContextMediaService
	{
		public ContextMediaService(IExecutionContext context) : base(context)
		{
		}

		public string ResourceUrl(string path)
		{
			var tokens = path.Split('/');
			var media = Context.Connection().GetService<IComponentService>().SelectConfiguration(Context.MicroService.Token, "Media", tokens[0]) as IMediaResources;

			return FindFile(media, tokens.Skip(1));
		}

		private string FindFile(IMediaResources media, IEnumerable<string> path)
		{
			if (path.Count() == 0)
			{
				var file = media.Files.FirstOrDefault(f => string.Compare(f.FileName, path.ElementAt(0), true) == 0);

				if (file == null)
					throw new RuntimeException($"{SR.ErrMediaFileNotFound} ({path.ElementAt(0)})");

				return GetUrl(file.Blob);
			}
			else
			{
				var folder = media.Folders.FirstOrDefault(f => string.Compare(f.Name, path.ElementAt(0), true) == 0);

				if (folder == null)
					throw new RuntimeException($"{SR.ErrMediaFolderNotFound} ({path.ElementAt(0)})");

				return FindFile(folder, path.Skip(1));
			}
		}

		private string FindFile(IMediaResourceFolder folder, IEnumerable<string> path)
		{
			if (path.Count() == 1)
			{
				var file = folder.Files.FirstOrDefault(f => string.Compare(f.FileName, path.ElementAt(0), true) == 0);

				if (file == null)
					throw new RuntimeException($"{SR.ErrMediaFileNotFound} ({path.ElementAt(0)})");

				return GetUrl(file.Blob);
			}
			else
			{
				var subFolder = folder.Folders.FirstOrDefault(f => string.Compare(f.Name, path.ElementAt(0), true) == 0);

				if (subFolder == null)
					throw new RuntimeException($"{SR.ErrMediaFolderNotFound} ({path.ElementAt(0)})");

				return FindFile(subFolder, path.Skip(1));
			}
		}

		private string GetUrl(Guid blob)
		{
			if (blob == null)
				return null;

			var b = Context.Connection().GetService<IStorageService>().Select(blob);

			if (b == null)
				return null;

			return $"{Shell.HttpContext.Request.RootUrl()}/sys/media/{blob}/{b.Version}";
		}
	}
}
