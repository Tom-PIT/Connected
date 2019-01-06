using System;
using System.IO;
using System.IO.Compression;

namespace TomPIT
{
	public static class Compress
	{
		public static byte[] ZipRaw(byte[] bytes)
		{
			if (bytes == null || bytes.Length == 0)
				return bytes;

			using (var msi = new MemoryStream(bytes))
			using (var mso = new MemoryStream())
			{
				using (var gs = new GZipStream(mso, CompressionMode.Compress))
				{
					copyTo(msi, gs);
				}

				return mso.ToArray();
			}
		}

		public static string Zip(byte[] bytes)
		{
			return Convert.ToBase64String(ZipRaw(bytes));
		}

		public static byte[] Unzip(byte[] bytes)
		{
			if (bytes == null || bytes.Length == 0)
				return null;

			using (var mso = new MemoryStream())
			{
				mso.Write(bytes, 0, bytes.Length);
				mso.Seek(0, SeekOrigin.Begin);

				return Unzip(mso);
			}
		}

		public static byte[] Unzip(Stream ms)
		{
			using (var mso = new MemoryStream())
			{
				using (var gs = new GZipStream(ms, CompressionMode.Decompress))
				{
					copyTo(gs, mso);
				}

				return mso.ToArray();
			}
		}

		public static byte[] Unzip(string value)
		{
			using (var msi = new MemoryStream(Convert.FromBase64String(value)))
			using (var mso = new MemoryStream())
			{
				using (var gs = new GZipStream(msi, CompressionMode.Decompress))
				{
					copyTo(gs, mso);
				}

				return mso.ToArray();
			}
		}

		private static void copyTo(Stream src, Stream dest)
		{
			var bytes = new byte[4096];
			var cnt = 0;

			while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
				dest.Write(bytes, 0, cnt);
		}
	}
}
