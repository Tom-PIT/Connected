using System;
using System.Drawing.Imaging;
using System.IO;
using DevExpress.Export;
using DevExpress.Utils;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Newtonsoft.Json.Linq;
using TomPIT.Cdn;
using TomPIT.Cdn.Documents;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Reports;
using TomPIT.MicroServices.Reporting.Storage;
using TomPIT.Middleware;
using TomPIT.Middleware.Interop;
using TomPIT.Serialization;

namespace TomPIT.MicroServices.Reporting.Printing
{
    internal class DocumentProvider : IDocumentProvider
    {
        public string Name => "DevEx";

        public IDocumentDescriptor Create(IPrintJob job)
        {
            var report = CreateReport(job);

            if (report == null)
                return null;

            using var ms = new MemoryStream();

            report.SaveLayoutToXml(ms);

            ms.Seek(0, SeekOrigin.Begin);

            return new DocumentDescriptor
            {
                Content = ms.ToArray(),
                MimeType = "devexpress/report"
            };
        }

        public void Print(IPrintJob job)
        {
            var report = CreateReport(job);

            if (report == null)
                return;

            var print = new PrintToolBase(report.PrintingSystem);

            if (OperatingSystem.IsWindows())
            {
                print.PrinterSettings.Copies = (short)job.CopyCount;

                print.Print();
            }
            else
            {
                for (int i = 0; i < job.CopyCount; i++)
                {
                    print.Print();
                }
            }
        }

        private static XtraReport CreateReport(IPrintJob job)
        {
            var args = Serializer.Deserialize<JObject>(job.Arguments);
            var arguments = Serializer.Deserialize<JObject>(args.Optional("arguments", string.Empty));

            var report = CreateReport(job.Component, arguments, job.User);

            if (report == null)
                return report;

            var printer = Serializer.Deserialize<Printer>(args.Required<string>("printer"));

            report.PrinterName = printer.Name;
            report.CreateDocument();

            return report;
        }

        private static XtraReport CreateReport(Guid component, object e, string user)
        {
            if (!(MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(component) is IReportConfiguration descriptor))
                return null;

            var report = new ReportCreateSession
            {
                Component = component,
                User = user,
                Arguments = e
            }.CreateReport();

            return report;
        }

        public IDocumentDescriptor Create(Guid report, DocumentCreateArgs e)
        {
            var rep = CreateReport(report, e.Arguments, e.User);

            if (rep == null)
                return null;

            rep.CreateDocument();

            var export = ExportReport(rep, e);

            return new DocumentDescriptor
            {
                Content = export.Item1,
                MimeType = export.Item2
            };
        }

        private static (byte[], string) ExportReport(XtraReport report, DocumentCreateArgs e)
        {
            using var ms = new MemoryStream();
            var mime = string.Empty;

            switch (e.Format)
            {
                case DocumentFormat.Csv:
                    mime = StreamOperationWriteArgs.MimeCsv;
                    ExportCsv(report, ms, e);
                    break;
                case DocumentFormat.Docx:
                    mime = StreamOperationWriteArgs.MimeDocx;
                    ExportDocx(report, ms, e);
                    break;
                case DocumentFormat.Html:
                    mime = StreamOperationWriteArgs.MimeHtml;
                    ExportHtml(report, ms, e);
                    break;
                case DocumentFormat.Image:
                    mime = ResolveImageMime(e);
                    ExportImage(report, ms, e);
                    break;
                case DocumentFormat.Mht:
                    mime = StreamOperationWriteArgs.MimeMht;
                    ExportMht(report, ms, e);
                    break;
                case DocumentFormat.Pdf:
                    mime = StreamOperationWriteArgs.MimePdf;
                    ExportPdf(report, ms, e);
                    break;
                case DocumentFormat.Rtf:
                    mime = StreamOperationWriteArgs.MimeRtf;
                    ExportRtf(report, ms, e);
                    break;
                case DocumentFormat.Text:
                    mime = StreamOperationWriteArgs.MimeText;
                    ExportText(report, ms, e);
                    break;
                case DocumentFormat.Xls:
                    mime = StreamOperationWriteArgs.MimeXls;
                    ExportXls(report, ms, e);
                    break;
                case DocumentFormat.Xlsx:
                    mime = StreamOperationWriteArgs.MimeXlsx;
                    ExportXlsx(report, ms, e);
                    break;
                default:
                    break;
            }

            ms.Seek(0, SeekOrigin.Begin);

            return (ms.ToArray(), mime);
        }

