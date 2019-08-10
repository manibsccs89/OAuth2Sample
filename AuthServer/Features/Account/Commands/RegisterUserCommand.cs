using AuthServer.Infrastructure.Constants;
using AuthServer.Infrastructure.Data.Identity;
using AuthServer.Infrastructure.Exceptions;
using AuthServer.ViewModels;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace AuthServer.Features.Account.Commands
{
    public class RegisterUserCommand : IRequest<RegisterResponseViewModel>
    {
        public class Data : IRequest<RegisterResponseViewModel>
        {
            public string Name { get; set; }

            public string Email { get; set; }

            public string Password { get; set; }
        }

        public class DataValidator : AbstractValidator<Data>
        {
            public DataValidator()
            {
                RuleFor(user => user.Name)
                    .NotEmpty()
                    .MaximumLength(50);

                RuleFor(user => user.Email)
                    .NotEmpty()
                    .EmailAddress()
                    .MaximumLength(50);

                RuleFor(user => user.Password)
                    .NotEmpty()
                    .MinimumLength(6)
                    .MaximumLength(100);
            }
        }

        public class RegisterUserCommandHandler : IRequestHandler<Data, RegisterResponseViewModel>
        {
            private readonly UserManager<AppUser> _userManager;

            public RegisterUserCommandHandler(UserManager<AppUser> userManager)
            {
                _userManager = userManager;
            }

            public async Task<RegisterResponseViewModel> Handle(Data request, CancellationToken cancellationToken)
            {
                var user = new AppUser
                {
                    UserName = request.Email,
                    Name = request.Name,
                    Email = request.Email
                };

                IdentityResult result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                    throw new RestException(HttpStatusCode.BadRequest, result.Errors);

                await _userManager.AddClaimAsync(user, new Claim("userName", user.UserName));
                await _userManager.AddClaimAsync(user, new Claim("name", user.Name));
                await _userManager.AddClaimAsync(user, new Claim("email", user.Email));
                await _userManager.AddClaimAsync(user, new Claim("role", Roles.Consumer));

                return new RegisterResponseViewModel(user);
            }
        }
    }
}