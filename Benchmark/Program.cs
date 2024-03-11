using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.ServiceModel;
using System.Xml;

namespace Benchmark
{

    public class ConnectionTest
    {
        private static readonly NetTcpBinding tcpbinding = new(
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
        private static readonly ChannelFactory<IRepository> channelFactory08 = new(tcpbinding, "net.tcp://localhost:12308/iintervenantrepository.svc");
        private static readonly ChannelFactory<IRepository> channelFactory09 = new(tcpbinding, "net.tcp://localhost:12309/iintervenantrepository.svc");

        //[Benchmark]
        //public int xPing12308()
        //{
        //    IRepository client = channelFactory08.CreateChannel();
        //    client.Ping();
        //    return 8;
        //}

        [Benchmark]
        public int Ping12309()
        {
            IRepository client = channelFactory09.CreateChannel();
            client.Ping();
            return 9;
        }
    }
    internal class Program
    {
        private static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }

    internal class Repository : IRepository
    {
        public void Ping() { Console.Write(".");}
    }

    [ServiceContract(Namespace = "http://easily")]
    internal interface IRepository
    {
        [OperationContract]
        void Ping();
    }
}
