using System;
using System.Collections.Concurrent;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.UI.Theming.Input;

namespace TomPIT.UI.Theming
{
	internal class ThemeFileReader : TenantObject, IFileReader
	{
		public bool UseCacheDependencies => true;
		private ConcurrentDictionary<string, IText> _files;

		public ThemeFileReader(ITenant tenant) : base(tenant)
		{

		}

		private ConcurrentDictionary<string, IText> Files => _files ??= new ConcurrentDictionary<string, IText>(StringComparer.OrdinalIgnoreCase);

		public bool DoesFileExist(string fileName)
		{
			return LoadFile(fileName) is not null;
		}

		public byte[] GetBinaryFileContents(string fileName)
		{
			throw new NotImplementedException();
		}

		public string GetFileContents(string fileName)
		{
			if (LoadFile(fileName) is not IText file)
				return null;

			return Tenant.GetService<IComponentService>().SelectText(file.Configuration().MicroService(), file);
		}

		private IText LoadFile(string fileName)
		{
			if (Files.TryGetValue(fileName, out IText result))
				return result;

			using var ctx = MicroServiceContext.FromIdentifier(fileName, Tenant);
			var descriptor = ComponentDescriptor.Theme(ctx, fileName);

			try
			{
				descriptor.Validate();
				descriptor.ValidateConfiguration();

				foreach (var stylesheet in descriptor.Configuration.Stylesheets)
				{
					if (stylesheet is IText text && string.Compare(text.FileName, descriptor.Element, true) == 0)
					{
						Files.TryAdd(fileName, text);

						return text;
					}
				}

				return null;
			}
			catch
			{
				return null;
			}
		}
	}
}
