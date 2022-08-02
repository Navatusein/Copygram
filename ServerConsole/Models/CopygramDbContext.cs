using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
            var configuration = new ConfigurationBuilder().AddJsonFile("config.json").Build();

            optionsBuilder
                .UseLazyLoadingProxies()
                .UseSqlServer(configuration["ConnectionString"]);
        }
    }
}
