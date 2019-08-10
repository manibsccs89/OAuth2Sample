using System.Collections.Generic;
using System.Linq;
using AuthServer.Domain;

namespace AuthServer.ViewModels
{
    public class LoginViewModel : LoginInputModel
    {
        public bool AllowRememberLogin { get; set; } = true;

        public bool EnableLocalLogin { get; set; } = true;

        public bool IsExternalLoginOnly =>
            EnableLocalLogin == false && ExternalProviders?.Count() == 1;

        public string ExternalLoginScheme =>
            IsExternalLoginOnly ?
                ExternalProviders?.SingleOrDefault()?.AuthenticationScheme :
                null;

        public IEnumerable<ExternalProvider> ExternalProviders { get; set; } =
            Enumerable.Empty<ExternalProvider>();

        public IEnumerable<ExternalProvider> VisibleExternalProviders =>
            ExternalProviders.Where(x => !string.IsNullOrWhiteSpace(x.DisplayName));
    }
}