using HtmlAgilityPack;
using System;
using System.IO;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Services;
using TomPIT.Storage;

namespace TomPIT.Annotations
{
	internal class HtmlTextSanitizer
	{
		public HtmlTextSanitizer(IExecutionContext context, IElement element)
		{
			Element = element;
			Context = context;
		}

		private IElement Element { get; }
		private IExecutionContext Context { get; }

		public void Drop(string value)
		{
			DropOrphanedImages(value, null);
		}

		public void Sanitize(string existingValue, string value)
		{
			DropOrphanedImages(existingValue, value);

			var doc = new HtmlDocument();

			doc.LoadHtml(value);

			SanitizeTags(doc);
			ProcessImages(doc);
			NormalizeLinks(doc);

			SaveDocument(doc);
		}

		private void SaveDocument(HtmlDocument doc)
		{
			Result = SaveDocumentContent(doc);
		}

		public string Result { get; private set; }

		private void ProcessImages(HtmlDocument doc)
		{
			var images = doc.DocumentNode.SelectNodes(@"//img[@src]");

			if (images == null || images.Count == 0)
				return;

			foreach (var i in images)
			{
				var att = i.Attributes["src"];
				var att1 = i.Attributes["data-src"];

				if (att1 != null && string.Compare(att1.Value, "resources", true) == 0)
				{
					var url = att.Value;

					url = string.Format("~{0}", url.Substring(url.IndexOf("/res/")));

					att.Value = url;

					continue;
				}

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

				string mime = mimeTokens[1].Trim();

				var blob = new Blob
				{
					ContentType = mime,
					FileName = Element.Id.ToString(),
					Type = BlobTypes.HtmlImage,
					PrimaryKey = string.Format("{0}.{1}", Element.ToString(), ResolveConfiguration(Element).Component.ToString()),
					MicroService = Context.MicroService()
				};

				Shell.GetService<IStorageService>().Upload(blob, Convert.FromBase64String(imageData), StoragePolicy.Extended);

				var b = Shell.GetService<IStorageService>().Select(blob.Token);

				att.Value = string.Format("~/sys/res/{0}/{1}", b.Version.AsString(), b.Token.AsString());
				i.Attributes.Add("data-src", "resources");
			}
		}

		private IConfiguration ResolveConfiguration(IElement element)
		{
			if (element == null)
				return null;

			if (element is IConfiguration || element.GetType().IsAssignableFrom(typeof(IConfiguration)))
				return element as IConfiguration;

			return ResolveConfiguration(element.Parent);
		}

		private static string SaveDocumentContent(HtmlDocument doc)
		{
			using (var ms = new MemoryStream())
			using (var w = new StreamWriter(ms))
			{
				doc.OptionWriteEmptyNodes = true;
				doc.Save(w);

				w.Flush();
				ms.Seek(0, SeekOrigin.Begin);

				using (var sr = new StreamReader(ms))
				{
					return sr.ReadToEnd();
				}
			}
		}

		private void NormalizeLinks(HtmlDocument doc)
		{
			try
			{
				var nodes = doc.DocumentNode.SelectNodes("//a[@href]");

				if (nodes == null)
					return;

				foreach (var i in nodes)
				{
					var att = i.Attributes["href"];

					if (att == null)
						continue;

					if (!Uri.TryCreate(att.Value, UriKind.RelativeOrAbsolute, out Uri uri))
						continue;

					if (!uri.IsAbsoluteUri)
						continue;

					att.Value = uri.ToString();
				}
			}
			catch { }
		}

		public void SanitizeTags(HtmlDocument doc)
		{
			//Remove potentially harmful elements
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
				foreach (HtmlNode node in nc)
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

		private void DropOrphanedImages(string existingValue, string proposedValue)
		{
			if (string.IsNullOrWhiteSpace(existingValue))
				return;

			var existingImages = QueryImages(existingValue);
			var newImages = QueryImages(proposedValue);

			if (existingImages == null || existingImages.Count == 0)
				return;

			foreach (var i in existingImages)
			{
				string src = i.Attributes["src"].Value;

				if (string.IsNullOrWhiteSpace(src))
					continue;

				if (Uri.TryCreate(src, UriKind.RelativeOrAbsolute, out Uri r) && r.IsAbsoluteUri)
					continue;

				if (newImages == null || newImages.Count == 0)
					DropImage(src);
				else
				{
					if (newImages.FirstOrDefault(f => string.Compare(f.Attributes["src"].Value, src, true) == 0) == null)
						DropImage(src);
				}
			}
		}

		private void DropImage(string src)
		{
			var tokens = src.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

			if (tokens.Length == 3)
			{
				var id = tokens[2];
				var token = id.AsGuid();

				if (token != Guid.Empty)
					Shell.GetService<IStorageService>().Delete(token);
			}
		}
		private HtmlNodeCollection QueryImages(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return null;

			var doc = new HtmlDocument();

			doc.LoadHtml(text);

			return doc.DocumentNode.SelectNodes("//img[@src]");
		}

	}
}
