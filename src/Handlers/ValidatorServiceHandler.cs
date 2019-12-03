using System.Threading.Tasks;
using Gauge.Dotnet.Processors;
using Gauge.Messages;
using Grpc.Core;

namespace Gauge.Dotnet.Handlers
{
    internal class ValidatorServiceHandler: Validator.ValidatorBase
    {
        private IStaticLoader _loader;
        private StepValidationProcessor stepValidateRequestProcessor;

        public ValidatorServiceHandler(IStaticLoader loader)
        {
            _loader = loader;
            this.stepValidateRequestProcessor = new StepValidationProcessor(_loader.GetStepRegistry());
        }

        public override Task<StepValidateResponse> ValidateStep(StepValidateRequest request, ServerCallContext context)
        {
            return Task.FromResult(this.stepValidateRequestProcessor.Process(request));
        }
    }
}