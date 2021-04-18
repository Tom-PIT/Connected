using System.Globalization;

namespace TomPIT.Cdn.Documents
{
	public enum DocumentXlsxMode
	{
		SingleFile = 0,
		SingleFilePageByPage = 1,
		DifferentFiles = 2
	}

	public class XlsxOptions : XlOptions
	{
		public DocumentXlsxMode Mode { get; set; } = DocumentXlsxMode.SingleFile;
		public DocumentBoolean AllowLookupValues { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean AllowFixedColumns { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean AllowHyperLinks { get; set; } = DocumentBoolean.Default;
		public bool SummaryCountBlankCells { get; set; }
		public bool CalcTotalSummaryOnCompositeRange { get; set; }
		public DocumentLayoutMode LayoutMode { get; set; }
		public CultureInfo DocumentCulture { get; set; } = CultureInfo.CurrentUICulture;
		public DocumentBandedLayoutMode BandedLayoutMode { get; set; }
		public DocumentUnboundExpressionMode UnboundExpressionExportMode { get; set; }
		public bool SuppressEmptyStrings { get; set; }
		public DocumentType Type { get; set; } = DocumentType.DataAware;
		public DocumentBoolean AllowBandHeaderCellMerge { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean AllowCellMerge { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean AllowFixedColumnHeaderPanel { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean AutoCalcConditionalFormattingIconSetMinValue { get; set; } = DocumentBoolean.Default;
		public bool SuppressMaxColumnsWarning { get; set; }
		public bool SuppressHyperlinkMaxCountWarning { get; set; }
		public DocumentBoolean AllowSortingAndFiltering { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean AllowCombinedBandAndColumnHeaderCellMerge { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean AllowGrouping { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean AllowSparklines { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean ApplyFormattingToEntireColumn { get; set; } = DocumentBoolean.True;
		public bool SuppressMaxRowsWarning { get; set; } 
		public DocumentBoolean ShowTotalSummaries { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean ShowGroupSummaries { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean ShowPageTitle { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean ShowColumnHeaders { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean ShowBandHeaders { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean AllowConditionalFormatting { get; set; } = DocumentBoolean.Default;
		public DocumentGroupState GroupState { get; set; } = DocumentGroupState.Default;
	}
}
