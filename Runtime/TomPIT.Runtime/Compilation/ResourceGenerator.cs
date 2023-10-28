using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Xml.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;

namespace TomPIT.Compilation;
internal static class ResourceGenerator
{
	public static List<ResourceDescription> Generate(IMicroService microService)
	{
		var resources = new List<ResourceDescription>();
		var configurations = Tenant.GetService<IComponentService>().QueryConfigurations(microService.Token, ComponentCategories.AssemblyResource);

		foreach (var configuration in configurations)
		{
			var descriptor = CreateResource(microService, configuration as IAssemblyResourceConfiguration);

			if (descriptor is null)
				continue;

			resources.Add(descriptor);
		}

		return resources;
	}

	private static ResourceDescription? CreateResource(IMicroService microService, IAssemblyResourceConfiguration? configuration)
	{
		if (configuration is null)
			return null;

		var strings = LoadStrings(microService, configuration);

		if (strings is null)
			return null;

		var stream = new MemoryStream();
		using var writer = new ResourceWriter(stream);

		foreach (var str in strings)
			writer.AddResource(str.Key, str.Value);

		writer.Generate();
		writer.Close();

		stream.Seek(0, SeekOrigin.Begin);

		var key = $"{microService.Name}.{configuration.ComponentName()}.resources";

		return new ResourceDescription(key, () => stream, true);
	}

	private static Dictionary<string, string>? LoadStrings(IMicroService microService, IAssemblyResourceConfiguration configuration)
	{
		var text = Tenant.GetService<IComponentService>().SelectText(microService.Token, configuration);

		if (string.IsNullOrEmpty(text))
			return null;

		using var ms = new MemoryStream(Encoding.UTF8.GetBytes(text));
		var keys = new List<string>();
		var doc = XDocument.Load(ms);

		if (doc.Root is null)
			return null;

		var result = new Dictionary<string, string>();

		var strings = from i in doc.Root.Elements("data")
						  select i.Attribute("name");

		foreach (var key in strings)
			result.Add(key.Name.LocalName, key.Value);

		return result;
	}
}
