using System;

namespace Shared.SC.Feature.Login.Pipelines.DoLogin
{
    [CLSCompliant(false)]
    public interface IDoLoginProcessor
    {
        void Process(DoLoginPipelineArgs args);
    }
}
