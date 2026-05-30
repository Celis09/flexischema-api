using ContactsAPI.Application.Users.Dtos;
using ContactsAPI.Data;
using MediatR;

namespace ContactsAPI.Application.Users.Queries.GetUserById
{
    public class GetUserByIdHandler(ContactsDbContext context) : IRequestHandler<GetUserByIdQuery, UserDto?>
    {
        public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await context.Users
                .FindAsync(new object[] { request.UserId }, cancellationToken);

            if (user == null) return null;

            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Status = user.Status.ToString(),
                CreatedDate = user.CreatedDate
            };
        }
    }
}