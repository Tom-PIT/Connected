using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using TomPIT.Middleware;
using TomPIT.Middleware.Services;

namespace TomPIT.Storage
{
	public static class StorageExtensions
	{
		public static void SaveUploadedFiles(this IMiddlewareStorageService service, string primaryKey, string topic)
		{
			SaveUploadedFiles(service, primaryKey, null, topic);
		}
		public static void SaveUploadedFileAsDrafts(this IMiddlewareStorageService service, string draft, string topic)
		{
			SaveUploadedFiles(service, null, draft, topic);
		}

		public static Guid SaveUploadedFile(this IMiddlewareStorageService service, string primaryKey, string topic, IFormFile file)
		{
			return SaveUploadedFile(service, file, primaryKey, null, topic);
		}

		public static Guid SaveUploadedFileAsDraft(this IMiddlewareStorageService service, string draft, string topic, IFormFile file)
		{
			return SaveUploadedFile(service, file, null, draft, topic);
		}

		private static void SaveUploadedFiles(this IMiddlewareStorageService service, string primaryKey, string draft, string topic)
		{
			if (Shell.HttpContext == null)
				return;

			var files = Shell.HttpContext.Request.Form.Files;

			if (files == null || files.Count == 0)
				return;

			foreach (var file in files)
				SaveUploadedFile(service, file, primaryKey, draft, topic);
		}

		private static Guid SaveUploadedFile(this IMiddlewareStorageService service, IFormFile file, string primaryKey, string draft, string topic)
		{
			var b = new Blob
			{
				ContentType = file.ContentType,
				FileName = Path.GetFileName(file.FileName),
				Size = Convert.ToInt32(file.Length),
				Draft = draft,
				PrimaryKey = primaryKey,
				Type = BlobTypes.UserContent,
				Topic = topic
			};

			using (var s = new MemoryStream())
			{
				file.CopyTo(s);

				var buffer = new byte[file.Length];

				s.Seek(0, SeekOrigin.Begin);
				s.Read(buffer, 0, buffer.Length);

				return MiddlewareDescriptor.Current.Tenant.GetService<IStorageService>().Upload(b, buffer, StoragePolicy.Singleton);
			}
		}
	}
}
