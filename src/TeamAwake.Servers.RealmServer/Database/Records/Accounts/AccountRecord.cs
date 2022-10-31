using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamAwake.Servers.RealmServer.Database.Records.Accounts;

[Table("accounts")]
public sealed class AccountRecord
{
    [Key]
    public int Id { get; init; }
    
    public required string Username { get; set; }
    
    public required string Password { get; set; }
    
    public string? Nickname { get; set; }
}