using Microsoft.Owin;

namespace Shared.SC.Feature.Login.Extensions
{
    public static class OwinContextExtensions
    {
        public static bool MapDomain(this IOwinContext ctx, string hostname)
        {
            return ctx?.Request.Headers.Get("Host").Equals(hostname) ?? false;
        }

        public static bool MapFolder(this IOwinContext ctx, string folder)
        {
            return ctx?.Request.Uri.LocalPath.StartsWith(folder) ?? false;
        }
    }
}