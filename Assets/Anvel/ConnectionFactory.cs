using Thrift.Protocol;
using Thrift.Transport;
using AnvelApi;

namespace CAVS.Anvel
{

    public static class ConnectionFactory
    {

        public static AnvelControlService.Client CreateConnection(string ipAddress = "127.0.0.1", int port = 9094)
        {
            var transport = new TSocket(ipAddress, port);
            var client = new AnvelControlService.Client(new TBinaryProtocol(transport));
            transport.Open();
            transport.TcpClient.NoDelay = true;
            return client;
        }

    }

}