using AuthServer.Domain;
using AuthServer.ViewModels;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AuthServer.Features.Account.Queries
{
    public class GetLoginViewModelQuery
    {
        public class Data : IRequest<LoginViewModel>
        {
            public Data(string returnUrl)
            {
                ReturnUrl = returnUrl;
            }

            public string ReturnUrl { get; }
        }

        public class GetLoginViewModelQueryHandler : IRequestHandler<Data, LoginViewModel>
        {
            private readonly IIdentityServerInteractionService _interaction;
            private readonly IAuthenticationSchemeProvider _schemeProvider;
            private readonly IClientStore _clientStore;

            public GetLoginViewModelQueryHandler(IIdentityServerInteractionService interaction,
                IAuthenticationSchemeProvider schemeProvider,
                IClientStore clientStore)
            {
                _interaction = interaction;
                _schemeProvider = schemeProvider;
                _clientStore = clientStore;
            }

            public async Task<LoginViewModel> Handle(Data request, CancellationToken cancellationToken)
            {
                AuthorizationRequest context = await _interaction.GetAuthorizationContextAsync(request.ReturnUrl);

                if (context?.IdP != null)
                {
                    bool local = context.IdP == IdentityServerConstants.LocalIdentityProvider;

                    // this is meant to short circuit the UI and only trigger the one external IdP
                    var vm = new LoginViewModel
                    {
                        EnableLocalLogin = local,
                        ReturnUrl = request.ReturnUrl,
                        Username = context.LoginHint
                    };

                    if (!local)
                    {
                        vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
                    }

                    return vm;
                }

                IEnumerable<AuthenticationScheme> schemes = await _schemeProvider.GetAllSchemesAsync();

                List<ExternalProvider> providers = schemes
                    .Where(x => x.DisplayName != null ||
                                x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                    .Select(x => new ExternalProvider
                    {
                        DisplayName = x.DisplayName,
                        AuthenticationScheme = x.Name
                    }).ToList();

                bool allowLocal = true;

                if (context?.ClientId != null)
                {
                    Client client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
                    if (client != null)
                    {
                        allowLocal = client.EnableLocalLogin;

                        if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                        {
                            providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                        }
                    }
                }

                return new LoginViewModel
                {
                    AllowRememberLogin = AccountOptions.AllowRememberLogin,
                    EnableLocalLogin = allowLocal,
                    ReturnUrl = request.ReturnUrl,
                    Username = context?.LoginHint,
                    ExternalProviders = providers.ToArray()
                };
            }
        }
    }
}