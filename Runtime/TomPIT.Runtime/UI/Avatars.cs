using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace TomPIT.UI
{
	internal class Avatars : IAvatars
	{
		private static readonly List<Color> Colors = new List<Color>();

		private static Random _r = new Random();

		static Avatars()
		{
			Colors.AddRange(new Color[] {
				Color.Blue,
				Color.Orange,
				Color.Green,
				Color.Red
						});
		}

		public byte[] Create(string text, int width, int height)
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

				return Create(text, width, height, c);
			}
		}

		public byte[] Create(string text, int width, int height, Color color)
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

					using var ms = new MemoryStream();

					ms.Seek(0, SeekOrigin.Begin);
					b.Save(ms, ImageFormat.Png);
					ms.Seek(0, SeekOrigin.Begin);

					return ms.ToArray();
				}
			}
		}
	}
}
