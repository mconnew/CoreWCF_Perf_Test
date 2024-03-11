using System.ServiceModel;
using System.Xml;

namespace Net48Host
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Netfx - port 12308";
            var tcpbinding = new NetTcpBinding(
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

            var host = new ServiceHost(typeof(Repository));
            host.AddServiceEndpoint(typeof(IRepository), tcpbinding, "net.tcp://localhost:12308/myservice.svc");
            Console.WriteLine("Opening");
            try
            {
                host.Open();
                Console.WriteLine("Opened");
            }
            catch (Exception exc)
            {
                Console.WriteLine($"Exception details :\r\n{exc.GetType().Name} - {exc.Message}\n{exc.StackTrace}");

                return;
            }
            Console.ReadLine();
        }
    }
    class Repository : IRepository
    {
        public void Ping() { Console.Write("."); }
    }

    [ServiceContract]
    internal interface IRepository
    {
        [OperationContract]
        void Ping();
    }
}
