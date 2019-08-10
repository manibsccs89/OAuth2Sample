using AuthServer.Domain;
using AuthServer.Infrastructure.Data.Identity;
using AuthServer.Infrastructure.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AuthServer.Features.Account.Commands
{
    public class LoginUserCommand : IRequest<Unit>
    {
        public class Data : IRequest
        {
            public string Username { get; set; }

            public string Password { get; set; }

            public bool RememberLogin { get; set; }

            public string ReturnUrl { get; set; }
        }

        public class DataValidator : AbstractValidator<Data>
        {
            public DataValidator()
            {
                RuleFor(loginData => loginData.Username)
                    .NotEmpty();

                RuleFor(loginData => loginData.Password)
                    .NotEmpty();
            }
        }

        public class LoginUserCommandHandler : IRequestHandler<Data, Unit>
        {
            private readonly SignInManager<AppUser> _signInManager;
            private readonly UserManager<AppUser> _userManager;

            public LoginUserCommandHandler(SignInManager<AppUser> signInManager,
                UserManager<AppUser> userManager)
            {
                _signInManager = signInManager;
                _userManager = userManager;
            }

            public async Task<Unit> Handle(Data request, CancellationToken cancellationToken)
            {
                AppUser user = await _userManager.FindByNameAsync(request.Username);

                SignInResult signInResult = await _signInManager.PasswordSignInAsync(user,
                    request.Password,
                    request.RememberLogin,
                    false);

                if (!signInResult.Succeeded)
                    throw new RestException(HttpStatusCode.Unauthorized, AccountOptions.InvalidCredentialsErrorMessage);

                return Unit.Value;
            }
        }
    }
}