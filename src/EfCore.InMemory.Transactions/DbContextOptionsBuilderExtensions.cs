using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EfCore.InMemory.Transactions;

/// <summary>
/// Extension methods for <see cref="DbContextOptionsBuilder"/> to add InMemory transaction support.
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
	/// <summary>
	/// Configures the DbContext to suppress transaction-related warnings when using the InMemory provider.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This method configures EF Core to ignore the <see cref="InMemoryEventId.TransactionIgnoredWarning"/>
	/// warning that occurs when transaction APIs are called on InMemory databases.
	/// </para>
	/// <para>
	/// <strong>Important:</strong> This only suppresses warnings. To actually make transaction code work
	/// with InMemory, use the <c>BeginTransactionSafeAsync</c> methods from <see cref="DatabaseFacadeExtensions"/>
	/// or implement a UnitOfWork pattern that detects InMemory and returns no-op transactions.
	/// </para>
	/// </remarks>
	/// <param name="optionsBuilder">The <see cref="DbContextOptionsBuilder"/> to configure.</param>
	/// <returns>The same <see cref="DbContextOptionsBuilder"/> instance for chaining.</returns>
	/// <example>
	/// <code>
	/// // In your test project's CustomWebApplicationFactory:
	/// services.AddDbContext&lt;AppDbContext&gt;(options =>
	/// {
	///     options.UseInMemoryDatabase("TestDb")
	///            .AddInMemoryTransactionSupport();
	/// });
	///
	/// // Then use the Safe extension methods in your code:
	/// await using var transaction = await _context.Database
	///     .BeginTransactionSafeAsync(IsolationLevel.Serializable, ct);
	/// </code>
	/// </example>
	public static DbContextOptionsBuilder AddInMemoryTransactionSupport(
			this DbContextOptionsBuilder optionsBuilder)
	{
		ArgumentNullException.ThrowIfNull(optionsBuilder);

		// Suppress the transaction warning for InMemory provider
		optionsBuilder.ConfigureWarnings(warnings => warnings
				.Ignore(InMemoryEventId.TransactionIgnoredWarning));

		return optionsBuilder;
	}

	/// <summary>
	/// Configures the DbContext to suppress transaction-related warnings when using the InMemory provider.
	/// </summary>
	/// <typeparam name="TContext">The type of the DbContext.</typeparam>
	/// <param name="optionsBuilder">The <see cref="DbContextOptionsBuilder{TContext}"/> to configure.</param>
	/// <returns>The same <see cref="DbContextOptionsBuilder{TContext}"/> instance for chaining.</returns>
	/// <example>
	/// <code>
	/// services.AddDbContext&lt;AppDbContext&gt;(options =>
	/// {
	///     options.UseInMemoryDatabase("TestDb")
	///            .AddInMemoryTransactionSupport();
	/// });
	/// </code>
	/// </example>
	public static DbContextOptionsBuilder<TContext> AddInMemoryTransactionSupport<TContext>(
			this DbContextOptionsBuilder<TContext> optionsBuilder)
			where TContext : DbContext
	{
		ArgumentNullException.ThrowIfNull(optionsBuilder);

		// Suppress the transaction warning for InMemory provider
		((DbContextOptionsBuilder)optionsBuilder).ConfigureWarnings(warnings => warnings
				.Ignore(InMemoryEventId.TransactionIgnoredWarning));

		return optionsBuilder;
	}
}
