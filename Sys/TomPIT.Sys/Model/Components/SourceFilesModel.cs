using System;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.Sys.SourceFiles;

namespace TomPIT.Sys.Model.Components;

public class SourceFilesModel : SynchronizedRepository<SourceFile, string>
{
	public SourceFilesModel(IMemoryCache container) : base(container, "sourceFiles")
	{
	}

	public ImmutableList<SourceFile> Query() => All();

	protected override void OnInitializing()
	{
		var files = FileSystem.LoadFiles();

		foreach (var file in files)
			Set(GenerateKey(file.Token, file.Type), file, TimeSpan.Zero);
	}
	public void Update(Guid token, int type, string primaryKey, Guid microService, string fileName, string contentType, int version, byte[] content)
	{
		Initialize();
		var existing = Get(GenerateKey(token, type));

		if (existing is not null)
		{
			existing.PrimaryKey = primaryKey;
			existing.FileName = fileName;
			existing.Size = content is null ? 0 : content.Length;
			existing.Version = version;
			existing.ContentType = contentType;
			existing.Modified = DateTime.UtcNow;
		}
		else
		{
			existing = new SourceFile
			{
				ContentType = contentType,
				FileName = fileName,
				MicroService = microService,
				Modified = DateTime.UtcNow,
				PrimaryKey = primaryKey,
				Size = content is null ? 0 : content.Length,
				Token = token,
				Type = type,
				Version = version
			};

			Set(GenerateKey(token, type), existing, TimeSpan.Zero);
		}

		FileSystem.Serialize(microService, token, type, content);
		FileSystem.Serialize(All());
	}

	public void Delete(Guid microService, Guid token, int type)
	{
		Initialize();
		FileSystem.Delete(microService, token, type);
		FileSystem.Serialize(All());
	}

	public byte[] Select(Guid microService, Guid token, int type)
	{
		Initialize();
		return FileSystem.Deserialize(microService, token, type);
	}

	public SourceFile SelectFileInfo(Guid token, int type)
	{
		return Get(GenerateKey(token, type));
	}

	private static string GenerateKey(Guid token, int type)
	{
		return $"{token}-{type}";
	}
}
