using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TeamAwake.Servers.RealmServer.Database.Records.Accounts;

namespace TeamAwake.Servers.RealmServer.Database.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly RealmDbContext _dbContext;
    
    public AccountRepository(RealmDbContext dbContext) =>
        _dbContext = dbContext;

    public async Task AddAccountAsync(AccountRecord account, CancellationToken cancellationToken = default)
    {
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAccountAsync(AccountRecord account, CancellationToken cancellationToken = default)
    {
        _dbContext.Accounts.Remove(account);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAccountAsync(AccountRecord account, CancellationToken cancellationToken = default)
    {
        _dbContext.Accounts.Update(account);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistAsync(Expression<Func<AccountRecord, bool>> predicate, CancellationToken cancellationToken = default) =>
        _dbContext.Accounts.AnyAsync(predicate, cancellationToken);

    public async Task<AccountRecord?> GetAccountAsync(Expression<Func<AccountRecord, bool>> predicate, CancellationToken cancellationToken = default) =>
        await _dbContext.Accounts.FirstOrDefaultAsync(predicate, cancellationToken);
}