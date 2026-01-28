using Microsoft.EntityFrameworkCore.Storage;

namespace EfCore.InMemory.Transactions;

/// <summary>
/// A no-operation implementation of <see cref="IDbContextTransaction"/> for use with EF Core's InMemory provider.
/// </summary>
/// <remarks>
/// <para>
/// The EF Core InMemory provider doesn't support transactions, especially with isolation levels like
/// <see cref="System.Data.IsolationLevel.Serializable"/>. This class provides a compatible implementation
/// that performs no actual transaction operations, allowing code that uses transactions to work
/// seamlessly in tests without modification.
/// </para>
/// <para>
/// This is automatically used when you configure your test DbContext with
/// <see cref="DbContextOptionsBuilderExtensions.AddInMemoryTransactionSupport"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // This is used internally by the interceptor, but you can also use it directly if needed:
/// IDbContextTransaction transaction = new NoOpDbContextTransaction();
///
/// // All operations are no-ops:
/// await transaction.CommitAsync();   // Does nothing
/// await transaction.RollbackAsync(); // Does nothing
/// transaction.Dispose();             // Does nothing
/// </code>
/// </example>
public sealed class NoOpDbContextTransaction : IDbContextTransaction
{
	/// <summary>
	/// Gets the unique identifier for this transaction instance.
	/// </summary>
	/// <remarks>
	/// Each <see cref="NoOpDbContextTransaction"/> instance gets a unique GUID,
	/// even though no actual transaction is created. This maintains API compatibility
	/// with real transactions.
	/// </remarks>
	public Guid TransactionId { get; } = Guid.NewGuid();

	/// <summary>
	/// Commits the transaction. This is a no-op for InMemory provider.
	/// </summary>
	public void Commit()
	{
		// No-op: InMemory provider doesn't support real transactions
	}

	/// <summary>
	/// Asynchronously commits the transaction. This is a no-op for InMemory provider.
	/// </summary>
	/// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
	/// <returns>A completed task.</returns>
	public Task CommitAsync(CancellationToken cancellationToken = default)
	{
		// No-op: InMemory provider doesn't support real transactions
		return Task.CompletedTask;
	}

	/// <summary>
	/// Rolls back the transaction. This is a no-op for InMemory provider.
	/// </summary>
	public void Rollback()
	{
		// No-op: InMemory provider doesn't support real transactions
	}

	/// <summary>
	/// Asynchronously rolls back the transaction. This is a no-op for InMemory provider.
	/// </summary>
	/// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
	/// <returns>A completed task.</returns>
	public Task RollbackAsync(CancellationToken cancellationToken = default)
	{
		// No-op: InMemory provider doesn't support real transactions
		return Task.CompletedTask;
	}

	/// <summary>
	/// Creates a savepoint within the transaction. This is a no-op for InMemory provider.
	/// </summary>
	/// <param name="name">The name of the savepoint.</param>
	public void CreateSavepoint(string name)
	{
		// No-op: InMemory provider doesn't support savepoints
	}

	/// <summary>
	/// Asynchronously creates a savepoint within the transaction. This is a no-op for InMemory provider.
	/// </summary>
	/// <param name="name">The name of the savepoint.</param>
	/// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
	/// <returns>A completed task.</returns>
	public Task CreateSavepointAsync(string name, CancellationToken cancellationToken = default)
	{
		// No-op: InMemory provider doesn't support savepoints
		return Task.CompletedTask;
	}

	/// <summary>
	/// Rolls back to a savepoint. This is a no-op for InMemory provider.
	/// </summary>
	/// <param name="name">The name of the savepoint.</param>
	public void RollbackToSavepoint(string name)
	{
		// No-op: InMemory provider doesn't support savepoints
	}

	/// <summary>
	/// Asynchronously rolls back to a savepoint. This is a no-op for InMemory provider.
	/// </summary>
	/// <param name="name">The name of the savepoint.</param>
	/// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
	/// <returns>A completed task.</returns>
	public Task RollbackToSavepointAsync(string name, CancellationToken cancellationToken = default)
	{
		// No-op: InMemory provider doesn't support savepoints
		return Task.CompletedTask;
	}

	/// <summary>
	/// Releases a savepoint. This is a no-op for InMemory provider.
	/// </summary>
	/// <param name="name">The name of the savepoint.</param>
	public void ReleaseSavepoint(string name)
	{
		// No-op: InMemory provider doesn't support savepoints
	}

	/// <summary>
	/// Asynchronously releases a savepoint. This is a no-op for InMemory provider.
	/// </summary>
	/// <param name="name">The name of the savepoint.</param>
	/// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
	/// <returns>A completed task.</returns>
	public Task ReleaseSavepointAsync(string name, CancellationToken cancellationToken = default)
	{
		// No-op: InMemory provider doesn't support savepoints
		return Task.CompletedTask;
	}

	/// <summary>
	/// Gets a value indicating whether savepoints are supported. Always returns false for InMemory.
	/// </summary>
	public bool SupportsSavepoints => false;

	/// <summary>
	/// Disposes the transaction. This is a no-op for InMemory provider.
	/// </summary>
	public void Dispose()
	{
		// No-op: Nothing to dispose
	}

	/// <summary>
	/// Asynchronously disposes the transaction. This is a no-op for InMemory provider.
	/// </summary>
	/// <returns>A completed value task.</returns>
	public ValueTask DisposeAsync()
	{
		// No-op: Nothing to dispose
		return ValueTask.CompletedTask;
	}
}
