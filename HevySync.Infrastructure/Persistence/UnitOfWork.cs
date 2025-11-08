using HevySync.Domain.Aggregates;
using HevySync.Domain.Repositories;
using HevySync.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace HevySync.Infrastructure.Persistence;

public class UnitOfWork(HevySyncDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;
    private IRepository<Workout, Guid>? _workouts;
    private IRepository<Activity, Guid>? _activities;

    public IRepository<Workout, Guid> Workouts => _workouts ??= new Repository<Workout, Guid>(context);
    public IRepository<Activity, Guid> Activities => _activities ??= new Repository<Activity, Guid>(context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        _transaction = await context.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No transaction has been started");

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        context.Dispose();
        GC.SuppressFinalize(this);
    }
}
