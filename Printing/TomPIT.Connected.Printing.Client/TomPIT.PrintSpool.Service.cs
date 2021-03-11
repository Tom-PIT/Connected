using System.ServiceProcess;
using TomPIT.Connected.Printing.Client.Handlers;

namespace TomPIT.Connected.Printing.Client
{
    partial class PrinterSpoolerService : ServiceBase
    {
        private PrintingHandler _printingHandler;
        public PrinterSpoolerService()
        {
            InitializeComponent();

            _printingHandler = new PrintingHandler();
        }

        protected override void OnStart(string[] args)
        {
            _printingHandler.Start();
        }

        protected override void OnStop()
        {
            _printingHandler.Stop();
        }
    }
}
