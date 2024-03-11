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
        static ChannelFactory<IRepository> channelFactory;

        static void Main(string[] args)
        {
            Console.Write("Port?");
            var port = Console.ReadLine();

            Console.Title = "Client - port " + port;

            channelFactory = new(tcpbinding, "net.tcp://localhost:" + port + "/myservice.svc");

            var sw = new Stopwatch();

            while (true)
            {
                Console.Write($"Start {DateTime.Now:O} ");
            sw.Restart();

            IRepository client = GetClient();

            long elapsedMilliseconds = sw.ElapsedMilliseconds;
            Console.Write($"CreateChannel took {elapsedMilliseconds} ms - ");
            sw.Restart();

            client.Ping();

                sw.Stop();
                long total = elapsedMilliseconds + sw.ElapsedMilliseconds;
                Console.ForegroundColor = total switch
                {
                    < 5 => ConsoleColor.Gray,
                    >= 5 and < 50 => ConsoleColor.Yellow,
                    >= 50 => ConsoleColor.Red
                };

                Console.WriteLine($"Ping took {sw.ElapsedMilliseconds} ms - total {total} ms - End {DateTime.Now:O} ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Thread.Sleep(50);
                (client as IDisposable)?.Dispose();
            }
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