        private static void ExportCsv(XtraReport report, MemoryStream stream, DocumentCreateArgs e)
        {
            var args = e.Options as CsvOptions;

            var options = new CsvExportOptionsEx
            {
                DocumentCulture = args.DocumentCulture,
                EncodeExecutableContent = ToDefaultBoolean(args.EncodeExecutableContent),
                QuoteStringsWithSeparators = args.QuoteStringsWithSeparators,
                Encoding = args.Encoding,
                ExportType = ToExportType(args.Type),
                Separator = args.Separator,
                SkipEmptyColumns = args.SkipEmptyColumns,
                SkipEmptyRows = args.SkipEmptyRows,
                SuppressEmptyStrings = args.SuppressEmptyStrings,
                TextExportMode = ToTextExportMode(args.TextMode),
                WritePreamble = args.WritePreamble
            };

            report.ExportToCsv(stream, options);
        }

        private static void ExportDocx(XtraReport report, MemoryStream stream, DocumentCreateArgs e)
        {
            var args = e.Options as DocxOptions;

            var options = new DocxExportOptions
            {
                AllowFloatingPictures = args.AllowFloatingPictures,
                ExportMode = ToDocxExportMode(args.Mode),
                EmptyFirstPageHeaderFooter = args.EmptyFirstPageHeaderFooter,
                ExportPageBreaks = args.PageBreaks,
                ExportWatermarks = args.Watermarks,
                KeepRowHeight = args.KeepRowHeight,
                PageRange = args.PageRange,
                RasterizationResolution = args.RasterizationResolution,
                RasterizeImages = args.RasterizeImages,
                TableLayout = args.TableLayout
            };

            options.DocumentOptions.Author = args.DocumentOptions.Author;
            options.DocumentOptions.Category = args.DocumentOptions.Category;
            options.DocumentOptions.Comments = args.DocumentOptions.Comments;
            options.DocumentOptions.Keywords = args.DocumentOptions.Keywords;
            options.DocumentOptions.Subject = args.DocumentOptions.Subject;
            options.DocumentOptions.Title = args.DocumentOptions.Title;

            report.ExportToDocx(stream, options);
        }

        private static void ExportHtml(XtraReport report, MemoryStream stream, DocumentCreateArgs e)
        {
            var args = e.Options as HtmlOptions;

            var options = new HtmlExportOptions
            {
                AllowURLsWithJSContent = args.AllowURLsWithJSContent,
                CharacterSet = args.CharacterSet,
                EmbedImagesInHTML = args.EmbedImagesInHTML,
                ExportMode = ToHtmlExportMode(args.Mode),
                ExportWatermarks = args.Watermarks,
                InlineCss = args.InlineCss,
                PageBorderColor = args.PageBorderColor,
                PageBorderWidth = args.PageBorderWidth,
                PageRange = args.PageRange,
                RasterizationResolution = args.RasterizationResolution,
                RemoveSecondarySymbols = args.RemoveSecondarySymbols,
                TableLayout = args.TableLayout,
                Title = args.Title,
                UseHRefHyperlinks = args.UseHRefHyperlinks
            };

            report.ExportToHtml(stream, options);
        }

        private static void ExportImage(XtraReport report, MemoryStream stream, DocumentCreateArgs e)
        {
            var args = e.Options as ImageOptions;

            var options = new ImageExportOptions
            {
                ExportMode = ToImageExportMode(args.Mode),
                Format = args.Format == null ? ImageFormat.Png : args.Format,
                PageBorderColor = args.PageBorderColor,
                PageBorderWidth = args.PageBorderWidth,
                PageRange = args.PageRange,
                Resolution = args.Resolution,
                RetainBackgroundTransparency = args.RetainBackgroundTransparency,
                TextRenderingMode = ToTextRenderingMode(args.TextRenderingMode)
            };

            report.ExportToImage(stream, options);
        }

