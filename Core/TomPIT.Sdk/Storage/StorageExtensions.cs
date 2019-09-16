using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using TomPIT.Middleware;

namespace TomPIT.Storage
{
	public static class StorageExtensions
	{
		public static void SaveUploadedFiles(this IMiddlewareContext context, string primaryKey)
		{
			SaveUploadedFiles(context, primaryKey, null);
		}
		public static void SaveUploadedFileAsDrafts(this IMiddlewareContext context, string draft)
		{
			SaveUploadedFiles(context, null, draft);
		}

		public static Guid SaveUploadedFile(this IMiddlewareContext context, string primaryKey, IFormFile file)
		{
			return SaveUploadedFile(context, file, primaryKey, null);
		}
		public static Guid SaveUploadedFileAsDraft(this IMiddlewareContext context, string draft, IFormFile file)
		{
			return SaveUploadedFile(context, file, null, draft);
		}

		private static void SaveUploadedFiles(this IMiddlewareContext context, string primaryKey, string draft)
		{
			if (Shell.HttpContext == null)
				return;

			var files = Shell.HttpContext.Request.Form.Files;

			if (files == null || files.Count == 0)
				return;

			foreach (var file in files)
				SaveUploadedFile(context, file, primaryKey, draft);
		}

		private static Guid SaveUploadedFile(this IMiddlewareContext context, IFormFile file, string primaryKey, string draft)
		{
			var ms = context.MicroService;

			var b = new Blob
			{
				ContentType = file.ContentType,
				FileName = Path.GetFileName(file.FileName),
				MicroService = ms.Token,
				Size = Convert.ToInt32(file.Length),
				ResourceGroup = ms.ResourceGroup,
				Draft = draft,
				PrimaryKey = primaryKey,
				Type = BlobTypes.UserContent
			};

			using (var s = new MemoryStream())
			{
				file.CopyTo(s);

				var buffer = new byte[file.Length];

				s.Seek(0, SeekOrigin.Begin);
				s.Read(buffer, 0, buffer.Length);

				return context.Tenant.GetService<IStorageService>().Upload(b, buffer, StoragePolicy.Singleton);
			}
		}
	}
}
