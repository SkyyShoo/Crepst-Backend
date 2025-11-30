using Backend.Models;
using Microsoft.AspNetCore.Identity;

namespace Backend.Data
{
    public class Seed
    {
        public Seed() { }

        public static IdentityUser[] SeedUsers()
        {
            PasswordHasher<IdentityUser> hasher = new();
            return [
                               new User()
            {
                Id = "00000000-0000-0000-0000-000000000001",
                UserName = "pablo",
                Email = "seed@example.invalid",
                // La comparaison d'identity se fait avec les versions normalisés
                NormalizedEmail = "PABLO@ADMIN.COM",
                NormalizedUserName = "PABLO",
                EmailConfirmed = true,
                // On encrypte le mot de passe
                PasswordHash = hasher.HashPassword(null, "REMOVED_DEFAULT_PASSWORD"),
                LockoutEnabled = true
            },


                                               new User()
            {
                Id = "00000000-0000-0000-0000-000000000002",
                UserName = "sam",
                Email = "seed@example.invalid",
                // La comparaison d'identity se fait avec les versions normalisés
                NormalizedEmail = "SAM@ADMIN.COM",
                NormalizedUserName = "SAM",
                EmailConfirmed = true,
                // On encrypte le mot de passe
                PasswordHash = hasher.HashPassword(null, "REMOVED_DEFAULT_PASSWORD"),
                LockoutEnabled = true
            }

                ];

        }

        public static IdentityRole[] SeedRoles()
        {
            IdentityRole adminRole = new()
            {
                Id = "1",
                Name = BackendContext.ADMIN_ROLE,
                NormalizedName = BackendContext.ADMIN_ROLE.ToUpper()
            };

            return [adminRole];
        }


        public static Event[] SeedEvents()
        {
            return
            [
                new Event
                {
                Id= 1,
                Titre= "Concert de Jazz",
                Date= new DateTime(2024, 7, 15, 20, 0, 0),
                Resumer= "Une soirée inoubliable avec les meilleurs musiciens de jazz.",
                Lieu= "Salle de Concert Paris",
                ExtraitId= new List<int> { 1 },
                CommentsId= new List<int> { 1 }
                }
            ];
        }

        public static Extrait[] SeedExtraits()
        {
            return
            [
                new Extrait
                {
                Id= 1,
                Titre= "Le Grand Gatsby",
                Auteur= "F. Scott Fitzgerald",
                MaisonEdition= "Scribner",
                NumPages= 180,
                FileName= "Test",
                MimeType= ".pdf"
                }
            ];
        }

        public static Comment[] SeedComments()
        {
            return
            [
                new Comment
                {
                Id= 1,
                Text= "Un extrait fascinant",
                Date= new DateTime(2024, 6, 1, 14, 25, 2),
                EventId= 1
                }
            ];
        }

    }
}
