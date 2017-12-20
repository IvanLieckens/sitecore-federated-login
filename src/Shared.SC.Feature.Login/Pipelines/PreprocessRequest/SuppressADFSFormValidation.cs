using System;
using System.Web;

using Sitecore.Pipelines.PreprocessRequest;

namespace Shared.SC.Feature.Login.Pipelines.PreprocessRequest
{
    [CLSCompliant(false)]
    public class SuppressAdfsFormValidation : PreprocessRequestProcessor
    {
        public override void Process(PreprocessRequestArgs args)
        {
            try
            {
                new SuppressFormValidation().Process(args);
            }
            catch (HttpRequestValidationException)
            {
                // NOTE [ILs] Only URLs with "login" are considered safe for special character input. (Required for WsFed)
                string rawUrl = args?.Context.Request.RawUrl ?? string.Empty;
                if (!rawUrl.Contains("login"))
                {
                    throw;
                }
            }
        }
    }
}