        private static void ExportMht(XtraReport report, MemoryStream stream, DocumentCreateArgs e)
        {
            var args = e.Options as MhtOptions;

            var options = new MhtExportOptions
            {
                AllowURLsWithJSContent = args.AllowURLsWithJSContent,
                CharacterSet = args.CharacterSet,
                ExportMode = ToHtmlExportMode(args.Mode),
                ExportWatermarks = args.Watermarks,
                InlineCss = args.InlineCss,
                PageBorderColor = args.PageBorderColor,
                PageBorderWidth = args.PageBorderWidth,
                PageRange = args.PageRange,
                RasterizationResolution = args.RasterizationResolution,
                RemoveSecondarySymbols = args.RemoveSecondarySymbols,
                TableLayout = args.TableLayout,
                Title = args.Title,
                UseHRefHyperlinks = args.UseHRefHyperlinks
            };

            report.ExportToMht(stream, options);
        }

        private static void ExportPdf(XtraReport report, MemoryStream stream, DocumentCreateArgs e)
        {
            var args = e.Options as PdfOptions;

            var options = new PdfExportOptions
            {
                AdditionalMetadata = args.AdditionalMetadata,
                ConvertImagesToJpeg = args.ConvertImagesToJpeg,
                ExportEditingFieldsToAcroForms = args.ExportEditingFieldsToAcroForms,
                ImageQuality = ToImageQuality(args.ImageQuality),
                NeverEmbeddedFonts = args.NeverEmbeddedFonts,
                PageRange = args.PageRange,
                PdfACompatibility = ToPdfACompatibility(args.PdfACompatibility),
                RasterizationResolution = args.RasterizationResolution,
                ShowPrintDialogOnOpen = args.ShowPrintDialogOnOpen,
            };

            foreach (var attachment in args.Attachments)
            {
                options.Attachments.Add(new DevExpress.XtraPrinting.PdfAttachment
                {
                    CreationDate = attachment.CreationDate,
                    Data = attachment.Data,
                    Description = attachment.Description,
                    FileName = attachment.FileName,
                    FilePath = attachment.FilePath,
                    ModificationDate = attachment.ModificationDate,
                    Relationship = ToAttachmentRelationship(attachment.Relationship),
                    Type = attachment.Type
                });
            }

            options.DocumentOptions.Application = args.DocumentOptions.Application;
            options.DocumentOptions.Author = args.DocumentOptions.Author;
            options.DocumentOptions.Keywords = args.DocumentOptions.Keywords;
            options.DocumentOptions.Producer = args.DocumentOptions.Producer;
            options.DocumentOptions.Subject = args.DocumentOptions.Subject;
            options.DocumentOptions.Title = args.DocumentOptions.Title;

            options.PasswordSecurityOptions.EncryptionLevel = ToEncryptionLevel(args.PasswordSecurityOptions.EncryptionLevel);
            options.PasswordSecurityOptions.OpenPassword = args.PasswordSecurityOptions.OpenPassword;

            options.PasswordSecurityOptions.PermissionsOptions.ChangingPermissions = ToChangingPermissions(args.PasswordSecurityOptions.PermissionsOptions.ChangingPermissions);
            options.PasswordSecurityOptions.PermissionsOptions.EnableCopying = args.PasswordSecurityOptions.PermissionsOptions.EnableCopying;
            options.PasswordSecurityOptions.PermissionsOptions.EnableScreenReaders = args.PasswordSecurityOptions.PermissionsOptions.EnableScreenReaders;
            options.PasswordSecurityOptions.PermissionsOptions.PrintingPermissions = ToPrintingPermissions(args.PasswordSecurityOptions.PermissionsOptions.PrintingPermissions);

            options.PasswordSecurityOptions.PermissionsPassword = args.PasswordSecurityOptions.PermissionsPassword;

            options.SignatureOptions.Certificate = args.SignatureOptions.Certificate;
            options.SignatureOptions.ContactInfo = args.SignatureOptions.ContactInfo;
            options.SignatureOptions.Location = args.SignatureOptions.Location;
            options.SignatureOptions.Reason = args.SignatureOptions.Reason;

            report.ExportToPdf(stream, options);
        }

        private static void ExportRtf(XtraReport report, MemoryStream stream, DocumentCreateArgs e)
        {
            var args = e.Options as RtfOptions;

            var options = new RtfExportOptions
            {
                EmptyFirstPageHeaderFooter = args.EmptyFirstPageHeaderFooter,
                ExportMode = ToRtfExportMode(args.Mode),
                ExportPageBreaks = args.PageBreaks,
                ExportWatermarks = args.Watermarks,
                KeepRowHeight = args.KeepRowHeight,
                PageRange = args.PageRange,
                RasterizationResolution = args.RasterizationResolution
            };

            report.ExportToRtf(stream, options);
        }

