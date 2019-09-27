using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Exceptions;
using TomPIT.Ide.ComponentModel;
using TomPIT.Ide.VersionControl;
using TomPIT.MicroServices.Resources;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Routing;
using TomPIT.Serialization;
using TomPIT.Storage;
using TomPIT.UI;

namespace TomPIT.MicroServices.Design.Media
{
	internal enum MediaCommand
	{
		AbortUpload = 1,
		Copy = 2,
		CreateDir = 3,
		GetDirContents = 4,
		Move = 5,
		Remove = 6,
		Rename = 7,
		UploadChunk = 8
	}

	internal enum ExceptionKind
	{
		None = 0,
		NoAccess = 1,
		FileExists = 2,
		FileNotFound = 3,
		DirectoryExists = 4
	}

	internal class MediaHandler : RouteHandlerBase
	{
		private static readonly FormOptions _defaultFormOptions = new FormOptions();
		private bool _responseStarted = false;
		protected override void OnProcessRequest()
		{
			if (string.Compare(Context.Request.Method, "POST", true) == 0)
			{
				if (!ParseQueryString())
				{
					Command = (MediaCommand)Enum.Parse(typeof(MediaCommand), Context.Request.Form["command"]);
					string query = Context.Request.Form["arguments"];

					Arguments = string.IsNullOrEmpty(query)
						? new Dictionary<string, string>()
						: Serializer.Deserialize<Dictionary<string, string>>(query);
				}
			}
			else
				ParseQueryString();

			MicroService = Tenant.GetService<IMicroServiceService>().Select((string)Context.GetRouteValue("microService"));

			if (MicroService == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			Media = Tenant.GetService<IComponentService>().SelectConfiguration(MicroService.Token, "Media", (string)Context.GetRouteValue("component")) as IMediaResourcesConfiguration;

			if (Media == null)
			{
				Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			Component = Tenant.GetService<IComponentService>().SelectComponent(Media.Component);

			switch (Command)
			{
				case MediaCommand.AbortUpload:
					AbordUpload();
					break;
				case MediaCommand.Copy:
					Copy();
					break;
				case MediaCommand.CreateDir:
					CreateDir();
					break;
				case MediaCommand.GetDirContents:
					GetDirContents();
					break;
				case MediaCommand.Move:
					Move();
					break;
				case MediaCommand.Remove:
					Remove();
					break;
				case MediaCommand.Rename:
					Rename();
					break;
				case MediaCommand.UploadChunk:
					UploadChunk();
					break;
				default:
					throw new NotSupportedException();
			}
		}

		private bool ParseQueryString()
		{
			if (Context.Request.Query.Count == 0)
				return false;

			Command = (MediaCommand)Enum.Parse(typeof(MediaCommand), Context.Request.Query["command"]);
			string query = Context.Request.Query["arguments"];

			Arguments = string.IsNullOrEmpty(query)
				? new Dictionary<string, string>()
				: Serializer.Deserialize<Dictionary<string, string>>(query);

			return true;
		}

		private void UploadChunk()
		{
			Tenant.GetService<IVersionControlService>().Lock(Media.Component, Development.LockVerb.Edit);

			var metaData = Serializer.Deserialize<ChunkMetaData>(Arguments["chunkMetadata"]);
			var files = Context.Request.Form.Files;

			if (files.Count == 0)
				return;

			var file = files[0];
			var blobId = Guid.Empty;

			using (var fileStream = file.OpenReadStream())
			{
				var provider = new FileExtensionContentTypeProvider();

				if (!provider.TryGetContentType(metaData.FileName, out string contentType))
					contentType = file.ContentType;

				var blob = new Blob
				{
					ContentType = contentType,
					Draft = metaData.UploadId,
					FileName = file.FileName,
					MicroService = MicroService.Token,
					ResourceGroup = MicroService.ResourceGroup,
					Type = BlobTypes.UserContent
				};

				var blobs = Tenant.GetService<IStorageService>().QueryDrafts(blob.Draft);
				var existing = blobs.Count == 0 ? null : blobs[0];
				var buffer = new List<byte>();

				if (existing != null)
				{
					var content = Tenant.GetService<IStorageService>().Download(existing.Token);

					if (content != null && content.Content != null && content.Content.Length > 0)
						buffer.AddRange(content.Content);
				}

				var newBuffer = new byte[file.Length];

				fileStream.Read(newBuffer, 0, newBuffer.Length);

				buffer.AddRange(newBuffer);

				blobId = Tenant.GetService<IStorageService>().Upload(blob, buffer.ToArray(), StoragePolicy.Singleton);
			}

			if (metaData.Index == metaData.TotalCount - 1)
			{
				var folder = ResolveFolder(Arguments["destinationId"]);
				var targetFile = folder == null
					? Media.Files.FirstOrDefault(f => string.Compare(f.FileName, metaData.FileName, true) == 0)
					: folder.Files.FirstOrDefault(f => string.Compare(f.FileName, metaData.FileName, true) == 0);

				if (targetFile == null)
				{
					targetFile = new MediaResourceFile
					{
						FileName = metaData.FileName,
						Size = metaData.FileSize,
						Modified = DateTime.UtcNow
					};

					if (folder == null)
						Media.Files.Add(targetFile);
					else
						folder.Files.Add(targetFile);
				}
				else
				{
					Tenant.GetService<IStorageService>().Delete(targetFile.Blob);

					targetFile.Modified = DateTime.UtcNow;
					targetFile.Size = metaData.FileSize;
				}

				Tenant.GetService<IStorageService>().Commit(metaData.UploadId, targetFile.Id.ToString());

				targetFile.Blob = blobId;
				UpdateModified(folder);
				CreateThumbnail(targetFile);
				Tenant.GetService<IComponentDevelopmentService>().Update(Media);
			}

			RenderResult(true);
		}

		private void Rename()
		{
			Tenant.GetService<IVersionControlService>().Lock(Media.Component, Development.LockVerb.Edit);

			var id = Arguments["id"];
			var name = Arguments["name"];

			var tokens = id.Split("/");
			IMediaResourceFolder parent = null;
			IMediaResourceFile file = null;

			foreach (var token in tokens)
			{
				file = null;

				if (parent == null)
				{
					var folder = Media.Folders.FirstOrDefault(f => string.Compare(f.Name, token, true) == 0);

					if (folder != null)
						parent = folder;
					else
						file = Media.Files.FirstOrDefault(f => string.Compare(f.FileName, token, true) == 0);
				}
				else
				{
					var folder = parent.Folders.FirstOrDefault(f => string.Compare(f.Name, token, true) == 0);

					if (folder != null)
						parent = folder;
					else
						file = parent.Files.FirstOrDefault(f => string.Compare(f.FileName, token, true) == 0);
				}
			}

			if (file != null)
			{
				file.FileName = name;
				file.Modified = DateTime.UtcNow;
			}
			else
				parent.Name = name;

			UpdateModified(parent);

			Tenant.GetService<IComponentDevelopmentService>().Update(Media);
			RenderResult(true);
		}

		private void UpdateModified(IMediaResourceFolder folder)
		{
			while (folder != null)
			{
				folder.Modified = DateTime.UtcNow;
				folder = folder.Parent.Closest<IMediaResourceFolder>();
			}
		}

		private void Remove()
		{
			Tenant.GetService<IVersionControlService>().Lock(Media.Component, Development.LockVerb.Edit);

			var id = Arguments["id"];

			if (IsFolder(id))
				DeleteFolder(id);
			else
				DeleteFile(id);

			Tenant.GetService<IComponentDevelopmentService>().Update(Media);
			RenderResult(true);
		}

		private void DeleteFolder(string id)
		{
			DeleteFolder(ResolveFolder(id));
		}

		private void DeleteFolder(IMediaResourceFolder folder)
		{
			for (var i = folder.Folders.Count - 1; i >= 0; i--)
				DeleteFolder(folder.Folders[i]);

			for (var i = folder.Files.Count - 1; i >= 0; i--)
				DeleteFile(folder.Files[i]);

			var parent = folder.Parent.Closest<IMediaResourceFolder>();

			if (parent == null)
				Media.Folders.Remove(folder);
			else
			{
				parent.Folders.Remove(folder);
				UpdateModified(parent);
			}
		}

		private void DeleteFile(string id)
		{
			DeleteFile(ResolveFile(id));
		}

		private void DeleteFile(IMediaResourceFile file)
		{
			var folder = file.Closest<IMediaResourceFolder>();

			if (folder == null)
				Media.Files.Remove(file);
			else
			{
				folder.Files.Remove(file);
				UpdateModified(folder);
			}

			if (file.Blob != Guid.Empty)
			{
				try
				{
					Tenant.GetService<IStorageService>().Delete(file.Blob);
				}
				catch { }
			}

			if (file.Thumb != Guid.Empty)
			{
				try
				{
					Tenant.GetService<IStorageService>().Delete(file.Thumb);
				}
				catch { }
			}
		}

		private void Move()
		{
			Tenant.GetService<IVersionControlService>().Lock(Media.Component, Development.LockVerb.Edit);

			var sourceId = Arguments["sourceId"];
			var destinationId = Arguments["destinationId"];

			if (IsFolder(sourceId))
				MoveFolder(sourceId, destinationId);
			else
				MoveFile(sourceId, destinationId);

			Tenant.GetService<IComponentDevelopmentService>().Update(Media);

			RenderResult(true);
		}

		private void MoveFile(string sourceId, string destinationId)
		{
			var sourceFolder = ResolveFolder(sourceId);
			var destinationFolder = ResolveFolder(destinationId);
			var sourceFile = ResolveFile(sourceId);

			if (destinationFolder != null)
			{
				if (destinationFolder.Files.FirstOrDefault(f => string.Compare(f.FileName, sourceFile.FileName, true) == 0) != null)
				{
					RenderResult(false, ExceptionKind.FileExists);
					return;
				}
			}
			else
			{
				if (Media.Files.FirstOrDefault(f => string.Compare(f.FileName, sourceFile.FileName, true) == 0) != null)
				{
					RenderResult(false, ExceptionKind.FileExists);
					return;
				}
			}

			if (sourceFolder == null)
				Media.Files.Remove(sourceFile);
			else
				sourceFolder.Files.Remove(sourceFile);

			if (destinationFolder == null)
				Media.Files.Add(sourceFile);
			else
				destinationFolder.Files.Add(sourceFile);

			if (sourceFolder != null)
				UpdateModified(sourceFolder);

			if (destinationFolder != null)
				UpdateModified(destinationFolder);

			Tenant.GetService<IComponentDevelopmentService>().Update(Media);
			RenderResult(true);
		}

		private void MoveFolder(string sourceId, string destinationId)
		{
			var sourceFolder = ResolveFolder(sourceId);

			var destinationTokens = destinationId.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			var destinationPath = new StringBuilder();

			for (var i = 0; i < destinationTokens.Length - 1; i++)
				destinationPath.Append($"{destinationTokens[i]}/");

			var destinationFolder = ResolveFolder(destinationPath.ToString().Trim('/'));

			if (destinationFolder != null)
			{
				if (destinationFolder.Folders.FirstOrDefault(f => string.Compare(f.Name, sourceFolder.Name, true) == 0) != null)
				{
					RenderResult(false, ExceptionKind.DirectoryExists);
					return;
				}
			}
			else
			{
				if (Media.Folders.FirstOrDefault(f => string.Compare(f.Name, sourceFolder.Name, true) == 0) != null)
				{
					RenderResult(false, ExceptionKind.DirectoryExists);
					return;
				}
			}

			if (sourceFolder == null)
				Media.Folders.Remove(sourceFolder);
			else
			{
				var sourceParent = sourceFolder.Parent.Closest<IMediaResourceFolder>();

				if (sourceParent == null)
					Media.Folders.Remove(sourceFolder);
				else
					sourceParent.Folders.Remove(sourceFolder);
			}

			if (destinationFolder == null)
				Media.Folders.Add(sourceFolder);
			else
				destinationFolder.Folders.Add(sourceFolder);

			if (sourceFolder != null)
				UpdateModified(sourceFolder);

			if (destinationFolder != null)
				UpdateModified(destinationFolder);

			Tenant.GetService<IComponentDevelopmentService>().Update(Media);
			RenderResult(true);
		}

		private void GetDirContents()
		{
			var path = Arguments["parentId"];
			var parentFolder = ResolveFolder(path);
			var result = new List<ClientFileDescriptor>();

			if (parentFolder == null)
			{
				foreach (var folder in Media.Folders)
					result.Add(Create(folder));

				foreach (var file in Media.Files)
					result.Add(Create(file));
			}
			else
			{
				foreach (var folder in parentFolder.Folders)
					result.Add(Create(folder));

				foreach (var file in parentFolder.Files)
					result.Add(Create(file));
			}

			RenderResult(true, result);
		}

		private ClientFileDescriptor Create(IMediaResourceFile file)
		{
			var descriptor = new ClientFileDescriptor
			{
				HasSubDirectories = false,
				IsDirectory = false,
				Modified = file.Modified,
				Name = file.FileName,
				Size = file.Size
			};

			if (file.Thumb != Guid.Empty)
			{
				var blob = Tenant.GetService<IStorageService>().Select(file.Thumb);

				if (blob != null)
				{
					var ctx = new MicroServiceContext(Tenant.GetService<IMicroServiceService>().Select(blob.MicroService), Tenant.Url);

					descriptor.Icon = $"{ctx.Services.Routing.RootUrl}/sys/media/{blob.Token}/{blob.Version}";
				}
			}

			return descriptor;
		}

		private ClientFileDescriptor Create(IMediaResourceFolder folder)
		{
			return new ClientFileDescriptor
			{
				HasSubDirectories = folder.Folders.Count > 0,
				IsDirectory = true,
				Modified = folder.Modified,
				Name = folder.Name,
				Size = 0
			};
		}

		private void CreateDir()
		{
			Tenant.GetService<IVersionControlService>().Lock(Media.Component, Development.LockVerb.Edit);

			var parentId = Arguments["parentId"];
			var name = Arguments["name"];

			var parent = ResolveFolder(parentId);

			if (parent == null)
			{
				if (Media.Folders.FirstOrDefault(f => string.Compare(f.Name, name, true) == 0) != null)
				{
					RenderResult(false, ExceptionKind.FileNotFound);
					return;
				}

				Media.Folders.Add(new MediaResourceFolder
				{
					Modified = DateTime.UtcNow,
					Name = name
				});
			}
			else
			{
				if (parent.Folders.FirstOrDefault(f => string.Compare(f.Name, name, true) == 0) != null)
				{
					RenderResult(false, ExceptionKind.FileNotFound);
					return;
				}

				parent.Folders.Add(new MediaResourceFolder
				{
					Modified = DateTime.UtcNow,
					Name = name
				});

				UpdateModified(parent);
			}

			Tenant.GetService<IComponentDevelopmentService>().Update(Media);

			RenderResult(true);
		}

		private void Copy()
		{
			Tenant.GetService<IVersionControlService>().Lock(Media.Component, Development.LockVerb.Edit);

			var sourceId = Arguments["sourceId"];
			var destinationId = Arguments["destinationId"];

			if (IsFolder(sourceId))
				CopyFolder(sourceId, destinationId);
			else
				CopyFile(sourceId, destinationId);

			Tenant.GetService<IComponentDevelopmentService>().Update(Media);

			RenderResult(true);
		}

		private void CopyFolder(string sourceId, string destinationId)
		{
			var sourceFolder = ResolveFolder(sourceId);

			var destinationTokens = destinationId.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			var destinationPath = new StringBuilder();

			for (var i = 0; i < destinationTokens.Length - 1; i++)
				destinationPath.Append($"{destinationTokens[i]}/");

			var destinationFolder = ResolveFolder(destinationPath.ToString().Trim('/'));

			if (sourceFolder == null)
				return;

			if (destinationFolder == null)
			{
				if (Media.Folders.FirstOrDefault(f => string.Compare(f.Name, sourceFolder.Name, true) == 0) != null)
				{
					RenderResult(false, ExceptionKind.DirectoryExists);
					return;
				}
			}
			else
			{
				if (destinationFolder.Folders.FirstOrDefault(f => string.Compare(f.Name, sourceFolder.Name, true) == 0) != null)
				{
					RenderResult(false, ExceptionKind.DirectoryExists);
					return;
				}
			}

			CopyFolder(sourceFolder, destinationFolder);
		}

		private void CopyFolder(IMediaResourceFolder source, IMediaResourceFolder destination)
		{
			var newFolder = new MediaResourceFolder
			{
				Modified = DateTime.UtcNow,
				Name = source.Name
			};

			if (destination == null)
				Media.Folders.Add(newFolder);
			else
				destination.Folders.Add(newFolder);

			foreach (var file in source.Files)
				CopyFile(file, newFolder);

			foreach (var folder in source.Folders)
				CopyFolder(folder, newFolder);

			if (destination != null)
				UpdateModified(destination);
		}

		private void CopyFile(string sourceId, string destinationId)
		{
			var file = ResolveFile(sourceId);
			var folder = ResolveFolder(destinationId);

			if (file == null || folder == null)
				return;

			if (folder.Files.FirstOrDefault(f => string.Compare(file.FileName, f.FileName, true) == 0) != null)
			{
				RenderResult(false, ExceptionKind.FileExists);
				return;
			}

			CopyFile(file, folder);
		}

		private void CopyFile(IMediaResourceFile file, IMediaResourceFolder destination)
		{
			var newFile = new MediaResourceFile()
			{
				FileName = file.FileName,
				Modified = DateTime.UtcNow,
				Size = file.Size
			};

			destination.Files.Add(newFile);

			if (file.Blob != Guid.Empty)
			{
				var blob = Tenant.GetService<IStorageService>().Select(file.Blob);

				if (blob != null)
				{
					var content = Tenant.GetService<IStorageService>().Download(file.Blob);

					if (content != null)
					{
						newFile.Blob = Tenant.GetService<IStorageService>().Upload(new Blob
						{
							ContentType = blob.ContentType,
							FileName = blob.FileName,
							MicroService = blob.MicroService,
							PrimaryKey = newFile.Id.ToString(),
							ResourceGroup = blob.ResourceGroup,
							Topic = blob.Topic,
							Type = blob.Type
						}, content.Content, StoragePolicy.Singleton);
					}
				}
			}

			if (file.Thumb != Guid.Empty)
			{
				var blob = Tenant.GetService<IStorageService>().Select(file.Thumb);

				if (blob != null)
				{
					var content = Tenant.GetService<IStorageService>().Download(file.Thumb);

					if (content != null)
					{
						newFile.Thumb = Tenant.GetService<IStorageService>().Upload(new Blob
						{
							ContentType = blob.ContentType,
							FileName = blob.FileName,
							MicroService = blob.MicroService,
							PrimaryKey = $"t-{newFile.Id.ToString()}",
							ResourceGroup = blob.ResourceGroup,
							Topic = blob.Topic,
							Type = blob.Type
						}, content.Content, StoragePolicy.Singleton);
					}
				}
			}

			UpdateModified(destination);
		}

		private void AbordUpload()
		{
			var blobs = Tenant.GetService<IStorageService>().QueryDrafts(Arguments["uploadId"]);

			foreach (var blob in blobs)
				Tenant.GetService<IStorageService>().Delete(blob.Token);

			RenderResult(true);
		}

		private MediaCommand Command { get; set; }
		private Dictionary<string, string> Arguments { get; set; }
		private IMicroService MicroService { get; set; }
		private IMediaResourcesConfiguration Media { get; set; }
		private IComponent Component { get; set; }

		private IMediaResourceFolder ResolveFolder(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return null;

			var tokens = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			IMediaResourceFolder result = null;

			foreach (var token in tokens)
			{
				if (result == null)
					result = Media.Folders.FirstOrDefault(f => string.Compare(f.Name, token, true) == 0);
				else
					result = result.Folders.FirstOrDefault(f => string.Compare(f.Name, token, true) == 0);

				if (result == null)
				{
					RenderResult(false, ExceptionKind.FileNotFound);
					throw new NotFoundException(SR.ErrMediaFolderNotFound);
				}
			}

			return result;
		}

		private IMediaResourceFile ResolveFile(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return null;

			var tokens = path.Split("/");
			IMediaResourceFolder result = null;

			foreach (var token in tokens)
			{
				IMediaResourceFolder folder = null;

				if (result == null)
					folder = Media.Folders.FirstOrDefault(f => string.Compare(f.Name, token, true) == 0);
				else
					folder = result.Folders.FirstOrDefault(f => string.Compare(f.Name, token, true) == 0);

				if (folder == null)
				{
					if (result == null)
						return Media.Files.FirstOrDefault(f => string.Compare(f.FileName, token, true) == 0);
					else
						return result.Files.FirstOrDefault(f => string.Compare(f.FileName, token, true) == 0);
				}
				else
					result = folder;
			}

			return null;
		}

		private bool IsFolder(string path)
		{
			var tokens = path.Split('/');
			IMediaResourceFolder currentFolder = null;

			foreach (var token in tokens)
			{
				if (currentFolder == null)
				{
					var folder = Media.Folders.FirstOrDefault(f => string.Compare(f.Name, token, true) == 0);

					if (folder != null)
						currentFolder = folder;
					else
						return false;
				}
				else
				{
					var folder = currentFolder.Folders.FirstOrDefault(f => string.Compare(f.Name, token, true) == 0);

					if (folder != null)
						currentFolder = folder;
					else
						return false;
				}
			}

			return currentFolder != null;
		}

		private void RenderResult(bool success, List<ClientFileDescriptor> result)
		{
			if (_responseStarted)
				return;

			_responseStarted = true;

			var r = new JObject
			{
				{"success", success },
			};

			var a = new JArray();

			r.Add("result", a);

			foreach (var descriptor in result)
			{
				var o = new JObject
				{
					{ "name", descriptor.Name },
					{ "dateModified", descriptor.Modified },
					{ "isDirectory", descriptor.IsDirectory },
					{ "size", descriptor.Size },
					{ "hasSubDirectories", descriptor.HasSubDirectories },
					{ "icon", descriptor.Icon }
				};

				a.Add(o);
			}

			var content = Encoding.UTF8.GetBytes(Serializer.Serialize(r));

			Context.Response.ContentLength = content.Length;
			Context.Response.Body.Write(content, 0, content.Length);
		}

		private void RenderResult(bool success, ExceptionKind ex = ExceptionKind.None)
		{
			if (_responseStarted)
				return;

			_responseStarted = true;

			var r = new JObject
			{
				{"success", success }
			};

			if (ex != ExceptionKind.None)
				r.Add("errorId", (int)ex);

			var content = Encoding.UTF8.GetBytes(Serializer.Serialize(r));

			Context.Response.ContentLength = content.Length;
			Context.Response.Body.Write(content, 0, content.Length);
		}

		private void CreateThumbnail(IMediaResourceFile file)
		{
			if (file.Blob == Guid.Empty)
				return;

			var blob = Tenant.GetService<IStorageService>().Select(file.Blob);

			if (blob != null && blob.ContentType.StartsWith("image/"))
			{
				var content = Tenant.GetService<IStorageService>().Download(blob.Token);

				if (content != null)
				{
					using (var ms = new MemoryStream(content.Content))
					{
						var image = Image.FromStream(ms);

						if (image != null)
						{
							var thumbnail = Tenant.GetService<IGraphicsService>().Resize(image, 100, 100, true);

							file.Thumb = Tenant.GetService<IStorageService>().Upload(new Blob
							{
								ContentType = blob.ContentType,
								FileName = blob.FileName,
								MicroService = blob.MicroService,
								PrimaryKey = $"t{file.Id.ToString()}",
								ResourceGroup = blob.ResourceGroup,
								Type = blob.Type
							}, thumbnail, StoragePolicy.Singleton);
						}
					}
				}
			}
		}
	}
}