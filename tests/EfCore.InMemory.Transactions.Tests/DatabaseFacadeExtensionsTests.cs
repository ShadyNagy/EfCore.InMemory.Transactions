using System.Data;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace EfCore.InMemory.Transactions.Tests;

/// <summary>
/// Tests for <see cref="DatabaseFacadeExtensions"/>.
/// </summary>
public class DatabaseFacadeExtensionsTests : IDisposable
{
	private readonly TestDbContext _context;

	public DatabaseFacadeExtensionsTests()
	{
		var options = new DbContextOptionsBuilder<TestDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

		_context = new TestDbContext(options);
	}

	public void Dispose()
	{
		_context.Dispose();
	}

	[Fact]
	public void IsInMemoryDatabase_WithInMemoryProvider_ReturnsTrue()
	{
		// Act
		var result = _context.Database.IsInMemoryDatabase();

		// Assert
		result.ShouldBeTrue();
	}

	[Fact]
	public void BeginTransactionSafe_WithInMemoryProvider_ReturnsNoOpTransaction()
	{
		// Act
		var transaction = _context.Database.BeginTransactionSafe();

		// Assert
		transaction.ShouldNotBeNull();
		transaction.ShouldBeOfType<NoOpDbContextTransaction>();
	}

	[Fact]
	public void BeginTransactionSafe_WithIsolationLevel_ReturnsNoOpTransaction()
	{
		// Act
		var transaction = _context.Database.BeginTransactionSafe(IsolationLevel.Serializable);

		// Assert
		transaction.ShouldNotBeNull();
		transaction.ShouldBeOfType<NoOpDbContextTransaction>();
	}

	[Fact]
	public async Task BeginTransactionSafeAsync_WithInMemoryProvider_ReturnsNoOpTransaction()
	{
		// Act
		var transaction = await _context.Database.BeginTransactionSafeAsync();

		// Assert
		transaction.ShouldNotBeNull();
		transaction.ShouldBeOfType<NoOpDbContextTransaction>();
	}

	[Fact]
	public async Task BeginTransactionSafeAsync_WithIsolationLevel_ReturnsNoOpTransaction()
	{
		// Act
		var transaction = await _context.Database
				.BeginTransactionSafeAsync(IsolationLevel.Serializable);

		// Assert
		transaction.ShouldNotBeNull();
		transaction.ShouldBeOfType<NoOpDbContextTransaction>();
	}

	[Fact]
	public async Task BeginTransactionSafeAsync_WithCancellationToken_ReturnsNoOpTransaction()
	{
		// Arrange
		using var cts = new CancellationTokenSource();

		// Act
		var transaction = await _context.Database
				.BeginTransactionSafeAsync(IsolationLevel.ReadCommitted, cts.Token);

		// Assert
		transaction.ShouldNotBeNull();
		transaction.ShouldBeOfType<NoOpDbContextTransaction>();
	}

	[Fact]
	public async Task FullTransactionWorkflow_WithInMemoryProvider_WorksCorrectly()
	{
		// Arrange
		var entity = new TestEntity
		{
			Id = Guid.NewGuid(),
			Name = "Test Entity",
			CreatedAt = DateTime.UtcNow
		};

		// Act - Full transaction workflow
		await using var transaction = await _context.Database
				.BeginTransactionSafeAsync(IsolationLevel.Serializable);

		_context.TestEntities.Add(entity);
		await _context.SaveChangesAsync();

		await transaction.CommitAsync();

		// Assert
		var savedEntity = await _context.TestEntities.FindAsync(entity.Id);
		savedEntity.ShouldNotBeNull();
		savedEntity.Name.ShouldBe("Test Entity");
	}

	[Fact]
	public async Task RollbackWorkflow_WithInMemoryProvider_DoesNotThrow()
	{
		// Arrange
		var entity = new TestEntity
		{
			Id = Guid.NewGuid(),
			Name = "Test Entity",
			CreatedAt = DateTime.UtcNow
		};

		// Act - Rollback workflow (no-op but should not throw)
		await using var transaction = await _context.Database
				.BeginTransactionSafeAsync(IsolationLevel.Serializable);

		_context.TestEntities.Add(entity);
		await _context.SaveChangesAsync();

		// Rollback is a no-op in InMemory, but should not throw
		await Should.NotThrowAsync(() => transaction.RollbackAsync());
	}

	[Fact]
	public void BeginTransactionSafe_WithNullDatabase_ThrowsArgumentNullException()
	{
		// Arrange
		Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade? database = null;

		// Act & Assert
		Should.Throw<ArgumentNullException>(() => database!.BeginTransactionSafe());
	}

	[Fact]
	public void BeginTransactionSafe_WithIsolationLevelAndNullDatabase_ThrowsArgumentNullException()
	{
		// Arrange
		Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade? database = null;

		// Act & Assert
		Should.Throw<ArgumentNullException>(() =>
				database!.BeginTransactionSafe(IsolationLevel.Serializable));
	}

	[Fact]
	public async Task BeginTransactionSafeAsync_WithNullDatabase_ThrowsArgumentNullException()
	{
		// Arrange
		Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade? database = null;

		// Act & Assert
		await Should.ThrowAsync<ArgumentNullException>(async () =>
				await database!.BeginTransactionSafeAsync());
	}

	[Fact]
	public async Task BeginTransactionSafeAsync_WithIsolationLevelAndNullDatabase_ThrowsArgumentNullException()
	{
		// Arrange
		Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade? database = null;

		// Act & Assert
		await Should.ThrowAsync<ArgumentNullException>(async () =>
				await database!.BeginTransactionSafeAsync(IsolationLevel.Serializable));
	}

	[Fact]
	public void IsInMemoryDatabase_WithNullDatabase_ThrowsArgumentNullException()
	{
		// Arrange
		Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade? database = null;

		// Act & Assert
		Should.Throw<ArgumentNullException>(() => database!.IsInMemoryDatabase());
	}
}