        private static void ExportText(XtraReport report, MemoryStream stream, DocumentCreateArgs e)
        {
            var args = e.Options as TextOptions;

            var options = new TextExportOptions
            {
                Encoding = args.Encoding,
                QuoteStringsWithSeparators = args.QuoteStringsWithSeparators,
                Separator = args.Separator,
                TextExportMode = ToTextExportMode(args.TextMode)
            };

            report.ExportToText(stream, options);
        }

        private static void ExportXls(XtraReport report, MemoryStream stream, DocumentCreateArgs e)
        {
            var args = e.Options as XlsOptions;

            var options = new XlsExportOptionsEx
            {
                AllowBandHeaderCellMerge = ToDefaultBoolean(args.AllowBandHeaderCellMerge),
                AllowCellMerge = ToDefaultBoolean(args.AllowCellMerge),
                AllowCombinedBandAndColumnHeaderCellMerge = ToDefaultBoolean(args.AllowCombinedBandAndColumnHeaderCellMerge),
                AllowConditionalFormatting = ToDefaultBoolean(args.AllowConditionalFormatting),
                AllowFixedColumnHeaderPanel = ToDefaultBoolean(args.AllowFixedColumnHeaderPanel),
                AllowFixedColumns = ToDefaultBoolean(args.AllowFixedColumns),
                AllowGrouping = ToDefaultBoolean(args.AllowGrouping),
                AllowHyperLinks = ToDefaultBoolean(args.AllowHyperLinks),
                AllowLookupValues = ToDefaultBoolean(args.AllowLookupValues),
                AllowSortingAndFiltering = ToDefaultBoolean(args.AllowSortingAndFiltering),
                AllowSparklines = ToDefaultBoolean(args.AllowSparklines),
                ApplyFormattingToEntireColumn = ToDefaultBoolean(args.ApplyFormattingToEntireColumn),
                AutoCalcConditionalFormattingIconSetMinValue = ToDefaultBoolean(args.AutoCalcConditionalFormattingIconSetMinValue),
                BandedLayoutMode = ToBandedLayoutMode(args.BandedLayoutMode),
                CalcTotalSummaryOnCompositeRange = args.CalcTotalSummaryOnCompositeRange,
                DocumentCulture = args.DocumentCulture,
                ExportMode = ToXlsExportMode(args.Mode),
                ExportType = ToExportType(args.Type),
                FitToPrintedPageHeight = args.FitToPrintedPageHeight,
                FitToPrintedPageWidth = args.FitToPrintedPageWidth,
                GroupState = ToGroupState(args.GroupState),
                IgnoreErrors = ToIgnoreErrors(args.IgnoreErrors),
                LayoutMode = ToLayoutMode(args.LayoutMode),
                PageRange = args.PageRange,
                RawDataMode = args.RawDataMode,
                RightToLeftDocument = ToDefaultBoolean(args.RightToLeftDocument),
                SheetName = args.SheetName,
                ShowBandHeaders = ToDefaultBoolean(args.ShowBandHeaders),
                ShowColumnHeaders = ToDefaultBoolean(args.ShowColumnHeaders),
                ShowGridLines = args.ShowGridLines,
                ShowGroupSummaries = ToDefaultBoolean(args.ShowGroupSummaries),
                ShowPageTitle = ToDefaultBoolean(args.ShowPageTitle),
                ShowTotalSummaries = ToDefaultBoolean(args.ShowTotalSummaries),
                SummaryCountBlankCells = args.SummaryCountBlankCells,
                Suppress256ColumnsWarning = args.Suppress256ColumnsWarning,
                Suppress65536RowsWarning = args.Suppress65536RowsWarning,
                SuppressEmptyStrings = args.SuppressEmptyStrings,
                SuppressHyperlinkMaxCountWarning = args.SuppressHyperlinkMaxCountWarning,
                TextExportMode = ToTextExportMode(args.TextMode),
                UnboundExpressionExportMode = ToUnboundExpressionExportMode(args.UnboundExpressionExportMode)
            };

            options.DocumentOptions.Application = args.DocumentOptions.Application;
            options.DocumentOptions.Author = args.DocumentOptions.Author;
            options.DocumentOptions.Category = args.DocumentOptions.Category;
            options.DocumentOptions.Comments = args.DocumentOptions.Comments;
            options.DocumentOptions.Company = args.DocumentOptions.Company;
            options.DocumentOptions.Subject = args.DocumentOptions.Subject;
            options.DocumentOptions.Tags = args.DocumentOptions.Tags;
            options.DocumentOptions.Title = args.DocumentOptions.Title;

            options.EncryptionOptions.Password = args.EncryptionOptions.Password;
            options.EncryptionOptions.Type = ToEncryptionType(args.EncryptionOptions.Type);

            report.ExportToXls(stream, options);
        }

