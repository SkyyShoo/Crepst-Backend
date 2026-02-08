using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NuGet.Protocol;

namespace Backend.Models
{
    public class User : IdentityUser
    {
        [ValidateNever]
        public virtual List<Event>? Events { get; set; } = null!;
        public string? EmailConfirmationToken { get; set; }
        public DateTime? EmailConfirmationTokenExpiry { get; set; }
    }
}
