using System;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Web;
using System.Web.Mvc;

using Shared.SC.Feature.Login.Pipelines.DoLogin;
using Shared.SC.Feature.Login.Pipelines.DoLogout;

using Sitecore;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Mvc.Controllers;
using Sitecore.Mvc.Extensions;
using Sitecore.Pipelines;

namespace Shared.SC.Feature.Login.Controllers
{
    [CLSCompliant(false)]
    public class LoginController : SitecoreController
    {
        private readonly HttpRequestBase _requestBase;

        public LoginController() : this(null)
        {
        }

        public LoginController(HttpRequestBase requestBase)
        {
            _requestBase = requestBase;
        }

        // NOTE [ILs] Request property is null during construction
        public new HttpRequestBase Request => _requestBase ?? base.Request;

        [Authorize]
        [SuppressMessage("SonarAnalyzer.CSharp", "S4040", Justification = "URLs are lowercase")]
        public ActionResult Login()
        {
            ActionResult result;
            try
            {
                DoLoginPipelineArgs pipelineArgs = new DoLoginPipelineArgs { HttpContext = HttpContext };

                if (!string.IsNullOrEmpty(Request.QueryString["returnurl"]))
                {
                    pipelineArgs.ReturnUrlQueryString = new Uri(Request.QueryString["returnurl"], UriKind.RelativeOrAbsolute);
                }

                CorePipeline.Run("doLogin", pipelineArgs);

                result = pipelineArgs.PostLoginAction;
            }
            catch (SecurityException ex)
            {
                Log.Info(ex.Message, this);

                result = new RedirectResult(
                    $"/{Context.Language.Name.ToLowerInvariant()}/{Settings.NoAccessUrl.WithoutPrefix('/')}",
                    false);
            }

            return result;
        }

        public ActionResult Logout()
        {
            DoLogoutPipelineArgs pipelineArgs = new DoLogoutPipelineArgs();
            pipelineArgs.Request = Request;
            CorePipeline.Run("doLogout", pipelineArgs);
            
            return pipelineArgs.PostLogoutAction;
        }
    }
}