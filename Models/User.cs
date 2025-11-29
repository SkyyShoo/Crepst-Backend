using Microsoft.AspNetCore.Identity;
using NuGet.Protocol;

namespace Backend.Models
{
    public class User : IdentityUser
    {
        public virtual List<Livre>? Livres { get; set; } = null!;

        public virtual List<Event>? Events { get; set; } = null!;
    }
}
