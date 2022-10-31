using Microsoft.EntityFrameworkCore;
using TeamAwake.Servers.RealmServer.Database.Records.Accounts;

#nullable disable

namespace TeamAwake.Servers.RealmServer.Database;

public sealed class RealmDbContext : DbContext
{
    public DbSet<AccountRecord> Accounts { get; set; }

    public RealmDbContext(DbContextOptions<RealmDbContext> options) : base(options) { }
}