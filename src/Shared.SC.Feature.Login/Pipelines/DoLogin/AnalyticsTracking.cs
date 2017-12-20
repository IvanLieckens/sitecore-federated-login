using System;
using System.Diagnostics.CodeAnalysis;

using Shared.SC.Feature.Login.Models;

using Sitecore;
using Sitecore.Analytics;
using Sitecore.Analytics.Model.Entities;
using Sitecore.Analytics.Tracking;
using Sitecore.ContentSearch;

namespace Shared.SC.Feature.Login.Pipelines.DoLogin
{
    [CLSCompliant(false)]
    public class AnalyticsTracking : IDoLoginProcessor
    {
        private const string PreferredEmailKey = "Default";

        private readonly ITracker _currentTracker;

        [SuppressMessage("SonarAnalyzer.CSharp", "S4040", Justification = "Index is lowercase")]
        public AnalyticsTracking()
        {
            _currentTracker = Tracker.Current;
            ContentSearchManager.GetIndex($"sitecore_{Context.Database.Name.ToLowerInvariant()}_index");
        }

        public AnalyticsTracking(ITracker tracker)
        {
            _currentTracker = tracker;
        }

        public void Process(DoLoginPipelineArgs args)
        {
            if (args?.Principal?.Identity != null && _currentTracker?.Session != null && !string.IsNullOrWhiteSpace(args.PrincipalClaimsInformation?.Email))
            {
                Session session = _currentTracker.Session;
                session.Identify(args.PrincipalClaimsInformation.Email);

                SetPersonalInfo(session, args.PrincipalClaimsInformation);
                SetEmail(session, args.PrincipalClaimsInformation.Email);
            }
        }

        private static void SetPersonalInfo(Session session, IPrincipalClaimsInformation principalClaimsInformation)
        {
            IContactPersonalInfo personalInfo = session.Contact.GetFacet<IContactPersonalInfo>("Personal");
            personalInfo.FirstName = principalClaimsInformation.GivenName;
            personalInfo.Surname = principalClaimsInformation.Surname;

            // if neither the given name nor the surname are specified for the user and
            // the display name is then we fall back to that field
            if (string.IsNullOrEmpty(principalClaimsInformation.GivenName) &&
                string.IsNullOrEmpty(principalClaimsInformation.Surname) &&
                !string.IsNullOrEmpty(principalClaimsInformation.DisplayName))
            {
                personalInfo.Surname = principalClaimsInformation.DisplayName;
            }
        }

        private static void SetEmail(Session session, string email)
        {
            IContactEmailAddresses emailAddresses = session.Contact.GetFacet<IContactEmailAddresses>("Emails");
            emailAddresses.Preferred = PreferredEmailKey;
            if (emailAddresses.Entries.Contains(PreferredEmailKey))
            {
                emailAddresses.Entries[PreferredEmailKey].SmtpAddress = email;
            }
            else
            {
                emailAddresses.Entries.Create(PreferredEmailKey).SmtpAddress = email;
            }
        }
    }
}