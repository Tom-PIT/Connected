/*
 * Copyright (c) 2020-2021 Tom PIT. All rights reserved.
 * Licensed under GNU Affero General Public License version 3.
 * Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license
 */

using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TomPIT.Connected.Printing.Client.Configuration;
using TomPIT.Connected.Printing.Client.Printing;

namespace TomPIT.Connected.Printing.Client.Handlers
{
    internal class PrintingHandler
    {
        private HubConnection _connection;

        private PrinterHandler _printerHandler = new PrinterHandler();

        private bool _keepAlive;

        private Uri _baseCdnUri;

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
            }).WithAutomaticReconnect(new ConnectionRetryPolicy()).Build();

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
                RegisterPrinters();
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
                    Logging.Debug($"Connection closed due to error: {error}");
                }

                if (!_keepAlive) 
                    return;

                await Task.Delay(1000);
                await _connection.StartAsync();
                RegisterPrinters();

                Logging.Debug("Connection re-established.");
            };

            _connection.On<Guid, Guid>(Constants.RequestPrint, (id, receipt) =>
            {
                Logging.Debug($"Print request {id}");

                try
                {
                    var job = SelectJob(id);

                    if (job != null)
                    {
                        Print(job);
                        Complete(receipt);
                    }
                    else
                    {
                        Logging.Error($"Print Job {id} not found.");
                    }
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex, LoggingLevel.Fatal);
                }
            });
        }

        public void Start()
        {
            Logging.Debug("Starting print spooler...");

            Settings.ResetSettings();

            _baseCdnUri = new Uri(Settings.CdnUrl);

            try
            {
                if (_connection is null)
                    CreateConnection();

                _connection.StartAsync().Wait();

                Logging.Debug("Print spooler started");

                RegisterPrinters();

                _keepAlive = true;
            }
            catch (Exception ex)
            {
                Logging.Exception(ex, LoggingLevel.Fatal);
            }
        }

        public void Stop()
        {
            Logging.Debug("Stopping print spooler...");

            try
            {
                _keepAlive = false;

                _connection.StopAsync().Wait();

                Logging.Debug("Print spooler stopped");
            }
            catch (Exception ex)
            {
                Logging.Exception(ex, LoggingLevel.Fatal);
            }
        }

        private void RegisterPrinters()
        {
            Logging.Debug("Registering printers...");

            var printerList = _printerHandler.GetPrinters(Settings.AvailablePrinters, Settings.PrinterNameMappings);

            try
            {
                AddPrinters(printerList);
                Logging.Trace($"Registered Printers: {string.Join(", ", printerList)}");
            }
            catch (Exception ex)
            {
                Logging.Exception(ex);
            }
        }

        private void Print(SpoolerJob job)
        {
            Logging.Trace($"Printing requested");

            try
            {
                if (job.Mime.Equals(Constants.MimeTypeReport, StringComparison.OrdinalIgnoreCase))
                {
                    Logging.Debug($"Printing Job {job}");

                    var report = new XtraReport();
                    using (var ms = new MemoryStream(Convert.FromBase64String(job.Content)))
                    {
                        report.LoadLayoutFromXml(ms);

                        var printerName = _printerHandler.MapToSystemName(job.Printer);
                        Logging.Debug($"Printing to '{printerName}' (mapped as '{job.Printer}')");

                        report.PrinterName = printerName;
                        report.CreateDocument(false);
                        report.PrintingSystem.Document.Name = $"TomPIT Printing Doc {job.Token}";

                        var print = new PrintToolBase(report.PrintingSystem);

                        Logging.Trace("Starting printing...");
                        print.Print(printerName);
                        Logging.Trace("Printing done...");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Exception(ex, LoggingLevel.Fatal);
                throw;
            }
        }

        private void Complete(Guid id)
        {
            Logging.Debug($"Completing Job {id}");

            try
            {
                if (_connection.State != HubConnectionState.Connected)
                    return;

                _connection.SendAsync(Constants.ServerMethodNameComplete, id).Wait();

                Logging.Trace($"Job {id} Completed");
            }
            catch (Exception ex)
            {
                Logging.Exception(ex, LoggingLevel.Fatal);
                throw;
            }
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
            Logging.Debug($"Getting Job {id} from Server");

            var spollerUri = new Uri(_baseCdnUri, "sys/printing-spooler");

            Logging.Trace($"HTTP Request to {spollerUri}");
            Logging.Trace($"Header: {Constants.HttpHeaderBearer} = {Settings.Token}");
            Logging.Trace($"Mime Type: {Constants.MimeTypeJson}");

            try
            {
                var client = new HttpClient();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constants.HttpHeaderBearer, Settings.Token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.MimeTypeJson));

                var result = client.PostAsync(spollerUri, new StringContent(JsonConvert.SerializeObject(new
                {
                    Id = id
                }), Encoding.UTF8, Constants.MimeTypeJson)).Result;

                var content = result.Content.ReadAsStringAsync().Result;

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
    }

}
