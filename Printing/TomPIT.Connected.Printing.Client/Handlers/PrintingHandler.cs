/*
 * Copyright (c) 2020-2021 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Expressions;
using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Connected.Printing.Client.Configuration;
using TomPIT.Connected.Printing.Client.Printing;
using Microsoft.Extensions.Caching.Memory;
using DevExpress.XtraReports.Services;
using System.ComponentModel.Design;

namespace TomPIT.Connected.Printing.Client.Handlers
{
	internal class PrintingHandler : BackgroundService
	{
		private readonly LocalizationProvider _localizationProvider;

		private readonly SemaphoreSlim _functionLock;

		private HubConnection _connection;

		private PrinterHandler _printerHandler;

		private IMemoryCache _memoryCache;

		private Uri _baseCdnUri;

		private static int counter = 0;

		public PrintingHandler(LocalizationProvider localizationProvider, IMemoryCache memoryCache)
		{
			_printerHandler = new PrinterHandler(Settings.PrinterNameMappings);
			_localizationProvider = localizationProvider;
			_memoryCache = memoryCache;
			_functionLock = new SemaphoreSlim(1, 1);
		}

		private void CreateConnection()
		{
			var printingUri = new Uri(_baseCdnUri, "printing");

			Logging.Debug($"Creating connection to {printingUri}");

			_connection = new HubConnectionBuilder().WithUrl(printingUri, (o) =>
			{
				o.AccessTokenProvider = () =>
					{
						return Task.FromResult(Settings.Token);
					};
			})
			.WithAutomaticReconnect(new ConnectionRetryPolicy())
			.Build();

			_connection.Reconnecting += async (error) =>
			{
				if (error is null)
				{
					Logging.Debug("Connection closed, no error. Reconnecting.");
				}
				else
				{
					Logging.Debug($"Connection closed due to error. Reconnecting. Error = {error}");
				}
			};

			_connection.Reconnected += async (arg) =>
			{
				Logging.Debug($"Reconnected, Connection Id = {arg}");
				await RegisterPrinters();
			};

			_connection.KeepAliveInterval = TimeSpan.FromSeconds(20);

			_connection.Closed += async (error) =>
			{
				if (error is null)
				{
					Logging.Debug("Connection closed normally.");
				}
				else
				{
					Logging.Debug($"Connection closed due to error: {error}. Connection will attempt to restart.");

					await Task.Delay(5000);
					await _connection.StartAsync();
					await RegisterPrinters();

					Logging.Debug("Connection re-established.");
				}
			};

			_connection.On<Guid, Guid>(Constants.RequestPrint, async (id, receipt) =>
			{
				Logging.Debug($"Print request {id}");

				bool exists = true;
				_memoryCache.GetOrCreate<Guid>(id, (item) =>
					{
						exists = false;
						item.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
						return id;
					});

				if (exists)
				{
					Logging.Debug($"Print request {id} was duplicate, ignoring.");
					return;
				}

				try
				{
					try
					{
						if (await SelectJob(id) is SpoolerJob job)
						{
							Logging.Debug($"Issuing request for printing request {id}.");
							await Print(job);
							Logging.Debug($"Completing job for request {id}.");
							await Complete(receipt);
							Logging.Debug($"Job for request {id} completed.");
						}
						else
						{
							Logging.Error($"Print Job {id} not found.");
						}
					}
					catch (Exception ex)
					{
						Logging.Exception(ex, LoggingLevel.Error);
					}
				}
				catch (Exception ex)
				{
					Logging.Exception(ex, LoggingLevel.Fatal);
				}
			});
		}

		public async Task Start()
		{
			Logging.Debug("Starting print spooler...");

			Settings.ResetSettings();

			_baseCdnUri = new Uri(Settings.CdnUrl);

			try
			{
				if (_connection is null)
					CreateConnection();

				Logging.Debug("Connection to server");

				await _connection.StartAsync();

				Logging.Debug("Print spooler started");

				await RegisterPrinters();
			}
			catch (Exception ex)
			{
				Logging.Exception(ex, LoggingLevel.Fatal);
			}
		}

		public async Task Stop()
		{
			Logging.Debug("Stopping print spooler...");

			try
			{
				await _connection.StopAsync();

				Logging.Debug("Print spooler stopped");
			}
			catch (Exception ex)
			{
				Logging.Exception(ex, LoggingLevel.Fatal);
			}
		}

		private async Task RegisterPrinters()
		{
			Logging.Debug("Registering printers...");

			var printerList = _printerHandler.GetPrinters();

			try
			{
				await AddPrinters(printerList);
				Logging.Trace($"Registered Printers: {string.Join(", ", printerList)}");
			}
			catch (Exception ex)
			{
				Logging.Exception(ex);
			}
		}

		private async Task Print(SpoolerJob job)
		{
			Logging.Trace($"Printing requested");

			try
			{
				if (job.Mime.Equals(Constants.MimeTypeReport, StringComparison.OrdinalIgnoreCase))
				{
					Logging.Debug($"Printing Job {job}");

					var report = new XtraReport();
					((IServiceContainer)report).RemoveService(typeof(IReportProvider));
					((IServiceContainer)report).RemoveService(typeof(IReportProviderAsync));

					using var ms = new MemoryStream(Convert.FromBase64String(job.Content));				
										
					var content = new StreamReader(ms).ReadToEnd();

					ms.Position = 0;
					
					((IServiceContainer)report).AddService(typeof(IReportProviderAsync), new ContentSubreportProvider(content));
									
					report.LoadLayoutFromXml(ms, true);

					var localizeFunction = new LocalizeFunction(_localizationProvider, job.Identity);

					var printerName = _printerHandler.MapToSystemName(job.Printer);

					Logging.Debug($"Printing to '{printerName}' (mapped as '{job.Printer}')");

					report.PrinterName = printerName;

					try
					{

						await _functionLock.WaitAsync();
						try
						{
							CustomFunctions.Register(localizeFunction);

							await report.CreateDocumentAsync();

							CustomFunctions.Unregister(localizeFunction.Name);
						}
						finally
						{
							_functionLock.Release();
						}

						report.PrintingSystem.Document.Name = $"TomPIT Printing Doc {job.Token}";

						var print = new PrintToolBase(report.PrintingSystem);

						Logging.Trace("Starting printing...");
						if (OperatingSystem.IsWindows())
						{
							print.PrinterSettings.Copies = (short)job.CopyCount;

							print.Print(printerName);
						}
						else
						{
							for (int i = 0; i < job.CopyCount; i++)
							{
								print.Print(printerName);
							}
						}

						Logging.Trace("Printing done...");
					}
					catch (Exception ex)
					{
						Logging.Error("There was and error printing the document.");
						Logging.Debug(ex.ToString());
						throw;
					}
				}
			}
			catch (Exception ex)
			{
				Logging.Exception(ex, LoggingLevel.Error);
				throw;
			}
		}

		private async Task Complete(Guid id)
		{
			Logging.Debug($"Completing Job {id}");

			try
			{
				if (_connection.State != HubConnectionState.Connected)
					return;

				await _connection.SendAsync(Constants.ServerMethodNameComplete, id);

				Logging.Trace($"Job {id} Completed");
			}
			catch (Exception ex)
			{
				Logging.Exception(ex, LoggingLevel.Fatal);
				throw;
			}
		}

		private async Task AddPrinters(List<string> printers)
		{
			var args = new List<object>();

			foreach (var printer in printers)
			{
				args.Add(new
				{
					Name = printer
				});
			}

			await _connection.SendAsync(Constants.ServerMethodNameAddPrinters, args);
		}

		private HttpClient _client;
		private object HttpClientLock = new object();
		private HttpClient Client
		{
			get
			{
				if (_client is null)
				{
					lock (HttpClientLock)
					{
						if (_client is null)
						{
							var client = new HttpClient();

							client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.HttpHeaderBearer, Settings.Token);
							client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.MimeTypeJson));

							_client = client;
						}
					}
				}

				return _client;
			}
		}

		private async Task<SpoolerJob> SelectJob(Guid id)
		{
			Logging.Debug($"Getting Job {id} from Server");

			var spoolerUrl = new Uri(_baseCdnUri, "sys/printing-spooler");

			Logging.Trace($"HTTP Request to {spoolerUrl}");
			Logging.Trace($"Header: {Constants.HttpHeaderBearer} = {Settings.Token}");
			Logging.Trace($"Mime Type: {Constants.MimeTypeJson}");

			try
			{
				var result = await Client.PostAsync(spoolerUrl, new StringContent(JsonConvert.SerializeObject(new
				{
					Id = id
				}), Encoding.UTF8, Constants.MimeTypeJson));

				var content = await result.Content.ReadAsStringAsync();

				if (string.Compare(content, "null", true) == 0
					 || string.IsNullOrWhiteSpace(content))
					return null;

				var spoolerJob = JsonConvert.DeserializeObject<SpoolerJob>(content);

				Logging.Trace($"Returned Info: {spoolerJob}");

				return spoolerJob;
			}
			catch (Exception ex)
			{
				Logging.Exception(ex, LoggingLevel.Fatal);
				throw;
			}
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await Start();

			var taskCompletionSource = new TaskCompletionSource<bool>();

			stoppingToken.Register(async s =>
			{
				((TaskCompletionSource<bool>)s).SetResult(true);
			}, taskCompletionSource);

			await taskCompletionSource.Task;
			await Stop();
		}
	}

}
