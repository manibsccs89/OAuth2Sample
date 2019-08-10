using AuthServer.Features.Account.Commands;
using AuthServer.Features.Account.Queries;
using AuthServer.ViewModels;
using IdentityServer4.Models;
using IdentityServer4.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AuthServer.Features.Account
{
    public class AccountController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IMediator _mediator;

        public AccountController(IIdentityServerInteractionService interaction,
            IMediator mediator)
        {
            _interaction = interaction;
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            LoginViewModel vm = await _mediator.Send(new GetLoginViewModelQuery.Data(returnUrl));

            if (vm.IsExternalLoginOnly)
            {
                return RedirectToAction("Challenge", "External", new
                {
                    provider = vm.ExternalLoginScheme,
                    returnUrl
                });
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginUserCommand.Data model, string button)
        {
            AuthorizationRequest context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            // user clicked the "cancel" button
            if (button != "login")
            {
                if (context == null)
                    return Redirect("~/");

                // if the user cancels, send a result back into IdentityServer as if they
                // denied the consent (even if this client does not require consent).
                // this will send back an access denied OIDC error response to the client.
                await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                return Redirect(model.ReturnUrl);
            }

            // Login process
            await _mediator.Send(model);

            if (context != null || Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            if (string.IsNullOrEmpty(model.ReturnUrl))
                return Redirect("~/");

            // Something went wrong
            LoginViewModel vm = await _mediator.Send(new GetLoginViewModelQuery.Data(model.ReturnUrl));
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;

            return View(vm);
        }

        [HttpPost]
        [Route("api/[controller]")]
        public async Task<IActionResult> Register([FromBody]RegisterUserCommand.Data registerUserVm) =>
            Ok(await _mediator.Send(registerUserVm));
    }
}