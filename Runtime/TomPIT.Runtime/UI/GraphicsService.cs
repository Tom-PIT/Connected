using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace TomPIT.UI
{
	internal class GraphicsService : IGraphicsService
	{
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
				Color c = Colors[_r.Next(Colors.Count)];

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

			using (Bitmap b = new Bitmap(width, height))
			{
				using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(b))
				{
					g.Clear(Color.Transparent);

					g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
					g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
					g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
					g.TextContrast = 12;

					Rectangle rc = new Rectangle(0, 0, b.Width, b.Height);

					HslColor dark = new HslColor(color);

					dark.Darken();

					using (SolidBrush brush = new SolidBrush(color))
					{
						g.FillRectangle(brush, 0, 0, b.Width - 1, b.Height - 1);

						rc = new Rectangle(0, 0, b.Width - 1, b.Height - 1);

						using (Pen p = new Pen(dark))
						{
							g.DrawRectangle(p, rc);
						}
					}

					float emSize = text.Length > 1 ? height / 3 : height / 2;

					using (Font f = new Font("Segoe UI", emSize, FontStyle.Bold))
					{
						SizeF size = g.MeasureString(text, f);

						RectangleF rcf = new RectangleF(0, ((float)b.Height - size.Height) / 2f + 2, (float)b.Width, (float)size.Height);

						StringFormat sf = new StringFormat();

						sf.Alignment = StringAlignment.Center;
						sf.LineAlignment = StringAlignment.Center;
						sf.Trimming = StringTrimming.EllipsisCharacter;

						g.DrawString(text.ToUpper(), f, Brushes.White, rcf, sf);
					}

					g.Flush();

					using (MemoryStream ms = new MemoryStream())
					{
						ms.Seek(0, SeekOrigin.Begin);
						b.Save(ms, ImageFormat.Png);
						ms.Seek(0, SeekOrigin.Begin);

						return ms.ToArray();
					}
				}
			}
		}
	}
}
