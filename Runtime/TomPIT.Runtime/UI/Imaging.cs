using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace TomPIT.UI
{
	internal class Imaging : IImaging
	{
		private const float ImageResolution = 72f;
		private const long CompressionLevel = 80L;

		public byte[] Resize(Image image, int maxWidth, int maxHeight, bool padImage)
		{
			int newWidth;
			int newHeight;

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

			if (padImage)
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

			using var graphics = Graphics.FromImage(newImage);

			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality.HighQuality;
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

			graphics.DrawImage(image, 0, 0, newWidth, newHeight);

			using var ms = new MemoryStream();
			var encoderParameters = new EncoderParameters(1);

			encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, CompressionLevel);

			newImage.Save(ms, EncoderInfo("image/jpeg"), encoderParameters);

			return ms.ToArray();
		}

		private static Image PadImage(Image image, Color backColor)
		{
			var maxSize = Math.Max(image.Height, image.Width);
			var squareSize = new Size(maxSize, maxSize);
			var squareImage = new Bitmap(squareSize.Width, squareSize.Height);

			using var graphics = Graphics.FromImage(squareImage);

			graphics.FillRectangle(new SolidBrush(backColor), 0, 0, squareSize.Width, squareSize.Height);
			graphics.DrawImage(image, (squareSize.Width / 2) - (image.Width / 2), (squareSize.Height / 2) - (image.Height / 2), image.Width, image.Height);

			return squareImage;
		}

		private static ImageCodecInfo EncoderInfo(string mimeType)
		{
			var encoders = ImageCodecInfo.GetImageEncoders();

			return encoders.FirstOrDefault(f => string.Compare(f.MimeType, mimeType, true) == 0);
		}

		private static RotateFlipType Rotate(int rotateValue)
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
	}
}
