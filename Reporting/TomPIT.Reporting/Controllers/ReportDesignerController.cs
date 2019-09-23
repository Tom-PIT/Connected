using System.Threading.Tasks;
using DevExpress.AspNetCore.Reporting.ReportDesigner;
using DevExpress.AspNetCore.Reporting.ReportDesigner.Native.Services;
using Microsoft.AspNetCore.Mvc;

namespace TomPIT.MicroServices.Reporting.Controllers
{
	public class DesignerController : ReportDesignerController
	{
		public DesignerController(IReportDesignerMvcControllerService controllerService) : base(controllerService) { }
		public override Task<IActionResult> Invoke()
		{
			return base.Invoke();
		}
	}
}