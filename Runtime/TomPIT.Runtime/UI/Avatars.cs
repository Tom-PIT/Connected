using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace TomPIT.UI
{
    internal class Avatars : IAvatars
    {
        private static readonly List<SKColor> Colors = new List<SKColor>();

        private static Random _r = new Random();

        static Avatars()
        {
            Colors.AddRange(new SKColor[] {
               new SKColor(33,150,243),
               new SKColor(255,87,34),
               new SKColor(76,175,80),
               new SKColor(244,67,54)
                        });
        }

        public byte[] Create(string text, int width, int height)
        {
            lock (_r)
            {
                var c = Colors[_r.Next(Colors.Count)];

                return Create(text, width, height, c);
            }
        }

        public byte[] Create(string text, int width, int height, SKColor color)
        {
            if (text.Length > 2)
                text = text.Substring(0, 2);

            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);

            canvas.Clear(SKColor.Empty);

            var dark = new HslColor(color);
            dark.Darken();

            var paint = new SKPaint
            {
                Color = color
            };

            canvas.DrawRect(new SKRect(1, 1, 1, 1), paint);
            canvas.ClipRect(new SKRect(1, 1, 1, 1));
            canvas.DrawColor(color);

            using var typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold);

            var emSize = text.Length > 1 ? height / 3 : height / 2;

            using var f = new SKFont(typeface, emSize);

            using var fontPaint = new SKPaint(f);

            fontPaint.IsAntialias = true;
            fontPaint.Color = new SKColor(255, 255, 255);
            fontPaint.IsStroke = true;
            fontPaint.StrokeWidth = 3;
            fontPaint.TextAlign = SKTextAlign.Center;

            canvas.DrawText(text, width / 2f, height / 2, paint);

            canvas.Flush();

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            using var ms = new MemoryStream();
            data.SaveTo(ms);
            ms.Position = 0;

            return ms.ToArray();

            //sf.Alignment = StringAlignment.Center;
            //sf.LineAlignment = StringAlignment.Center;
            //sf.Trimming = StringTrimming.EllipsisCharacter;

            //g.DrawString(text.ToUpper(), f, Brushes.White, rcf, sf);


            //using (var b = new SKBitmap(width, height))
            //{
            //    using (var g = new SKGraphics().FromImage(b))
            //    {
            //        g.Clear(SKColor.Transparent);

            //        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            //        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            //        g.TextContrast = 12;

            //        var rc = new Rectangle(0, 0, b.Width, b.Height);
            //        var dark = new HslColor(SKColor);

            //        dark.Darken();

            //        using (var brush = new SKSolidBrush(SKColor))
            //        {
            //            g.FillRectangle(brush, 0, 0, b.Width - 1, b.Height - 1);

            //            rc = new Rectangle(0, 0, b.Width - 1, b.Height - 1);

            //            using (var p = new SKPen(dark))
            //            {
            //                g.DrawRectangle(p, rc);
            //            }
            //        }

            //        var emSize = text.Length > 1 ? height / 3 : height / 2;

            //        using (var f = new SKFont("Segoe UI", emSize, FontStyle.Bold))
            //        {
            //            var size = g.MeasureString(text, f);
            //            var rcf = new RectangleF(0, (b.Height - size.Height) / 2f + 2, b.Width, size.Height);
            //            var sf = new StringFormat();

            //            sf.Alignment = StringAlignment.Center;
            //            sf.LineAlignment = StringAlignment.Center;
            //            sf.Trimming = StringTrimming.EllipsisCharacter;

            //            g.DrawString(text.ToUpper(), f, Brushes.White, rcf, sf);
            //        }

            //        g.Flush();

            //        using var ms = new MemoryStream();

            //        ms.Seek(0, SeekOrigin.Begin);
            //        b.Save(ms, ImageFormat.Png);
            //        ms.Seek(0, SeekOrigin.Begin);

            //        return ms.ToArray();
        }
    }
}
