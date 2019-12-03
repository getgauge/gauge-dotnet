using System.Threading.Tasks;
using Gauge.Messages;
using Grpc.Core;

namespace Gauge.Dotnet.Handlers
{
    internal class ProcessHandler : Process.ProcessBase
    {
        private Server _server;
        public ProcessHandler(Server server)
        {
            this._server = server;
        }

        public override Task<Empty> Kill(KillProcessRequest request, ServerCallContext context)
        {
            try
            {
                Logger.Debug("KillProcessrequest received");
                return Task.FromResult(new Empty());
            }
            finally
            {
                _server.ShutdownAsync();
            }
        }

    }
}