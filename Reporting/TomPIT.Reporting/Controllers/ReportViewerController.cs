using System.Threading.Tasks;
using DevExpress.AspNetCore.Reporting.WebDocumentViewer;
using DevExpress.AspNetCore.Reporting.WebDocumentViewer.Native.Services;
using Microsoft.AspNetCore.Mvc;

namespace TomPIT.MicroServices.Reporting.Controllers
{
	public class ReportViewerController : WebDocumentViewerController
	{
		public ReportViewerController(IWebDocumentViewerMvcControllerService controllerService) : base(controllerService) { }
		public override Task<IActionResult> Invoke()
		{
			return base.Invoke();
		}
	}
}