using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Exceptions;
using TomPIT.Storage;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareMediaService : MiddlewareObject, IMiddlewareMediaService
	{
		public MiddlewareMediaService(IMiddlewareContext context) : base(context)
		{
		}

		public void CleanOrphanedResources(string existingText, string newText)
		{
			if (string.IsNullOrWhiteSpace(existingText))
				return;

			var existingImages = ParseImages(existingText);
			var newImages = ParseImages(newText);

			if (existingImages == null || existingImages.Count == 0)
				return;

			foreach (var i in existingImages)
			{
				var src = i.Attributes["src"].Value;

				if (string.IsNullOrWhiteSpace(src))
					continue;

				if (IsExternalResource(src))
					continue;

				if (newImages == null || newImages.Count == 0)
					DropResource(src);
				else
				{
					if (newImages.FirstOrDefault(f => string.Compare(f.Attributes["src"].Value, src, true) == 0) == null)
						DropResource(src);
				}
			}
		}

		public string ResourceUrl(string path)
		{
			var tokens = path.Split('/');

			Context.MicroService.ValidateMicroServiceReference(tokens[0]);

			var ms = Context.Tenant.GetService<IMicroServiceService>().Select(tokens[0]);

			if (ms == null)
				throw new RuntimeException($"{SR.ErrMicroServiceNotFound} ({tokens[0]})");

			var component = tokens[1];
			var media = Context.Tenant.GetService<IComponentService>().SelectConfiguration(ms.Token, "Media", component) as IMediaResourcesConfiguration;

			return FindFile(media, tokens.Skip(2));
		}

		public string SanitizeText(string text)
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(text);

			ReplaceImages(doc);
			Sanitize(doc);

			using (var ms = new MemoryStream())
			using (var w = new StreamWriter(ms))
			{
				doc.Save(w);

				w.Flush();
				ms.Seek(0, SeekOrigin.Begin);

				using (StreamReader sr = new StreamReader(ms))
				{
					return sr.ReadToEnd();
				}
			}
		}

		private string FindFile(IMediaResourcesConfiguration media, IEnumerable<string> path)
		{
			if (path.Count() == 1)
			{
				var file = media.Files.FirstOrDefault(f => string.Compare(f.FileName, path.ElementAt(0), true) == 0);

				if (file == null)
					throw new RuntimeException($"{SR.ErrMediaFileNotFound} ({path.ElementAt(0)})");

				return GetUrl(file.Blob);
			}
			else
			{
				var folder = media.Folders.FirstOrDefault(f => string.Compare(f.Name, path.ElementAt(0), true) == 0);

				if (folder == null)
					throw new RuntimeException($"{SR.ErrMediaFolderNotFound} ({path.ElementAt(0)})");

				return FindFile(folder, path.Skip(1));
			}
		}

		private string FindFile(IMediaResourceFolder folder, IEnumerable<string> path)
		{
			if (path.Count() == 1)
			{
				var file = folder.Files.FirstOrDefault(f => string.Compare(f.FileName, path.ElementAt(0), true) == 0);

				if (file == null)
					throw new RuntimeException($"{SR.ErrMediaFileNotFound} ({path.ElementAt(0)})");

				return GetUrl(file.Blob);
			}
			else
			{
				var subFolder = folder.Folders.FirstOrDefault(f => string.Compare(f.Name, path.ElementAt(0), true) == 0);

				if (subFolder == null)
					throw new RuntimeException($"{SR.ErrMediaFolderNotFound} ({path.ElementAt(0)})");

				return FindFile(subFolder, path.Skip(1));
			}
		}

		private string GetUrl(Guid blob)
		{
			if (blob == null)
				return null;

			var b = Context.Tenant.GetService<IStorageService>().Select(blob);

			if (b == null)
				return null;

			return $"{Context.Services.Routing.RootUrl}/sys/media/{blob}/{b.Version}";
		}

		private void ReplaceImages(HtmlDocument doc)
		{
			var images = doc.DocumentNode.SelectNodes(@"//img[@src]");

			if (images == null || images.Count == 0)
				return;

			foreach (var i in images)
			{
				var att = i.Attributes["src"];

				if (att != null)
				{
					if (!att.Value.StartsWith("data:"))
						continue;

					var tokens = att.Value.Split(',');

					if (tokens == null || tokens.Length < 2)
						continue;

					var meta = tokens[0].Split(';');
					var imageData = tokens[tokens.Length - 1];

					if (meta == null || meta.Length == 0)
						continue;

					var mimeTokens = meta[0].Split(':');

					if (mimeTokens == null || mimeTokens.Length < 2)
						continue;

					var mime = mimeTokens[1].Trim();
					var blob = new Blob
					{
						ContentType = mime,
						FileName = "htmlImage",
						MicroService = Context.MicroService.Token,
						PrimaryKey = Guid.NewGuid().ToString(),
						ResourceGroup = Context.MicroService.ResourceGroup,
						Topic = "$HtmlImage",
						Type = BlobTypes.HtmlImage
					};

					var imageId = Context.Tenant.GetService<IStorageService>().Upload(blob, Convert.FromBase64String(imageData), StoragePolicy.Extended);

					att.Value = $"/sys/media/{imageId}/0";
				}
			}
		}

		private void Sanitize(HtmlDocument doc)
		{
			var nc = doc.DocumentNode.SelectNodes("//script|//link|//iframe|//frameset|//frame|//applet|//object|//embed");

			if (nc != null)
			{
				foreach (var node in nc)
					node.ParentNode.RemoveChild(node, false);
			}

			//remove hrefs to java/j/vbscript URLs
			nc = doc.DocumentNode.SelectNodes("//a[starts-with(translate(@href, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'javascript')]|//a[starts-with(translate(@href, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'jscript')]|//a[starts-with(translate(@href, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'vbscript')]");

			if (nc != null)
			{
				foreach (var node in nc)
					node.SetAttributeValue("href", "#");
			}

			//remove img with refs to java/j/vbscript URLs
			nc = doc.DocumentNode.SelectNodes("//img[starts-with(translate(@src, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'javascript')]|//img[starts-with(translate(@src, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'jscript')]|//img[starts-with(translate(@src, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'vbscript')]");

			if (nc != null)
			{
				foreach (var node in nc)
					node.SetAttributeValue("src", "#");
			}

			//remove on<Event> handlers from all tags
			nc = doc.DocumentNode.SelectNodes("//*[@onclick or @onmouseover or @onfocus or @onblur or @onmouseout or @ondoubleclick or @onload or @onunload]");

			if (nc != null)
			{
				foreach (var node in nc)
				{
					node.Attributes.Remove("onFocus");
					node.Attributes.Remove("onBlur");
					node.Attributes.Remove("onClick");
					node.Attributes.Remove("onMouseOver");
					node.Attributes.Remove("onMouseOut");
					node.Attributes.Remove("onDoubleClick");
					node.Attributes.Remove("onLoad");
					node.Attributes.Remove("onUnload");
				}
			}

			// remove any style attributes that contain the word expression (IE evaluates this as script)
			nc = doc.DocumentNode.SelectNodes("//*[contains(translate(@style, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'expression')]");

			if (nc != null)
			{
				foreach (var node in nc)
					node.Attributes.Remove("style");
			}

			// remove any style attributes that contain the word expression (IE evaluates this as script)
			nc = doc.DocumentNode.SelectNodes("//base[@href]");

			if (nc != null)
			{
				foreach (var node in nc)
					node.ParentNode.RemoveChild(node, false);
			}
		}

		private HtmlNodeCollection ParseImages(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return null;

			var doc = new HtmlDocument();

			doc.LoadHtml(text);

			return doc.DocumentNode.SelectNodes("//img[@src]");
		}

		private bool IsExternalResource(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
				return false;

			if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
				return true;

			return false;
		}

		private void DropResource(string url)
		{
			var tokens = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

			if (tokens.Length > 2)
				Context.Services.Storage.Delete(new Guid(tokens[2]));
		}

		public string StripHtml(string htmlText)
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(htmlText);

			if (doc == null)
				return htmlText;

			return doc.DocumentNode.InnerText;
		}
	}
}
