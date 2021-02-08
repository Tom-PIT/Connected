using System;
using Microsoft.AspNetCore.StaticFiles;

namespace TomPIT.Middleware.Interop
{
	public class StreamOperationWriteArgs : EventArgs
	{
		private string _fileName = string.Empty;

		public const string MimeJavaScript = "application/javascript";
		public const string MimeJson = "application/json";
		public const string MimeMP4 = "application/mp4";
		public const string MimePdf = "application/pdf";
		public const string MimeRtf = "application/rtf";
		public const string MimeZip = "application/zip";
		public const string MimeGZip = "application/gzip";
		public const string MimeXml = "text/xml";
		public const string MimeBmp = "image/bmp";
		public const string MimeWmf = "image/wmf";
		public const string MimePng = "image/png";
		public const string MimeSvg = "image/svg+xml";
		public const string MimeTiff = "image/tiff";
		public const string MimeIcon = "image/x-icon";
		public const string MimeJpeg = "image/jpeg";
		public const string MimeGif = "image/gif";
		public const string MimeEmf = "image/emf";
		public const string MimeCalendar = "text/calendar";
		public const string MimeCss = "text/css";
		public const string MimeCsv = "text/csv";
		public const string MimeHtml = "text/html";
		public const string MimeDocx = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
		public const string MimeMht = "multipart/related";
		public const string MimeText = "text/plain";
		public const string MimeXls = "application/vnd.ms-excel";
		public const string MimeXlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

		public string ContentType { get; set; }
		public DateTime Modified { get; set; }
		public byte[] Content { get; set; }
		public bool Inline { get; set; } = true;
		public string FileName
		{
			get
			{
				return _fileName;
			}
			set
			{
				_fileName = value;

				if (!string.IsNullOrWhiteSpace(value) && string.IsNullOrWhiteSpace(ContentType))
				{
					if (new FileExtensionContentTypeProvider().TryGetContentType(_fileName, out string ct))
						ContentType = ct;
				}
			}
		}
	}
}