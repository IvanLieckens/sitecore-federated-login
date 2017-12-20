using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Shared.SC.Feature.Login.Data;

namespace Shared.SC.Feature.Login.Agents
{
    public class AuthSessionCleanupAgent
    {
        [SuppressMessage("SonarAnalyzer.CSharp", "S2325", Justification = "Is invoked by Sitecore pipeline as instance method")]
        public void Run()
        {
            using (SqlAuthSessionStoreContext authSessionStore = new SqlAuthSessionStoreContext())
            {
                authSessionStore.Entries.RemoveRange(authSessionStore.Entries.Where(e => e.ValidUntil <= DateTime.UtcNow));
                authSessionStore.SaveChanges();
            }
        }
    }
}