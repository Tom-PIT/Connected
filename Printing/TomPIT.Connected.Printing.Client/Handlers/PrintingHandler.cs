using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TomPIT.Connected.Printing.Client.Printing;

namespace TomPIT.Connected.Printing.Client.Handlers
{
    internal class PrintingHandler
    {
        private HubConnection _connection;

        private void CreateConnection()
        {
            _connection = new HubConnectionBuilder().WithUrl(new Uri(Settings.CdnUrl), (o) =>
            {
                o.AccessTokenProvider = () =>
                {
                    return Task.FromResult("Token");
                };
            }).WithAutomaticReconnect().Build();

            _connection.Closed += async (error) =>
            {
                await Task.Delay(1000);
                await _connection.StartAsync();
            };


            _connection.On<Guid>(Constants.RequestPrint, (id) =>
            {
                var job = SelectJob(id);

                if (job != null)
                    Print(job);

                Complete(id);
            });
        }

        public void Start()
        {
            Logging.Info("Starting print spooler...");

            if (_connection is null)
                CreateConnection();

            _connection.StartAsync().Wait();

            AddPrinters(new List<string>
            {
                "HPE9367C (HP Officejet Pro 8620)"
            });
        }

        public void Stop()
        {
            Logging.Info("Stopping print spooler...");
        }

        private void Print(SpoolerJob job)
        {
            if (string.Compare(job.Mime, Constants.MimeTypeReport, true) == 0)
            {
                var report = new XtraReport();
                using var ms = new MemoryStream(Convert.FromBase64String(job.Content));

                report.LoadLayoutFromXml(ms);

                report.PrinterName = job.Printer;
                report.CreateDocument();

                var print = new PrintToolBase(report.PrintingSystem);

                print.Print();
            }
        }

        private void Complete(Guid id)
        {
            if (_connection.State != HubConnectionState.Connected)
                return;

            _connection.SendAsync(Constants.ServerMethodNameComplete, id).Wait();
        }

        private void Ping(Guid id)
        {
            if (_connection.State != HubConnectionState.Connected)
                return;

            _connection.SendAsync(Constants.ServerMethodNamePing, id).Wait();
        }

        private void AddPrinters(List<string> printers)
        {
            var args = new List<object>();

            foreach (var printer in printers)
            {
                args.Add(new
                {
                    Name = printer
                });
            }

            _connection.SendAsync(Constants.ServerMethodNameAddPrinters, args).Wait();
        }

        private SpoolerJob SelectJob(Guid id)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.HttpHeaderBearer, "Token");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.MimeTypeJson));

            var result = client.PostAsync($"Cdn/sys/printing-spooler", new StringContent(JsonConvert.SerializeObject(new
            {
                Id = id
            }), Encoding.UTF8, Constants.MimeTypeJson)).Result;

            var content = result.Content.ReadAsStringAsync().Result;

            if (string.Compare(content, "null", true) == 0
                || string.IsNullOrWhiteSpace(content))
                return null;

            return JsonConvert.DeserializeObject<SpoolerJob>(content);
        }
    }

}
