using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json;
using TomPIT.ComponentModel;
using TomPIT.Sys.Model.Components;

namespace TomPIT.Sys.SourceFiles;

internal static class FileSystem
{
	private static string Folder { get; set; }
	private static string MicroServicesFileName => Path.Combine(Folder, "microServices.json");
	private static string ComponentsFileName => Path.Combine(Folder, "components.json");
	private static string FoldersFileName => Path.Combine(Folder, "folders.json");
	private static string SourceFilesFileName => Path.Combine(Folder, "files.json");
	public static void Initialize(Microsoft.Extensions.Configuration.IConfiguration configuration)
	{
		var section = configuration.GetRequiredSection("sourceFiles");

		Folder = section.GetValue<string>("folder");

		LoadIndex();
	}

	private static void LoadIndex()
	{
		//if (!Directory.Exists(Folder))
		//	Directory.CreateDirectory(Folder);

		//if (!File.Exists(IndexFileName))
		//	return;

		//var text = File.ReadAllText(IndexFileName);
		//var microServices = JsonSerializer.Deserialize<List<MicroServiceIndexEntry>>(text);

		//foreach (var microService in microServices)
		//{
		//	foreach (var component in microService.Components)
		//	{

		//	}
		//	//TODO: add microservices and components to the datamodel cache.
		//}

		//if (!File.Exists(SourceFilesFileName))
		//	return;

		//text = File.ReadAllText(SourceFilesFileName);
		//var files = JsonSerializer.Deserialize<List<SourceFile>>(text);

		//DataModel.SourceFiles.Initialize(files);
	}

	public static void Serialize(ImmutableList<IMicroService> microServices)
	{
		var content = new List<MicroServiceIndexEntry>();

		foreach (var microService in microServices)
			content.Add(MicroServiceIndexEntry.From(microService));

		File.WriteAllText(MicroServicesFileName, JsonSerializer.Serialize(content));
	}

	public static void Serialize(ImmutableList<IComponent> components)
	{
		var content = new List<ComponentIndexEntry>();

		foreach (var component in components)
			content.Add(ComponentIndexEntry.From(component));

		File.WriteAllText(ComponentsFileName, JsonSerializer.Serialize(content));
	}

	public static void Serialize(ImmutableList<IFolder> folders)
	{
		var content = new List<FolderIndexEntry>();

		foreach (var folder in folders)
			content.Add(FolderIndexEntry.From(folder));

		File.WriteAllText(FoldersFileName, JsonSerializer.Serialize(content));
	}

	public static void Serialize(ImmutableList<SourceFile> files)
	{
		File.WriteAllText(SourceFilesFileName, JsonSerializer.Serialize(files));
	}

	public static void Serialize(Guid microService, Guid token, int type, byte[] content)
	{
		var path = Path.Combine(Folder, microService.ToString());

		if (!Directory.Exists(path))
			Directory.CreateDirectory(path);

		File.WriteAllBytes(Path.Combine(path, ParseFileName(token, type)), content);
	}

	public static void Delete(Guid microService, Guid token, int type)
	{
		var path = Path.Combine(Folder, microService.ToString());

		if (!Directory.Exists(path))
			return;

		File.Delete(Path.Combine(path, ParseFileName(token, type)));
	}

	public static byte[]? Deserialize(Guid microService, Guid token, int type)
	{
		var path = Path.Combine(Folder, microService.ToString());

		if (!Directory.Exists(path))
			return null;

		var fileName = Path.Combine(path, ParseFileName(token, type));

		if (!File.Exists(fileName))
			return null;

		return File.ReadAllBytes(fileName);
	}

	private static string ParseFileName(Guid token, int type) => $"{token}-{type}.txt";
}
