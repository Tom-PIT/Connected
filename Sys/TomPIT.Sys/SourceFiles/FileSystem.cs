using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
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
	public static bool LockUpdate { get; set; }
	static FileSystem()
	{
		var section = Shell.Configuration.GetRequiredSection("sourceFiles");

		Folder = section.GetValue<string>("folder");

		if (!Directory.Exists(Folder))
			Directory.CreateDirectory(Folder);
	}

	public static List<IMicroService> LoadMicroServices()
	{
		if (!File.Exists(MicroServicesFileName))
			return new List<IMicroService>();

		var text = File.ReadAllText(MicroServicesFileName);

		return JsonSerializer.Deserialize<List<MicroServiceIndexEntry>>(text).ToList<IMicroService>();
	}

	public static List<IFolder> LoadFolders()
	{
		if (!File.Exists(FoldersFileName))
			return new List<IFolder>();

		var text = File.ReadAllText(FoldersFileName);

		return JsonSerializer.Deserialize<List<FolderIndexEntry>>(text).ToList<IFolder>();
	}

	public static List<IComponent> LoadComponents()
	{
		if (!File.Exists(ComponentsFileName))
			return new List<IComponent>();

		var text = File.ReadAllText(ComponentsFileName);

		return JsonSerializer.Deserialize<List<ComponentIndexEntry>>(text).ToList<IComponent>();
	}

	public static List<SourceFile> LoadFiles()
	{
		if (!File.Exists(SourceFilesFileName))
			return new List<SourceFile>();

		var text = File.ReadAllText(SourceFilesFileName);

		return JsonSerializer.Deserialize<List<SourceFile>>(text);
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
