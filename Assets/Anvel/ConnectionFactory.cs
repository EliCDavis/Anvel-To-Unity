using Thrift.Protocol;
using Thrift.Transport;
using AnvelApi;

namespace CAVS.Anvel
{

    public static class ConnectionFactory
    {

        public static AnvelControlService.Client CreateConnection(ClientConnectionToken connectionToken)
        {
            var transport = new TSocket(connectionToken.GetIpAddress(), connectionToken.GetPort());
            var client = new AnvelControlService.Client(new TBinaryProtocol(transport));
            transport.Open();
            transport.TcpClient.NoDelay = true;
            return client;
        }

    }

}