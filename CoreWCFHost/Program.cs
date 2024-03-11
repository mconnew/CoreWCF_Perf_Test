using System.Xml;

Console.Title = "Net70 - port 12309";

var builder = WebApplication.CreateBuilder();

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();
builder.WebHost.UseNetTcp(12309);

var app = builder.Build();

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


app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<Repository>();
    serviceBuilder.AddServiceEndpoint<Repository, IRepository>(tcpbinding, "net.tcp://localhost:12309/myservice.svc");
    var serviceMetadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
    serviceMetadataBehavior.HttpGetEnabled = true;
});

app.Run();

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