        private static void ExportXlsx(XtraReport report, MemoryStream stream, DocumentCreateArgs e)
        {
            var args = e.Options as XlsxOptions;

            var options = new XlsxExportOptionsEx
            {
                AllowBandHeaderCellMerge = ToDefaultBoolean(args.AllowBandHeaderCellMerge),
                AllowCellMerge = ToDefaultBoolean(args.AllowCellMerge),
                AllowCombinedBandAndColumnHeaderCellMerge = ToDefaultBoolean(args.AllowCombinedBandAndColumnHeaderCellMerge),
                AllowConditionalFormatting = ToDefaultBoolean(args.AllowConditionalFormatting),
                AllowFixedColumnHeaderPanel = ToDefaultBoolean(args.AllowFixedColumnHeaderPanel),
                AllowFixedColumns = ToDefaultBoolean(args.AllowFixedColumns),
                AllowGrouping = ToDefaultBoolean(args.AllowGrouping),
                AllowHyperLinks = ToDefaultBoolean(args.AllowHyperLinks),
                AllowLookupValues = ToDefaultBoolean(args.AllowLookupValues),
                AllowSortingAndFiltering = ToDefaultBoolean(args.AllowSortingAndFiltering),
                AllowSparklines = ToDefaultBoolean(args.AllowSparklines),
                ApplyFormattingToEntireColumn = ToDefaultBoolean(args.ApplyFormattingToEntireColumn),
                AutoCalcConditionalFormattingIconSetMinValue = ToDefaultBoolean(args.AutoCalcConditionalFormattingIconSetMinValue),
                BandedLayoutMode = ToBandedLayoutMode(args.BandedLayoutMode),
                CalcTotalSummaryOnCompositeRange = args.CalcTotalSummaryOnCompositeRange,
                DocumentCulture = args.DocumentCulture,
                ExportMode = ToXlsxExportMode(args.Mode),
                ExportType = ToExportType(args.Type),
                FitToPrintedPageHeight = args.FitToPrintedPageHeight,
                FitToPrintedPageWidth = args.FitToPrintedPageWidth,
                GroupState = ToGroupState(args.GroupState),
                IgnoreErrors = ToIgnoreErrors(args.IgnoreErrors),
                LayoutMode = ToLayoutMode(args.LayoutMode),
                PageRange = args.PageRange,
                RawDataMode = args.RawDataMode,
                RightToLeftDocument = ToDefaultBoolean(args.RightToLeftDocument),
                SheetName = args.SheetName,
                ShowBandHeaders = ToDefaultBoolean(args.ShowBandHeaders),
                ShowColumnHeaders = ToDefaultBoolean(args.ShowColumnHeaders),
                ShowGridLines = args.ShowGridLines,
                ShowGroupSummaries = ToDefaultBoolean(args.ShowGroupSummaries),
                ShowPageTitle = ToDefaultBoolean(args.ShowPageTitle),
                ShowTotalSummaries = ToDefaultBoolean(args.ShowTotalSummaries),
                SummaryCountBlankCells = args.SummaryCountBlankCells,
                SuppressEmptyStrings = args.SuppressEmptyStrings,
                SuppressHyperlinkMaxCountWarning = args.SuppressHyperlinkMaxCountWarning,
                TextExportMode = ToTextExportMode(args.TextMode),
                UnboundExpressionExportMode = ToUnboundExpressionExportMode(args.UnboundExpressionExportMode),
                SuppressMaxColumnsWarning = args.SuppressMaxColumnsWarning,
                SuppressMaxRowsWarning = args.SuppressMaxRowsWarning
            };

            options.DocumentOptions.Application = args.DocumentOptions.Application;
            options.DocumentOptions.Author = args.DocumentOptions.Author;
            options.DocumentOptions.Category = args.DocumentOptions.Category;
            options.DocumentOptions.Comments = args.DocumentOptions.Comments;
            options.DocumentOptions.Company = args.DocumentOptions.Company;
            options.DocumentOptions.Subject = args.DocumentOptions.Subject;
            options.DocumentOptions.Tags = args.DocumentOptions.Tags;
            options.DocumentOptions.Title = args.DocumentOptions.Title;

            options.EncryptionOptions.Password = args.EncryptionOptions.Password;
            options.EncryptionOptions.Type = ToEncryptionType(args.EncryptionOptions.Type);

            report.ExportToXlsx(stream, options);
        }

