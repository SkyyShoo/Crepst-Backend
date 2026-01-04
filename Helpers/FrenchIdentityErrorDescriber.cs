using Microsoft.AspNetCore.Identity;

namespace Backend.Helpers
{
    public class FrenchIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError()
            => new IdentityError { Code = nameof(DefaultError), Description = "Une erreur inconnue s'est produite." };

        public override IdentityError ConcurrencyFailure()
            => new IdentityError { Code = nameof(ConcurrencyFailure), Description = "Échec de l'accès concurrentiel, l'objet a été modifié." };

        public override IdentityError PasswordMismatch()
            => new IdentityError { Code = nameof(PasswordMismatch), Description = "Mot de passe incorrect." };

        public override IdentityError InvalidToken()
            => new IdentityError { Code = nameof(InvalidToken), Description = "Jeton invalide." };

        public override IdentityError LoginAlreadyAssociated()
            => new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = "Un utilisateur avec cette connexion existe déjà." };

        public override IdentityError InvalidUserName(string userName)
            => new IdentityError { Code = nameof(InvalidUserName), Description = $"Le nom d'utilisateur '{userName}' est invalide, seuls les lettres et les chiffres sont autorisés." };

        public override IdentityError InvalidEmail(string email)
            => new IdentityError { Code = nameof(InvalidEmail), Description = $"L'email '{email}' est invalide." };

        public override IdentityError DuplicateUserName(string userName)
            => new IdentityError { Code = nameof(DuplicateUserName), Description = $"Le nom d'utilisateur '{userName}' est déjà utilisé." };

        public override IdentityError DuplicateEmail(string email)
            => new IdentityError { Code = nameof(DuplicateEmail), Description = $"L'email '{email}' est déjà utilisé." };

        public override IdentityError InvalidRoleName(string role)
            => new IdentityError { Code = nameof(InvalidRoleName), Description = $"Le nom de rôle '{role}' est invalide." };

        public override IdentityError DuplicateRoleName(string role)
            => new IdentityError { Code = nameof(DuplicateRoleName), Description = $"Le nom de rôle '{role}' est déjà utilisé." };

        public override IdentityError UserAlreadyHasPassword()
            => new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = "L'utilisateur a déjà un mot de passe défini." };

        public override IdentityError UserLockoutNotEnabled()
            => new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = "Le verrouillage n'est pas activé pour cet utilisateur." };

        public override IdentityError UserAlreadyInRole(string role)
            => new IdentityError { Code = nameof(UserAlreadyInRole), Description = $"L'utilisateur a déjà le rôle '{role}'." };

        public override IdentityError UserNotInRole(string role)
            => new IdentityError { Code = nameof(UserNotInRole), Description = $"L'utilisateur n'a pas le rôle '{role}'." };

        public override IdentityError PasswordTooShort(int length)
            => new IdentityError { Code = nameof(PasswordTooShort), Description = $"Le mot de passe doit contenir au moins {length} caractères." };

        public override IdentityError PasswordRequiresNonAlphanumeric()
            => new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Le mot de passe doit contenir au moins un caractère spécial." };

        public override IdentityError PasswordRequiresDigit()
            => new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "Le mot de passe doit contenir au moins un chiffre ('0'-'9')." };

        public override IdentityError PasswordRequiresLower()
            => new IdentityError { Code = nameof(PasswordRequiresLower), Description = "Le mot de passe doit contenir au moins une minuscule ('a'-'z')." };

        public override IdentityError PasswordRequiresUpper()
            => new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "Le mot de passe doit contenir au moins une majuscule ('A'-'Z')." };

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
            => new IdentityError { Code = nameof(PasswordRequiresUniqueChars), Description = $"Le mot de passe doit contenir au moins {uniqueChars} caractères uniques." };

        public override IdentityError RecoveryCodeRedemptionFailed()
            => new IdentityError { Code = nameof(RecoveryCodeRedemptionFailed), Description = "Échec de l'utilisation du code de récupération." };
    }
}