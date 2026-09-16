
using TicketManagement.Application.Common;
using TicketManagement.Application.Features.Authentication.Common;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Features.Authentication.Register
{
    public class RegisterCommandHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuthSessionIssuer _authSessionIssuer;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IAuthSessionIssuer authSessionIssuer, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _authSessionIssuer = authSessionIssuer;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<RegisterResult>> HandleAsync(RegisterCommand command)
        {
            var existingUser = await _userRepository.GetByEmailAsync(command.Email);

            if(existingUser != null)
            {
                return Result<RegisterResult>.Failure(AuthErrors.EmailAlreadyExists);
            }

            var user = new User
            {
                FirstName = command.FirstName,
                LastName = command.LastName,
                Email = command.Email,
                PasswordHash = _passwordHasher.Hash(command.Password),
                Role =  UserRole.Employee,
            };

            await _userRepository.AddAsync(user);

            // This still has to commit here, before IssueAsync runs: user.Id is an
            // identity column, so it doesn't get a real value until EF Core actually
            // executes the INSERT - and IssueAsync needs that real Id both for the JWT's
            // claim and as the RefreshToken's UserId foreign key. Unit of Work makes the
            // commit point explicit and consistent (one interface, not "whichever
            // repository happens to be handy"), but it can't remove a genuine
            // read-your-own-write dependency within a single request. Previously this
            // exact two-commit shape existed too, just spread across two different
            // repositories' SaveChangesAsync() instead of one IUnitOfWork - so nothing
            // about atomicity changed here, only that both calls are now consistent.
            await _unitOfWork.SaveChangesAsync();

            var session = await _authSessionIssuer.IssueAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return Result<RegisterResult>.Success(new RegisterResult
            {
                UserId = user.Id,
                AccessToken = session.AccessToken,
                AccessTokenExpiresAt = session.AccessTokenExpiresAt,
                RefreshToken = session.RefreshToken,
                RefreshTokenExpiresAt = session.RefreshTokenExpiresAt
            });
        }
    }
}
