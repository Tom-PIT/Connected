using DevExpress.DataAccess.Json;

namespace TomPIT.MicroServices.Reporting.Storage
{
	internal class ReportParameterTag
	{
		public string DataMember { get; set; }
		public JsonDataSource DataSource { get; set; }
	}
}
