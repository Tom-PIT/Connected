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

				if (IsExternalResource(src) && !IsMediaImage(src))
					continue;

				if (newImages == null || newImages.Count == 0)
					DropResource(src);
				else
				{
					if (IsMediaImage(src))
					{
						if (newImages.FirstOrDefault(f => string.Compare(f.Attributes["src"].Value, MediaIdentifier(src), true) == 0) == null)
							DropResource(src);
					}
					else if (newImages.FirstOrDefault(f => string.Compare(f.Attributes["src"].Value, src, true) == 0) == null)
						DropResource(src);
				}
			}
		}

		public string ResourceUrl(string path)
		{
			var descriptor = ComponentDescriptor.Media(Context, path);

			descriptor.Validate();

			return GetUrl(FindFile(descriptor.Configuration, descriptor.Element.Split('/')).Blob);
		}

		public string SanitizeText(string text)
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(text);

			ReplaceImages(doc);
			Sanitize(doc);

			using var ms = new MemoryStream();
			using var w = new StreamWriter(ms);

			doc.Save(w);

			w.Flush();
			ms.Seek(0, SeekOrigin.Begin);

			using StreamReader sr = new StreamReader(ms);

			return sr.ReadToEnd();
		}

		private IMediaResourceFile FindFile(IMediaResourcesConfiguration media, IEnumerable<string> path)
		{
			if (path.Count() == 1)
			{
				var file = media.Files.FirstOrDefault(f => string.Compare(f.FileName, path.ElementAt(0), true) == 0);

				if (file == null)
					throw new RuntimeException($"{SR.ErrMediaFileNotFound} ({path.ElementAt(0)})");

				return file;
			}
			else
			{
				var folder = media.Folders.FirstOrDefault(f => string.Compare(f.Name, path.ElementAt(0), true) == 0);

				if (folder == null)
					throw new RuntimeException($"{SR.ErrMediaFolderNotFound} ({path.ElementAt(0)})");

				return FindFile(folder, path.Skip(1));
			}
		}

		private IMediaResourceFile FindFile(IMediaResourceFolder folder, IEnumerable<string> path)
		{
			if (path.Count() == 1)
			{
				var file = folder.Files.FirstOrDefault(f => string.Compare(f.FileName, path.ElementAt(0), true) == 0);

				if (file == null)
					throw new RuntimeException($"{SR.ErrMediaFileNotFound} ({path.ElementAt(0)})");

				return file;
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
			if (blob == Guid.Empty)
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
					if (IsMediaImage(att.Value))
					{
						var mediaImageTokens = att.Value.Split('/');

						att.Value = mediaImageTokens[^2];
						continue;
					}

					if (!att.Value.StartsWith("data:"))
						continue;

					var tokens = att.Value.Split(',');

					if (tokens == null || tokens.Length < 2)
						continue;

					var meta = tokens[0].Split(';');
					var imageData = tokens[^1];

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
						PrimaryKey = Guid.NewGuid().ToString(),
						Topic = "$HtmlImage",
						Type = BlobTypes.HtmlImage
					};

					att.Value = Context.Tenant.GetService<IStorageService>().Upload(blob, Convert.FromBase64String(imageData), StoragePolicy.Extended).ToString();
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

			if (Uri.TryCreate(url, UriKind.Absolute, out Uri _))
				return true;

			return false;
		}

		private void DropResource(string url)
		{
			if (IsMediaImage(url))
				Context.Services.Storage.Delete(MediaIdentifier(url));
			else
			{
				var tokens = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

				if (tokens.Length > 2)
					Context.Services.Storage.Delete(new Guid(tokens[2]));
			}
		}

		public string StripHtml(string htmlText)
		{
			if (string.IsNullOrWhiteSpace(htmlText))
				return string.Empty;

			var doc = new HtmlDocument();

			doc.LoadHtml(htmlText);

			if (doc == null)
				return htmlText;

			return doc.DocumentNode.InnerText;
		}

		private string MediaIdentifier(string url)
		{
			return url.Split('/')[^2];
		}

		private bool IsMediaImage(string url)
		{
			if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
				return false;

			var template = new Uri(Context.Services.Routing.Absolute($"~/sys/media/{Guid.NewGuid()}/1"), UriKind.Absolute);

			if (uri.Segments.Length != template.Segments.Length)
				return false;

			if (Uri.Compare(uri, template, UriComponents.Scheme | UriComponents.HostAndPort, UriFormat.UriEscaped, StringComparison.OrdinalIgnoreCase) != 0)
				return false;

			for (var i = 0; i < uri.Segments.Length - 2; i++)
			{
				if (string.Compare(uri.Segments[i], template.Segments[i], true) != 0)
					return false;
			}

			return true;
		}

		public IMediaResourceFile SelectFile(string path)
		{
			var descriptor = ComponentDescriptor.Media(Context, path);

			descriptor.Validate();

			return FindFile(descriptor.Configuration, descriptor.Element.Split('/'));
		}
	}
}