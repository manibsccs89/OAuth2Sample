using Microsoft.AspNetCore.Server.IISIntegration;
using System;

namespace AuthServer.Domain
{
    public class AccountOptions
    {
        public const bool AllowLocalLogin = true;

        public const bool AllowRememberLogin = true;

        public static TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(30);

        public static bool ShowLogoutPrompt = true;

        public static bool AutomaticRedirectAfterSignOut = false;

        // specify the Windows authentication scheme being used
        public static readonly string WindowsAuthenticationSchemeName = IISDefaults.AuthenticationScheme;

        // if user uses windows auth, should we load the groups from windows ?
        public static bool IncludeWindowsGroups = false;

        public const string InvalidCredentialsErrorMessage = "Invalid username or password";
    }
}