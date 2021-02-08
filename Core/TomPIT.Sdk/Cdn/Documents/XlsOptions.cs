using System.Globalization;

namespace TomPIT.Cdn.Documents
{
	public enum DocumentXlsIgnoreErrors
	{
		None = 0,
		NumberStoredAsText = 64
	}

	public enum DocumentWorkbookColorPaletteCompliance
	{
		AdjustColorsToDefaultPalette = 0,
		ReducePaletteForExactColors = 1
	}

	public enum DocumentXlsMode
	{
		SingleFile = 0,
		SingleFilePageByPage = 1,
		DifferentFiles = 2
	}

	public enum DocumentBandedLayoutMode
	{
		Default = 0,
		LinearBandsAndColumns = 1,
		LinearColumns = 2
	}

	public enum DocumentUnboundExpressionMode
	{
		AsValue = 0,
		AsFormula = 1
	}

	public enum DocumentLayoutMode
	{
		Standard = 0,
		Table = 1
	}

	public enum DocumentType
	{
		Default = 0,
		DataAware = 1,
		WYSIWYG = 2
	}

	public enum DocumentGroupState
	{
		Default = 0,
		ExpandAll = 1,
		CollapseAll = 2
	}

	public class XlsOptions : XlOptions
	{
		public DocumentWorkbookColorPaletteCompliance WorkbookColorPaletteCompliance { get; set; } = DocumentWorkbookColorPaletteCompliance.ReducePaletteForExactColors;
		public bool Suppress256ColumnsWarning { get; set; }
		public bool Suppress65536RowsWarning { get; set; }
		public DocumentXlsMode Mode { get; set; } = DocumentXlsMode.SingleFile;
		public DocumentBoolean AllowFixedColumnHeaderPanel { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean AllowFixedColumns { get; set; } = DocumentBoolean.Default;
		public bool SummaryCountBlankCells { get; set; }
		public CultureInfo DocumentCulture { get; set; } = CultureInfo.CurrentUICulture;
		public DocumentBandedLayoutMode BandedLayoutMode { get; set; }
		public DocumentUnboundExpressionMode UnboundExpressionExportMode { get; set; }
		public bool SuppressEmptyStrings { get; set; }
		public bool CalcTotalSummaryOnCompositeRange { get; set; }
		public DocumentLayoutMode LayoutMode { get; set; }
		public DocumentType Type { get; set; } = DocumentType.DataAware;
		public DocumentBoolean AllowLookupValues { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean AllowBandHeaderCellMerge { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean AllowHyperLinks { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean AllowSortingAndFiltering { get; set; } = DocumentBoolean.Default;
		public bool SuppressHyperlinkMaxCountWarning { get; set; }
		public DocumentBoolean AllowCombinedBandAndColumnHeaderCellMerge { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean AllowCellMerge { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean AutoCalcConditionalFormattingIconSetMinValue { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean AllowGrouping { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean AllowSparklines { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean AllowConditionalFormatting { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean ShowTotalSummaries { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean ShowGroupSummaries { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean ShowPageTitle { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean ShowColumnHeaders { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean ShowBandHeaders { get; set; } = DocumentBoolean.Default;
		public DocumentBoolean ApplyFormattingToEntireColumn { get; set; } = DocumentBoolean.True;
		public DocumentGroupState GroupState { get; set; } = DocumentGroupState.Default;
	}
}
