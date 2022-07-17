using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConsole.Models
{
    internal class CopygramDbContext : DbContext
    {
        public CopygramDbContext()
        {
        }

        public CopygramDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=178.151.124.250,21062; Initial Catalog=Copygram; ; User Id=sa; Password=cUarOm9If67yI6sFQBa6rOlJ; Trusted_Connection=false;");
            }
        }

        public virtual DbSet<Chat> Chats { get; set; } = null!;

        public virtual DbSet<ChatMember> ChatMembers { get; set; } = null!;

        public virtual DbSet<ChatMemberRole> ChatMemberRoles { get; set; } = null!;

        public virtual DbSet<ChatType> ChatTypes { get; set; } = null!;

        public virtual DbSet<Message> Messages { get; set; } = null!;

        public virtual DbSet<User> Users { get; set; } = null!;
    }
}
