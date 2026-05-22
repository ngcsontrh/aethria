using Microsoft.AspNetCore.Identity;

namespace Aethria.Domain.Entities;

public class AppRole : IdentityRole<Guid>
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
