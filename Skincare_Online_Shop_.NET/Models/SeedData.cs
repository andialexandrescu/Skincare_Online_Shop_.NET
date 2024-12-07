using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Skincare_Online_Shop_.NET.Data;

namespace Skincare_Online_Shop_.NET.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService
            <DbContextOptions<ApplicationDbContext>>()))
            {
                // Verificam daca in baza de date exista cel putin un rol
                // insemnand ca a fost rulat codul
                // De aceea facem return pentru a nu insera rolurile inca o data
                // Acesta metoda trebuie sa se execute o singura data
                if (context.Roles.Any())
                {
                    return; // baza de date contine deja roluri
                }
                // CREAREA ROLURILOR IN BD
                // daca nu contine roluri, acestea se vor crea
                context.Roles.AddRange(
                new IdentityRole { Id = "9adab8e5-49e2-4a33-99d0-2758a8f28364", Name = "Admin", NormalizedName = "Admin".ToUpper() },
                new IdentityRole { Id = "dd55f142-b710-4585-9bb7-8f0b71a3f630", Name = "Partner", NormalizedName = "Partner".ToUpper() },
                new IdentityRole { Id = "c3285489-6ae5-4096-b624-84c1cc9914f4", Name = "User", NormalizedName = "User".ToUpper() }
                );
                // o noua instanta pe care o vom utiliza pentru crearea parolelor utilizatorilor
                // parolele sunt de tip hash
                var hasher = new PasswordHasher<ApplicationUser>();
                // CREAREA USERILOR IN BD
                // Se creeaza cate un user pentru fiecare rol
                context.Users.AddRange(
                new ApplicationUser
                {
                    Id = "4640c65c-1470-4de5-8c9f-7ab249b24a70", // primary key
                    UserName = "admin@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "ADMIN@TEST.COM",
                    Email = "admin@test.com",
                    NormalizedUserName = "ADMIN@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "Admin1!")
                },
                new ApplicationUser
                {
                    Id = "eb9255f5-b989-4c16-b263-c34b894b7740", // primary key
                    UserName = "partner@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "PARTNER@TEST.COM",
                    Email = "partner@test.com",
                    NormalizedUserName = "PARTNER@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "Partner1!")
                },
                new ApplicationUser
                {
                    Id = "6d5bbf59-1e8f-4ea6-aead-c1b11298f4c6", // primary key
                    UserName = "user@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "USER@TEST.COM",
                    Email = "user@test.com",
                    NormalizedUserName = "USER@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "User1!")
                }
                );
                // ASOCIEREA USER-ROLE
                context.UserRoles.AddRange(
                new IdentityUserRole<string>
                {
                    RoleId = "9adab8e5-49e2-4a33-99d0-2758a8f28364",
                    UserId = "4640c65c-1470-4de5-8c9f-7ab249b24a70"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "dd55f142-b710-4585-9bb7-8f0b71a3f630",
                    UserId = "eb9255f5-b989-4c16-b263-c34b894b7740"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "c3285489-6ae5-4096-b624-84c1cc9914f4",
                    UserId = "6d5bbf59-1e8f-4ea6-aead-c1b11298f4c6"
                }
                );
                context.SaveChanges();
            }
        }
    }

}
