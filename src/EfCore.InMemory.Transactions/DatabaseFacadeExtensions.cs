using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using RelationalDatabaseFacadeExtensions = Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions;

namespace EfCore.InMemory.Transactions;

/// <summary>
/// Extension methods for <see cref="DatabaseFacade"/> to provide InMemory-safe transaction operations.
/// </summary>
/// <remarks>
/// <para>
/// These extension methods provide an alternative to using the interceptor approach.
/// They explicitly check if the InMemory provider is being used and return appropriate
/// no-op transactions when needed.
/// </para>
/// <para>
/// Use these methods if you prefer explicit control over transaction behavior rather than
/// the transparent interceptor approach.
/// </para>
/// </remarks>
public static class DatabaseFacadeExtensions
{
	/// <summary>
	/// Determines if the database provider is the InMemory provider.
	/// </summary>
	/// <param name="database">The <see cref="DatabaseFacade"/> to check.</param>
	/// <returns><c>true</c> if using InMemory provider; otherwise, <c>false</c>.</returns>
	/// <example>
	/// <code>
	/// if (context.Database.IsInMemoryDatabase())
	/// {
	///     // Skip transaction-dependent logic in tests
	/// }
	/// </code>
	/// </example>
	public static bool IsInMemoryDatabase(this DatabaseFacade database)
	{
		ArgumentNullException.ThrowIfNull(database);
		return database.ProviderName?.Contains("InMemory", StringComparison.OrdinalIgnoreCase) == true;
	}

	/// <summary>
	/// Begins a new transaction, returning a no-op transaction if using InMemory provider.
	/// </summary>
	/// <param name="database">The <see cref="DatabaseFacade"/> to use.</param>
	/// <returns>
	/// A <see cref="IDbContextTransaction"/> that is either a real transaction or a no-op
	/// transaction if using InMemory provider.
	/// </returns>
	/// <example>
	/// <code>
	/// // Works with both real database and InMemory:
	/// using var transaction = context.Database.BeginTransactionSafe();
	/// // ... do work ...
	/// transaction.Commit();
	/// </code>
	/// </example>
	public static IDbContextTransaction BeginTransactionSafe(this DatabaseFacade database)
	{
		ArgumentNullException.ThrowIfNull(database);

		if (database.IsInMemoryDatabase())
		{
			return new NoOpDbContextTransaction();
		}

		return database.BeginTransaction();
	}

	/// <summary>
	/// Begins a new transaction with the specified isolation level, returning a no-op transaction
	/// if using InMemory provider.
	/// </summary>
	/// <param name="database">The <see cref="DatabaseFacade"/> to use.</param>
	/// <param name="isolationLevel">The isolation level to use for the transaction.</param>
	/// <returns>
	/// A <see cref="IDbContextTransaction"/> that is either a real transaction or a no-op
	/// transaction if using InMemory provider.
	/// </returns>
	/// <example>
	/// <code>
	/// // Works with both real database and InMemory:
	/// using var transaction = context.Database.BeginTransactionSafe(IsolationLevel.Serializable);
	/// // ... do work ...
	/// transaction.Commit();
	/// </code>
	/// </example>
	public static IDbContextTransaction BeginTransactionSafe(
			this DatabaseFacade database,
			IsolationLevel isolationLevel)
	{
		ArgumentNullException.ThrowIfNull(database);

		if (database.IsInMemoryDatabase())
		{
			return new NoOpDbContextTransaction();
		}

		return database.BeginTransaction(isolationLevel);
	}

	/// <summary>
	/// Asynchronously begins a new transaction, returning a no-op transaction if using InMemory provider.
	/// </summary>
	/// <param name="database">The <see cref="DatabaseFacade"/> to use.</param>
	/// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
	/// <returns>
	/// A task containing a <see cref="IDbContextTransaction"/> that is either a real transaction
	/// or a no-op transaction if using InMemory provider.
	/// </returns>
	/// <example>
	/// <code>
	/// // Works with both real database and InMemory:
	/// await using var transaction = await context.Database.BeginTransactionSafeAsync(ct);
	/// // ... do work ...
	/// await transaction.CommitAsync(ct);
	/// </code>
	/// </example>
	public static async Task<IDbContextTransaction> BeginTransactionSafeAsync(
			this DatabaseFacade database,
			CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(database);

		if (database.IsInMemoryDatabase())
		{
			return new NoOpDbContextTransaction();
		}

		return await database.BeginTransactionAsync(cancellationToken);
	}

	/// <summary>
	/// Asynchronously begins a new transaction with the specified isolation level, returning a
	/// no-op transaction if using InMemory provider.
	/// </summary>
	/// <param name="database">The <see cref="DatabaseFacade"/> to use.</param>
	/// <param name="isolationLevel">The isolation level to use for the transaction.</param>
	/// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
	/// <returns>
	/// A task containing a <see cref="IDbContextTransaction"/> that is either a real transaction
	/// or a no-op transaction if using InMemory provider.
	/// </returns>
	/// <example>
	/// <code>
	/// // Works with both real database and InMemory - the most common use case!
	/// await using var transaction = await context.Database
	///     .BeginTransactionSafeAsync(IsolationLevel.Serializable, ct);
	/// try
	/// {
	///     // ... do work ...
	///     await transaction.CommitAsync(ct);
	/// }
	/// catch
	/// {
	///     await transaction.RollbackAsync(ct);
	///     throw;
	/// }
	/// </code>
	/// </example>
	public static async Task<IDbContextTransaction> BeginTransactionSafeAsync(
			this DatabaseFacade database,
			IsolationLevel isolationLevel,
			CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(database);

		if (database.IsInMemoryDatabase())
		{
			return new NoOpDbContextTransaction();
		}

		return await database.BeginTransactionAsync(isolationLevel, cancellationToken);
	}
}
