using Spectre.Console;
using Spectre.Console.Rendering;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace Client
{
    internal class Program
    {
        static NetTcpBinding tcpbinding = new(
     SecurityMode.None
     )
        {
            MaxBufferSize = int.MaxValue,
            MaxBufferPoolSize = int.MaxValue,
            MaxReceivedMessageSize = int.MaxValue,
            ReaderQuotas = new XmlDictionaryReaderQuotas
            {
                MaxArrayLength = int.MaxValue,
                MaxBytesPerRead = int.MaxValue,
                MaxDepth = int.MaxValue,
                MaxNameTableCharCount = int.MaxValue,
                MaxStringContentLength = int.MaxValue
            },
            SendTimeout = TimeSpan.FromSeconds(60),
            ReceiveTimeout = TimeSpan.MaxValue,
            TransferMode = TransferMode.Streamed,
            MaxConnections = 500
        };
        static ChannelFactory<IRepository>? channelFactory;

        static void Main(string[] args)
        {
            int port = AnsiConsole.Prompt(
                new TextPrompt<int>("Port?")
                .PromptStyle("green")
                .ValidationErrorMessage("[red]Not a valid port number{/}")
                .Validate(port =>
                {
                    return port switch
                    {
                        <= 0 => ValidationResult.Error("[red]Port must be greater than zero[/]"),
                        >= 65536 => ValidationResult.Error("[red]Port must be less than or equal to 65535[/]"),
                        _ => ValidationResult.Success()
                    };
                }));

            int iterationCount = AnsiConsole.Prompt(
                new TextPrompt<int>("Iteration Count?")
                .PromptStyle("green")
                .ValidationErrorMessage("[red]Not a valid iteration count[/]")
                .DefaultValue(20000)
                .ShowDefaultValue()
                .Validate(count =>
                {
                    return count switch
                    {
                        <= 0 => ValidationResult.Error("[red]Count must be greater than 0[/]"),
                        _ => ValidationResult.Success()
                    };
                }));

            IDictionary<long,long> pingDurationData = new SortedDictionary<long, long>();
            List<long> pingTimes = new List<long>();
            string endpointUrl = "net.tcp://localhost:" + port + "/myservice.svc";
            channelFactory = new(tcpbinding, endpointUrl);
            channelFactory.Open();
            var rawData = new List<Tuple<int, long>>();
            var tableTitle = new TableTitle("Client - " + endpointUrl);
            var table = new Table().LeftAligned().Width(tableTitle.Text.Length);
            table.Title = tableTitle;
            table.AddColumn("Ping duration");
            table.AddColumn("Count");
            AnsiConsole.Live(table)
                .AutoClear(false)
                .Start(ctx =>
                {
                    var sw = new Stopwatch();
                    for(int i = 0; i < iterationCount; i++)
                    {
                        IRepository? client = null;
                        try
                        {
                            sw.Restart();
                            client = GetClient();
                            long elapsedMilliseconds = sw.ElapsedMilliseconds;
                            sw.Restart();
                            client.Ping();
                            sw.Stop();
                            long pingDuration = sw.ElapsedMilliseconds;
                            int dataRow = -1;
                            if (pingDurationData.TryGetValue(pingDuration, out long duration))
                            {
                                pingDurationData[pingDuration] = duration + 1;
                                dataRow = pingTimes.IndexOf(pingDuration);
                            }
                            else
                            {
                                pingDurationData[pingDuration] = 1;
                            }
                            if (dataRow == -1) // Redo all table data
                            {
                                table.Rows.Clear();
                                foreach (var kv in pingDurationData)
                                {
                                    table.AddRow($"{kv.Key}ms", kv.Value.ToString());
                                }
                            }
                            else
                            {
                                table.UpdateCell(dataRow, 1, pingDurationData[pingDuration].ToString());
                            }
                            ctx.Refresh();
                        }
                        catch (Exception) { } // Swallow
                        Thread.Sleep(50);
                        (client as IDisposable)?.Dispose();
                    }
                });
        }

        private static IRepository GetClient()
        {
            return channelFactory.CreateChannel();
        }
    }

    [ServiceContract]
    internal interface IRepository
    {
        [OperationContract]
        void Ping();
    }
}
