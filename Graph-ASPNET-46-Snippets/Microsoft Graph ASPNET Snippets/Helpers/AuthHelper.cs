using System;
using System.Configuration;
using System.Globalization;

using Microsoft_Graph_ASPNET_Snippets.Helpers;

static internal class AuthHelper
{
    private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
    private static string authorityType = ConfigurationManager.AppSettings["ida:AuthorityType"];

    /// <summary>
    /// Get an authority string that makes sense given the app registration type.
    /// The `Authority` represents the v2.0 endpoint
    /// https://login.microsoftonline.com/[common|organizations|consumer]/v2.0
    /// </summary>
    public static string ConstructAuthority()
    {
        return String.Format(CultureInfo.InvariantCulture, aadInstance, authorityType, "/v2.0");
    }
}