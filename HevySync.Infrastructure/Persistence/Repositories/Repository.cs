using HevySync.Domain.Common;
using HevySync.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HevySync.Infrastructure.Persistence.Repositories;

public class Repository<TEntity, TKey>(HevySyncDbContext context) : IRepository<TEntity, TKey>
    where TEntity : AggregateRoot<TKey>
    where TKey : notnull
{
    protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default) =>
        await DbSet.FindAsync([id], cancellationToken);

    public virtual async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);

    public virtual async Task<List<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        await DbSet.Where(predicate).ToListAsync(cancellationToken);

    public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet.ToListAsync(cancellationToken);

    public virtual async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        await DbSet.AnyAsync(predicate, cancellationToken);

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        await DbSet.AddAsync(entity, cancellationToken);

    public virtual async Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default) =>
        await DbSet.AddRangeAsync(entities, cancellationToken);

    public virtual void Update(TEntity entity) =>
        DbSet.Update(entity);

    public virtual void UpdateRange(IEnumerable<TEntity> entities) =>
        DbSet.UpdateRange(entities);

    public virtual void Remove(TEntity entity) =>
        DbSet.Remove(entity);

    public virtual void RemoveRange(IEnumerable<TEntity> entities) =>
        DbSet.RemoveRange(entities);
}
