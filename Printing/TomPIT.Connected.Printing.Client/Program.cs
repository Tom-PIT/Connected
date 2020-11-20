using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace TomPIT.Connected.Printing.Client
{
	class Program
	{
		private static readonly string Token = Convert.ToBase64String(Encoding.UTF8.GetBytes("c00588c4-a3e8-4ed4-95e8-e995ba18ab5f"));
		private static readonly string Cdn = "http://localhost:44018";
		static void Main(string[] args)
		{

			Connection = new HubConnectionBuilder().WithUrl(new Uri($"{Cdn}/printing"), (o) =>
			{
				o.AccessTokenProvider = () =>
				{
					return Task.FromResult(Token);
				};
			}).WithAutomaticReconnect().Build();

			Connection.Closed += async (error) =>
			{
				await Task.Delay(1000);
				await Connection.StartAsync();
			};


			Connection.On<Guid>("print", (id) =>
			{
				var job = SelectJob(id);

				if (job != null)
					Print(job);

				Complete(id);
			});

			Connection.StartAsync().Wait();

			AddPrinters(new List<string>
			{
				"HPE9367C (HP Officejet Pro 8620)"
			});

			Console.ReadLine();
		}

		private static void Print(SpoolerJob job)
		{
			if (string.Compare(job.Mime, "devexpress/report", true) == 0)
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

		private static HubConnection Connection { get; set; }

		private static void Complete(Guid id)
		{
			if (Connection.State != HubConnectionState.Connected)
				return;

			Connection.SendAsync("Complete", id).Wait();
		}

		private static void Ping(Guid id)
		{
			if (Connection.State != HubConnectionState.Connected)
				return;

			Connection.SendAsync("Ping", id).Wait();
		}

		private static void AddPrinters(List<string> printers)
		{
			var args = new List<object>();

			foreach (var printer in printers)
			{
				args.Add(new
				{
					Name = printer
				});
			}

			Connection.SendAsync("Add", args).Wait();
		}

		private static SpoolerJob SelectJob(Guid id)
		{
			var client = new HttpClient();

			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			var result = client.PostAsync($"{Cdn}/sys/printing-spooler", new StringContent(JsonConvert.SerializeObject(new
			{
				Id = id
			}), Encoding.UTF8, "application/json")).Result;

			var content = result.Content.ReadAsStringAsync().Result;

			if (string.Compare(content, "null", true) == 0
				|| string.IsNullOrWhiteSpace(content))
				return null;

			return JsonConvert.DeserializeObject<SpoolerJob>(content);
		}
	}

	internal class SpoolerJob
	{
		public Guid Token { get; set; }

		public string Mime { get; set; }

		public string Content { get; set; }

		public string Printer { get; set; }
	}
}