        private static UnboundExpressionExportMode ToUnboundExpressionExportMode(DocumentUnboundExpressionMode value)
        {
            switch (value)
            {
                case DocumentUnboundExpressionMode.AsValue:
                    return UnboundExpressionExportMode.AsValue;
                case DocumentUnboundExpressionMode.AsFormula:
                    return UnboundExpressionExportMode.AsFormula;
                default:
                    throw new NotSupportedException();
            }
        }

        private static LayoutMode ToLayoutMode(DocumentLayoutMode value)
        {
            switch (value)
            {
                case DocumentLayoutMode.Standard:
                    return LayoutMode.Standard;
                case DocumentLayoutMode.Table:
                    return LayoutMode.Table;
                default:
                    throw new NotSupportedException();
            }
        }

        private static XlIgnoreErrors ToIgnoreErrors(DocumentXlsIgnoreErrors value)
        {
            switch (value)
            {
                case DocumentXlsIgnoreErrors.None:
                    return XlIgnoreErrors.None;
                case DocumentXlsIgnoreErrors.NumberStoredAsText:
                    return XlIgnoreErrors.NumberStoredAsText;
                default:
                    throw new NotSupportedException();
            }
        }

        private static GroupState ToGroupState(DocumentGroupState value)
        {
            switch (value)
            {
                case DocumentGroupState.Default:
                    return GroupState.Default;
                case DocumentGroupState.ExpandAll:
                    return GroupState.ExpandAll;
                case DocumentGroupState.CollapseAll:
                    return GroupState.CollapseAll;
                default:
                    throw new NotSupportedException();
            }
        }

        private static XlsExportMode ToXlsExportMode(DocumentXlsMode value)
        {
            switch (value)
            {
                case DocumentXlsMode.SingleFile:
                    return XlsExportMode.SingleFile;
                case DocumentXlsMode.SingleFilePageByPage:
                    return XlsExportMode.SingleFilePageByPage;
                case DocumentXlsMode.DifferentFiles:
                    return XlsExportMode.DifferentFiles;
                default:
                    throw new NotSupportedException();
            }
        }

        private static XlsxExportMode ToXlsxExportMode(DocumentXlsxMode value)
        {
            switch (value)
            {
                case DocumentXlsxMode.SingleFile:
                    return XlsxExportMode.SingleFile;
                case DocumentXlsxMode.SingleFilePageByPage:
                    return XlsxExportMode.SingleFilePageByPage;
                case DocumentXlsxMode.DifferentFiles:
                    return XlsxExportMode.DifferentFiles;
                default:
                    throw new NotSupportedException();
            }
        }

        private static XlEncryptionType ToEncryptionType(DocumentXlsEncryptionType value)
        {
            switch (value)
            {
                case DocumentXlsEncryptionType.Compatible:
                    return XlEncryptionType.Compatible;
                case DocumentXlsEncryptionType.Strong:
                    return XlEncryptionType.Strong;
                default:
                    throw new NotSupportedException();
            }
        }

        private static BandedLayoutMode ToBandedLayoutMode(DocumentBandedLayoutMode value)
        {
            switch (value)
            {
                case DocumentBandedLayoutMode.Default:
                    return BandedLayoutMode.Default;
                case DocumentBandedLayoutMode.LinearBandsAndColumns:
                    return BandedLayoutMode.LinearBandsAndColumns;
                case DocumentBandedLayoutMode.LinearColumns:
                    return BandedLayoutMode.LinearColumns;
                default:
                    throw new NotSupportedException();
            }
        }

        private static RtfExportMode ToRtfExportMode(DocumentRtfMode value)
        {
            switch (value)
            {
                case DocumentRtfMode.SingleFile:
                    return RtfExportMode.SingleFile;
                case DocumentRtfMode.SingleFilePageByPage:
                    return RtfExportMode.SingleFilePageByPage;
                default:
                    throw new NotSupportedException();
            }
        }

