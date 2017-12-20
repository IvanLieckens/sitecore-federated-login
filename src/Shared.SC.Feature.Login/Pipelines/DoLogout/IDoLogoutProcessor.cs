using System;

namespace Shared.SC.Feature.Login.Pipelines.DoLogout
{
    [CLSCompliant(false)]
    public interface IDoLogoutProcessor
    {
        void Process(DoLogoutPipelineArgs args);
    }
}
