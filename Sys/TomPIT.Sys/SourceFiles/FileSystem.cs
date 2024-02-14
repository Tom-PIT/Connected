using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TomPIT.Sys.Model;
using TomPIT.Sys.Model.Components;

namespace TomPIT.Sys.SourceFiles;

internal static class FileSystem
{
	private static string Folder { get; set; }
	private static string IndexFileName => Path.Combine(Folder, "index.json");
	private static string SourceFilesFileName => Path.Combine(Folder, "files.json");
	public static void Initialize(IConfiguration configuration)
	{
		var section = configuration.GetRequiredSection("sourceFiles");

		Folder = section.GetValue<string>("folder");

		LoadIndex();
	}

	private static void LoadIndex()
	{
		if (!Directory.Exists(Folder))
			Directory.CreateDirectory(Folder);

		if (!File.Exists(IndexFileName))
			return;

		var text = File.ReadAllText(IndexFileName);
		var microServices = JsonSerializer.Deserialize<List<MicroServiceIndexEntry>>(text);

		foreach (var microService in microServices)
		{
			foreach (var component in microService.Components)
			{

			}
			//TODO: add microservices and components to the datamodel cache.
		}

		if (!File.Exists(SourceFilesFileName))
			return;

		text = File.ReadAllText(SourceFilesFileName);
		var files = JsonSerializer.Deserialize<List<SourceFile>>(text);

		DataModel.SourceFiles.Initialize(files);
	}

	public static void SerializeIndex()
	{
		var content = new List<MicroServiceIndexEntry>();

		foreach (var microService in DataModel.MicroServices.Query())
		{
			var components = DataModel.Components.Query(microService.Token);
			var entries = new List<ComponentIndexEntry>();

			foreach (var component in components)
				entries.Add(ComponentIndexEntry.From(component));

			content.Add(MicroServiceIndexEntry.From(microService, entries));
		}

		File.WriteAllText(IndexFileName, JsonSerializer.Serialize(content));
	}

	public static void SerializeSourceFileIndex()
	{
		var content = new List<SourceFile>();

		foreach (var file in DataModel.SourceFiles.Query())
			content.Add(file);

		File.WriteAllText(SourceFilesFileName, JsonSerializer.Serialize(content));
	}
	public static void Serialize(Guid microService, Guid token, int type, byte[] content)
	{
		var path = Path.Combine(Folder, microService.ToString());

		if (!Directory.Exists(path))
			Directory.CreateDirectory(path);

		File.WriteAllBytes(Path.Combine(path, $"{type}-{token}.txt"), content);
	}

	public static void Delete(Guid microService, Guid token, int type)
	{
		var path = Path.Combine(Folder, microService.ToString());

		if (!Directory.Exists(path))
			return;

		File.Delete(Path.Combine(path, $"{type}-{token}.txt"));
	}

	public static byte[]? Deserialize(Guid microService, Guid token, int type)
	{
		var path = Path.Combine(Folder, microService.ToString());

		if (!Directory.Exists(path))
			return null;

		var fileName = Path.Combine(path, $"{type}-{token}.txt");

		if (!File.Exists(fileName))
			return null;

		return File.ReadAllBytes(fileName);
	}

	public static void Migrate()
	{
		var content = new List<MicroServiceIndexEntry>();

		foreach (var microService in DataModel.MicroServices.Query())
		{
			var components = DataModel.Components.Query(microService.Token);
			var entries = new List<ComponentIndexEntry>();

			foreach (var component in components)
			{
				entries.Add(ComponentIndexEntry.From(component));
			}

			content.Add(MicroServiceIndexEntry.From(microService, entries));
		}

		File.WriteAllText(IndexFileName, JsonSerializer.Serialize(content));
	}
}