        private static PdfACompatibility ToPdfACompatibility(DocumentPdfACompatibility value)
        {
            switch (value)
            {
                case DocumentPdfACompatibility.None:
                    return PdfACompatibility.None;
                case DocumentPdfACompatibility.PdfA1b:
                    return PdfACompatibility.PdfA1b;
                case DocumentPdfACompatibility.PdfA2b:
                    return PdfACompatibility.PdfA2b;
                case DocumentPdfACompatibility.PdfA3b:
                    return PdfACompatibility.PdfA3b;
                default:
                    throw new NotSupportedException();
            }
        }

        private static PrintingPermissions ToPrintingPermissions(DocumentPrintingPermissions value)
        {
            switch (value)
            {
                case DocumentPrintingPermissions.None:
                    return PrintingPermissions.None;
                case DocumentPrintingPermissions.LowResolution:
                    return PrintingPermissions.LowResolution;
                case DocumentPrintingPermissions.HighResolution:
                    return PrintingPermissions.HighResolution;
                default:
                    throw new NotSupportedException();
            }
        }

        private static ChangingPermissions ToChangingPermissions(DocumentChangingPermissions value)
        {
            switch (value)
            {
                case DocumentChangingPermissions.None:
                    return ChangingPermissions.None;
                case DocumentChangingPermissions.InsertingDeletingRotating:
                    return ChangingPermissions.InsertingDeletingRotating;
                case DocumentChangingPermissions.FillingSigning:
                    return ChangingPermissions.FillingSigning;
                case DocumentChangingPermissions.CommentingFillingSigning:
                    return ChangingPermissions.CommentingFillingSigning;
                case DocumentChangingPermissions.AnyExceptExtractingPages:
                    return ChangingPermissions.AnyExceptExtractingPages;
                default:
                    throw new NotSupportedException();
            }
        }

        private static PdfEncryptionLevel ToEncryptionLevel(DocumentPdfEncryptionLevel value)
        {
            switch (value)
            {
                case DocumentPdfEncryptionLevel.AES128:
                    return PdfEncryptionLevel.AES128;
                case DocumentPdfEncryptionLevel.AES256:
                    return PdfEncryptionLevel.AES256;
                case DocumentPdfEncryptionLevel.ARC4:
                    return PdfEncryptionLevel.ARC4;
                default:
                    throw new NotSupportedException();
            }
        }

        private static PdfJpegImageQuality ToImageQuality(DocumentPdfJpegImageQuality value)
        {
            switch (value)
            {
                case DocumentPdfJpegImageQuality.Lowest:
                    return PdfJpegImageQuality.Lowest;
                case DocumentPdfJpegImageQuality.Low:
                    return PdfJpegImageQuality.Low;
                case DocumentPdfJpegImageQuality.Medium:
                    return PdfJpegImageQuality.Medium;
                case DocumentPdfJpegImageQuality.High:
                    return PdfJpegImageQuality.High;
                case DocumentPdfJpegImageQuality.Highest:
                    return PdfJpegImageQuality.Highest;
                default:
                    throw new NotSupportedException();
            }
        }

        private static PdfAttachmentRelationship ToAttachmentRelationship(DocumentPdfAttachmentRelationship value)
        {
            switch (value)
            {
                case DocumentPdfAttachmentRelationship.Alternative:
                    return PdfAttachmentRelationship.Alternative;
                case DocumentPdfAttachmentRelationship.Data:
                    return PdfAttachmentRelationship.Data;
                case DocumentPdfAttachmentRelationship.Source:
                    return PdfAttachmentRelationship.Source;
                case DocumentPdfAttachmentRelationship.Supplement:
                    return PdfAttachmentRelationship.Supplement;
                case DocumentPdfAttachmentRelationship.Unspecified:
                    return PdfAttachmentRelationship.Unspecified;
                default:
                    throw new NotSupportedException();
            }
        }

