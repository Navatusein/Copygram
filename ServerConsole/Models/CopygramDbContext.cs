using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConsole.Models
{
    public class CopygramDbContext : DbContext
    {
        public DbSet<Chat> Chats { get; set; } = null!;

        public DbSet<ChatMember> ChatMembers { get; set; } = null!;

        public DbSet<ChatMemberRole> ChatMemberRoles { get; set; } = null!;

        public DbSet<ChatMessage> ChatMessages { get; set; } = null!;

        public DbSet<ChatType> ChatTypes { get; set; } = null!;

        public DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLazyLoadingProxies()
                .UseSqlServer("Data Source=178.151.124.250,21062; Initial Catalog=Copygram; User Id=sa; Password=cUarOm9If67yI6sFQBa6rOlJ; Trusted_Connection=false;MultipleActiveResultSets=true;");
        }
    }
}
