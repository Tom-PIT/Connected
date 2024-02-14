using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.Sys.SourceFiles;

namespace TomPIT.Sys.Model.Components;

public class SourceFilesModel : SynchronizedRepository<SourceFile, Guid>
{
	public SourceFilesModel(IMemoryCache container) : base(container, "sourceFiles")
	{
	}

	public ImmutableList<SourceFile> Query() => All();

	public void Initialize(List<SourceFile> files)
	{
		foreach (var file in files)
			Set(file.Token, file, TimeSpan.Zero);
	}
	public void Update(Guid token, int type, string primaryKey, Guid microService, string fileName, string contentType, int version, byte[] content)
	{
		var existing = Get(token);

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

			Set(token, existing, TimeSpan.Zero);
		}

		FileSystem.Serialize(microService, token, type, content);
		FileSystem.SerializeSourceFileIndex();
	}

	public void Delete(Guid microService, Guid token, int type)
	{
		FileSystem.Delete(microService, token, type);
		FileSystem.SerializeSourceFileIndex();
	}

	public byte[] Select(Guid microService, Guid token, int type)
	{
		return FileSystem.Deserialize(microService, token, type);
	}
}
