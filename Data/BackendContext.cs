using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Backend.Data
{
    public class BackendContext : IdentityDbContext<User>
    {
        public BackendContext (DbContextOptions<BackendContext> options)
            : base(options)
        {
        }

    }
}
