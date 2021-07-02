using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VolunteerComputing.ManagementServer.Client
{
    public class QueryParser
    {
        public enum RedirectValues
        {
            CreateProject,
            ViewProjects,
            AdminPanel
        }
        public static string RedirectFromKey = "redirectFrom";
        public static Dictionary<RedirectValues, string> RedirectDictionary = new()
        {
            [RedirectValues.CreateProject] = "CreateProject",
            [RedirectValues.ViewProjects] = "ViewProjects",
            [RedirectValues.AdminPanel] = "AdminPanel",
        };


        public static void AccessDenied(RedirectValues redirectValue, NavigationManager navigationManager)
        {
            navigationManager.NavigateTo($"/access-denied?{RedirectFromKey}={redirectValue}");
        }


        Dictionary<string, StringValues> ParsedQuery;
        public QueryParser(NavigationManager navigationManager)
        {
            ParsedQuery = ParseQuery(navigationManager);
        }

        Dictionary<string, StringValues> ParseQuery(NavigationManager navigationManager)
        {
            var query = navigationManager.ToAbsoluteUri(navigationManager.Uri).Query;
            var parsed = QueryHelpers.ParseQuery(query);
            return parsed;
        }

        public string this[string key]
        {
            get { return ParsedQuery.ContainsKey(key) ? (string)ParsedQuery[key] : null; }
        }

    }
}
