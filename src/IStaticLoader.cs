using Gauge.Dotnet.Models;

namespace Gauge.Dotnet
{
    public interface IStaticLoader
    {
        IStepRegistry GetStepRegistry();
        void ReloadSteps(string content, string file);
        void RemoveSteps(string file);
        void LoadStepsFromText(string content, string file);
    }
}