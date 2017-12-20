using System;

namespace Shared.SC.Feature.Login.Pipelines.AuthenticationCheck
{
    [CLSCompliant(false)]
    public interface IAuthenticationCheckProcessor
    {
        void Process(AuthenticationCheckPipelineArgs args);
    }
}
