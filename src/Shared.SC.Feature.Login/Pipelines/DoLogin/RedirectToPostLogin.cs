using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Mvc;

using Sitecore;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Links;
using Sitecore.Text;

namespace Shared.SC.Feature.Login.Pipelines.DoLogin
{
    [CLSCompliant(false)]
    public class RedirectToPostLogin : IDoLoginProcessor
    {
        private readonly Database _db;

        private readonly ISearchIndex _searchIndex;

        private readonly ID _siteRoot;

        [SuppressMessage("SonarAnalyzer.CSharp", "S4040", Justification = "Index is lowercase")]
        public RedirectToPostLogin()
            : this(
                Context.Database,
                Context.Database.GetItem(Context.Site.RootPath).ID,
                ContentSearchManager.GetIndex($"sitecore_{Context.Database.Name.ToLowerInvariant()}_index"))
        {
        }

        public RedirectToPostLogin(Database db, ID siteRoot, ISearchIndex searchIndex)
        {
            _db = db;
            _siteRoot = siteRoot;
            _searchIndex = searchIndex;
        }

        public void Process(DoLoginPipelineArgs args)
        {
            if (args != null && args.PostLoginAction == null)
            {
                args.PostLoginAction = new RedirectResult(GetRedirectUrl(args.ReturnUrlQueryString).ToString());
            }
        }

        private UrlString GetRedirectUrl(Uri returnUrl)
        {
            // NOTE [ILs] Don't redirect to absolute uris for redirectUrl after login, this may be a nefarious attempt to steal user information
            string result = returnUrl != null && returnUrl.IsAbsoluteUri ? string.Empty : returnUrl?.OriginalString;
            if (string.IsNullOrWhiteSpace(result))
            {
                SearchResultItem searchResult;
                using (IProviderSearchContext searchContext = _searchIndex.CreateSearchContext())
                {
                    searchResult =
                        searchContext.GetQueryable<SearchResultItem>()
                            .FirstOrDefault(
                                i => i.Paths.Contains(_siteRoot) && i.TemplateId == Templates.LoginSettings.Id);
                }

                Item loginSettingsItem = searchResult?.GetItem();
                if (loginSettingsItem != null)
                {
                    LinkField linkField = loginSettingsItem.Fields[Templates.LoginSettings.Fields.PostLoginRedirect];
                    Item targetItem = linkField.TargetID != (ID)null && linkField.TargetID != ID.Null
                                          ? _db.GetItem(linkField.TargetID)
                                          : null;

                    result = targetItem != null ? LinkManager.GetItemUrl(targetItem) : null;
                }
            }

            return new UrlString(result ?? "/");
        }
    }
}