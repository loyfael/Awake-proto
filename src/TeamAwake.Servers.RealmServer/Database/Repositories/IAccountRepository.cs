using System.Linq.Expressions;
using TeamAwake.Servers.RealmServer.Database.Records.Accounts;

namespace TeamAwake.Servers.RealmServer.Database.Repositories;

public interface IAccountRepository
{
    Task AddAccountAsync(AccountRecord account, CancellationToken cancellationToken = default);
    
    Task RemoveAccountAsync(AccountRecord account, CancellationToken cancellationToken = default);
    
    Task UpdateAccountAsync(AccountRecord account, CancellationToken cancellationToken = default);

    Task<bool> ExistAsync(Expression<Func<AccountRecord, bool>> predicate, CancellationToken cancellationToken = default);
    
    Task<AccountRecord?> GetAccountAsync(Expression<Func<AccountRecord, bool>> predicate, CancellationToken cancellationToken = default);
}