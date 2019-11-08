using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace TomPIT.UI
{
	internal class GraphicsService : IGraphicsService
	{
		private const float ImageResolution = 72f;
		private const long CompressionLevel = 80L;

		private static Random _r = new Random();
		private static readonly List<Color> Colors = new List<Color>();

		public GraphicsService()
		{

		}
		static GraphicsService()
		{
			Colors.AddRange(new Color[] {
				Color.Blue,
				Color.Orange,
				Color.Green,
				Color.Red
						});
		}

		public byte[] CreateImage(string text, int width, int height)
		{
			lock (_r)
			{
				var c = Colors[_r.Next(Colors.Count)];

				if (c == Color.Blue)
					c = Color.FromArgb(33, 150, 243);
				else if (c == Color.Orange)
					c = Color.FromArgb(255, 87, 34);
				else if (c == Color.Green)
					c = Color.FromArgb(76, 175, 80);
				else if (c == Color.Red)
					c = Color.FromArgb(244, 67, 54);

				return CreateImage(text, width, height, c);
			}
		}

		public byte[] CreateImage(string text, int width, int height, Color color)
		{
			if (text.Length > 2)
				text = text.Substring(0, 2);

			using (var b = new Bitmap(width, height))
			{
				using (var g = Graphics.FromImage(b))
				{
					g.Clear(Color.Transparent);

					g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
					g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
					g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
					g.TextContrast = 12;

					var rc = new Rectangle(0, 0, b.Width, b.Height);
					var dark = new HslColor(color);

					dark.Darken();

					using (var brush = new SolidBrush(color))
					{
						g.FillRectangle(brush, 0, 0, b.Width - 1, b.Height - 1);

						rc = new Rectangle(0, 0, b.Width - 1, b.Height - 1);

						using (var p = new Pen(dark))
						{
							g.DrawRectangle(p, rc);
						}
					}

					var emSize = text.Length > 1 ? height / 3 : height / 2;

					using (var f = new Font("Segoe UI", emSize, FontStyle.Bold))
					{
						var size = g.MeasureString(text, f);
						var rcf = new RectangleF(0, (b.Height - size.Height) / 2f + 2, b.Width, size.Height);
						var sf = new StringFormat();

						sf.Alignment = StringAlignment.Center;
						sf.LineAlignment = StringAlignment.Center;
						sf.Trimming = StringTrimming.EllipsisCharacter;

						g.DrawString(text.ToUpper(), f, Brushes.White, rcf, sf);
					}

					g.Flush();

					using (var ms = new MemoryStream())
					{
						ms.Seek(0, SeekOrigin.Begin);
						b.Save(ms, ImageFormat.Png);
						ms.Seek(0, SeekOrigin.Begin);

						return ms.ToArray();
					}
				}
			}
		}

		public byte[] Resize(Image image, int maxWidth, int maxHeight, bool padImage)
		{
			var newWidth = 0;
			var newHeight = 0;

			foreach (var prop in image.PropertyItems)
			{
				if (prop.Id == 0x0112)
				{
					var orientationValue = image.GetPropertyItem(prop.Id).Value[0];
					var rotateFlipType = Rotate(orientationValue);

					image.RotateFlip(rotateFlipType);

					break;
				}
			}

			if (padImage == true)
				image = PadImage(image, Color.White);

			if (image.Width > maxWidth || image.Height > maxHeight)
			{
				var ratioX = (double)maxWidth / image.Width;
				var ratioY = (double)maxHeight / image.Height;
				var ratio = Math.Min(ratioX, ratioY);

				newWidth = (int)(image.Width * ratio);
				newHeight = (int)(image.Height * ratio);
			}
			else
			{
				newWidth = image.Width;
				newHeight = image.Height;
			}

			var newImage = new Bitmap(newWidth, newHeight);

			newImage.SetResolution(ImageResolution, ImageResolution);

			using (var graphics = Graphics.FromImage(newImage))
			{
				graphics.CompositingMode = CompositingMode.SourceCopy;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

				graphics.DrawImage(image, 0, 0, newWidth, newHeight);
			}

			using (var ms = new MemoryStream())
			{
				var encoderParameters = new EncoderParameters(1);

				encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, CompressionLevel);

				newImage.Save(ms, EncoderInfo("image/jpeg"), encoderParameters);

				return ms.ToArray();
			}
		}

		private Image PadImage(Image image, Color backColor)
		{
			var maxSize = Math.Max(image.Height, image.Width);
			var squareSize = new Size(maxSize, maxSize);
			var squareImage = new Bitmap(squareSize.Width, squareSize.Height);

			using (var graphics = Graphics.FromImage(squareImage))
			{
				graphics.FillRectangle(new SolidBrush(backColor), 0, 0, squareSize.Width, squareSize.Height);
				graphics.DrawImage(image, (squareSize.Width / 2) - (image.Width / 2), (squareSize.Height / 2) - (image.Height / 2), image.Width, image.Height);
			}

			return squareImage;
		}

		private ImageCodecInfo EncoderInfo(string mimeType)
		{
			var encoders = ImageCodecInfo.GetImageEncoders();

			return encoders.FirstOrDefault(f => string.Compare(f.MimeType, mimeType, true) == 0);
		}

		private RotateFlipType Rotate(int rotateValue)
		{
			var flip = RotateFlipType.RotateNoneFlipNone;

			switch (rotateValue)
			{
				case 1:
					flip = RotateFlipType.RotateNoneFlipNone;
					break;
				case 2:
					flip = RotateFlipType.RotateNoneFlipX;
					break;
				case 3:
					flip = RotateFlipType.Rotate180FlipNone;
					break;
				case 4:
					flip = RotateFlipType.Rotate180FlipX;
					break;
				case 5:
					flip = RotateFlipType.Rotate90FlipX;
					break;
				case 6:
					flip = RotateFlipType.Rotate90FlipNone;
					break;
				case 7:
					flip = RotateFlipType.Rotate270FlipX;
					break;
				case 8:
					flip = RotateFlipType.Rotate270FlipNone;
					break;
				default:
					flip = RotateFlipType.RotateNoneFlipNone;
					break;
			}

			return flip;
		}


		//private string ToBase64(Image image)
		//{
		//	using (var ms = new MemoryStream())
		//	{
		//		image.Save(ms, ImageFormat.Jpeg);
		//		var buffer = ms.ToArray();

		//		return Convert.ToBase64String(buffer);
		//	}
		//}
	}
}
