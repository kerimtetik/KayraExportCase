using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Auth.Domain.Entities;
using Auth.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Auth.Infrastructure.Persistence
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    }
}