        private static TextRenderingMode ToTextRenderingMode(DocumentTextRenderingMode value)
        {
            switch (value)
            {
                case DocumentTextRenderingMode.SystemDefault:
                    return TextRenderingMode.SystemDefault;
                case DocumentTextRenderingMode.SingleBitPerPixelGridFit:
                    return TextRenderingMode.SingleBitPerPixelGridFit;
                case DocumentTextRenderingMode.SingleBitPerPixel:
                    return TextRenderingMode.SingleBitPerPixel;
                case DocumentTextRenderingMode.AntiAliasGridFit:
                    return TextRenderingMode.AntiAliasGridFit;
                case DocumentTextRenderingMode.AntiAlias:
                    return TextRenderingMode.AntiAlias;
                case DocumentTextRenderingMode.ClearTypeGridFit:
                    return TextRenderingMode.ClearTypeGridFit;
                default:
                    throw new NotSupportedException();
            }
        }

        private static ImageExportMode ToImageExportMode(DocumentImageMode value)
        {
            switch (value)
            {
                case DocumentImageMode.SingleFile:
                    return ImageExportMode.SingleFile;
                case DocumentImageMode.SingleFilePageByPage:
                    return ImageExportMode.SingleFilePageByPage;
                case DocumentImageMode.DifferentFiles:
                    return ImageExportMode.DifferentFiles;
                default:
                    throw new NotSupportedException();
            }
        }
        private static HtmlExportMode ToHtmlExportMode(DocumentHtmlMode value)
        {
            switch (value)
            {
                case DocumentHtmlMode.SingleFile:
                    return HtmlExportMode.SingleFile;
                case DocumentHtmlMode.SingleFilePageByPage:
                    return HtmlExportMode.SingleFilePageByPage;
                case DocumentHtmlMode.DifferentFiles:
                    return HtmlExportMode.DifferentFiles;
                default:
                    throw new NotSupportedException();
            }
        }

        private static DocxExportMode ToDocxExportMode(DocumentDocxMode value)
        {
            switch (value)
            {
                case DocumentDocxMode.SingleFile:
                    return DocxExportMode.SingleFile;
                case DocumentDocxMode.SingleFilePageByPage:
                    return DocxExportMode.SingleFilePageByPage;
                default:
                    throw new NotSupportedException();
            }
        }

        private static TextExportMode ToTextExportMode(DocumentTextMode value)
        {
            switch (value)
            {
                case DocumentTextMode.Value:
                    return TextExportMode.Value;
                case DocumentTextMode.Text:
                    return TextExportMode.Text;
                default:
                    throw new NotSupportedException();
            }
        }

        private static ExportType ToExportType(DocumentType value)
        {
            switch (value)
            {
                case DocumentType.Default:
                    return ExportType.Default;
                case DocumentType.DataAware:
                    return ExportType.DataAware;
                case DocumentType.WYSIWYG:
                    return ExportType.WYSIWYG;
                default:
                    throw new NotSupportedException();
            }
        }

        private static DefaultBoolean ToDefaultBoolean(DocumentBoolean value)
        {
            switch (value)
            {
                case DocumentBoolean.True:
                    return DefaultBoolean.True;
                case DocumentBoolean.False:
                    return DefaultBoolean.False;
                case DocumentBoolean.Default:
                    return DefaultBoolean.Default;
                default:
                    throw new NotSupportedException();
            }
        }

        private static string ResolveImageMime(DocumentCreateArgs e)
        {
            if (e.Options is not ImageOptions options)
                return StreamOperationWriteArgs.MimePng;

            if (options.Format is null)
                return StreamOperationWriteArgs.MimePng;

            if (options.Format == ImageFormat.Jpeg)
                return StreamOperationWriteArgs.MimeJpeg;
            else if (options.Format == ImageFormat.Gif)
                return StreamOperationWriteArgs.MimeGif;
            else if (options.Format == ImageFormat.Bmp)
                return StreamOperationWriteArgs.MimeBmp;
            else if (options.Format == ImageFormat.Tiff)
                return StreamOperationWriteArgs.MimeTiff;
            else if (options.Format == ImageFormat.Emf)
                return StreamOperationWriteArgs.MimeEmf;
            else if (options.Format == ImageFormat.Exif)
                return StreamOperationWriteArgs.MimeTiff;
            else if (options.Format == ImageFormat.Icon)
                return StreamOperationWriteArgs.MimeIcon;
            else if (options.Format == ImageFormat.Png)
                return StreamOperationWriteArgs.MimePng;
            else if (options.Format == ImageFormat.MemoryBmp)
                return StreamOperationWriteArgs.MimeBmp;
            else if (options.Format == ImageFormat.Wmf)
                return StreamOperationWriteArgs.MimeWmf;
            else
                return StreamOperationWriteArgs.MimePng;
        }
    }
}
