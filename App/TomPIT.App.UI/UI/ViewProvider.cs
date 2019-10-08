using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace TomPIT.App.UI
{
	public class ViewProvider : IFileProvider
	{
		public ViewProvider()
		{
		}

		public IDirectoryContents GetDirectoryContents(string subpath)
		{
			return null;
		}

		public IFileInfo GetFileInfo(string subpath)
		{
			if (!subpath.StartsWith("/Views/Dynamic/", System.StringComparison.OrdinalIgnoreCase))
				return new NotFoundFileInfo(subpath);

			if (ChangeToken.SystemViews.FirstOrDefault(f => string.Compare(f, System.IO.Path.GetFileName(subpath), true) == 0) != null)
				return new NotFoundFileInfo(subpath);

			var result = new ViewInfo(subpath, null);

			return result.Exists
				? result as IFileInfo
				: new NotFoundFileInfo(subpath);
		}

		public IChangeToken Watch(string filter)
		{
			return new ChangeToken(filter);
		}
	}
}