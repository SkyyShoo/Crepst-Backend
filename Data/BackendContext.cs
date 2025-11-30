using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Data
{
    public class BackendContext : IdentityDbContext<User>
    {
        public BackendContext (DbContextOptions<BackendContext> options)
            : base(options)
        {
           
        }

        public const string ADMIN_ROLE = "admin";

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().HasData(Seed.SeedUsers());
            builder.Entity<IdentityRole>().HasData(Seed.SeedRoles());
            builder.Entity<Event>().HasData(Seed.SeedEvents());
            builder.Entity<Extrait>().HasData(Seed.SeedExtraits());
        }

        public DbSet<Backend.Models.Extrait> Extraits { get; set; } = default!;

        public DbSet<Backend.Models.Event> Events { get; set; } = default!;
    }
}
