using Microsoft.AspNetCore.Identity;

namespace Aethria.Domain.Entities;

public class AppUser : IdentityUser<Guid>
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
