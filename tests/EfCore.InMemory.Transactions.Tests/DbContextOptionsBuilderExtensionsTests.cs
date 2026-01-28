using System.Data;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace EfCore.InMemory.Transactions.Tests;

/// <summary>
/// Tests for <see cref="DbContextOptionsBuilderExtensions"/>.
/// </summary>
public class DbContextOptionsBuilderExtensionsTests
{
	[Fact]
	public void AddInMemoryTransactionSupport_WithNullBuilder_ThrowsArgumentNullException()
	{
		// Arrange
		DbContextOptionsBuilder? builder = null;

		// Act & Assert
		Should.Throw<ArgumentNullException>(() => builder!.AddInMemoryTransactionSupport());
	}

	[Fact]
	public void AddInMemoryTransactionSupport_ReturnsBuilder()
	{
		// Arrange
		var builder = new DbContextOptionsBuilder<TestDbContext>();

		// Act
		var result = builder.UseInMemoryDatabase(Guid.NewGuid().ToString())
												.AddInMemoryTransactionSupport();

		// Assert
		result.ShouldBe(builder);
	}

	[Fact]
	public void AddInMemoryTransactionSupport_GenericVersion_ReturnsBuilder()
	{
		// Arrange
		var builder = new DbContextOptionsBuilder<TestDbContext>();

		// Act
		var result = builder.UseInMemoryDatabase(Guid.NewGuid().ToString())
												.AddInMemoryTransactionSupport();

		// Assert
		result.ShouldBeOfType<DbContextOptionsBuilder<TestDbContext>>();
	}

	[Fact]
	public async Task WithTransactionSupport_UsingSafeMethod_DoesNotThrow()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<TestDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.AddInMemoryTransactionSupport()
				.Options;

		using var context = new TestDbContext(options);

		// Act & Assert - Using the Safe extension method
		await Should.NotThrowAsync(async () =>
		{
			await using var transaction = await context.Database
							.BeginTransactionSafeAsync(IsolationLevel.Serializable);
			await transaction.CommitAsync();
		});
	}

	[Fact]
	public async Task WithTransactionSupport_MultipleIsolationLevels_AllWork()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<TestDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.AddInMemoryTransactionSupport()
				.Options;

		using var context = new TestDbContext(options);

		// Act & Assert - Test various isolation levels
		var isolationLevels = new[]
		{
						IsolationLevel.ReadUncommitted,
						IsolationLevel.ReadCommitted,
						IsolationLevel.RepeatableRead,
						IsolationLevel.Serializable,
						IsolationLevel.Snapshot
				};

		foreach (var level in isolationLevels)
		{
			await Should.NotThrowAsync(async () =>
			{
				await using var transaction = await context.Database
									.BeginTransactionSafeAsync(level);
				await transaction.CommitAsync();
			}, $"Failed for isolation level: {level}");
		}
	}

	[Fact]
	public async Task WithTransactionSupport_FullWorkflow_WorksCorrectly()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<TestDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.AddInMemoryTransactionSupport()
				.Options;

		using var context = new TestDbContext(options);
		var entity = new TestEntity
		{
			Id = Guid.NewGuid(),
			Name = "Test Entity",
			CreatedAt = DateTime.UtcNow
		};

		// Act - Full workflow with transaction
		await using var transaction = await context.Database
				.BeginTransactionSafeAsync(IsolationLevel.Serializable);

		context.TestEntities.Add(entity);
		await context.SaveChangesAsync();
		await transaction.CommitAsync();

		// Assert
		var savedEntity = await context.TestEntities.FindAsync(entity.Id);
		savedEntity.ShouldNotBeNull();
		savedEntity.Name.ShouldBe("Test Entity");
	}

	[Fact]
	public async Task WithTransactionSupport_TryPattern_WorksCorrectly()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<TestDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.AddInMemoryTransactionSupport()
				.Options;

		using var context = new TestDbContext(options);
		var entity = new TestEntity
		{
			Id = Guid.NewGuid(),
			Name = "Test Entity",
			CreatedAt = DateTime.UtcNow
		};

		// Act - Try-catch pattern with transaction
		await using var transaction = await context.Database
				.BeginTransactionSafeAsync(IsolationLevel.Serializable);

		try
		{
			context.TestEntities.Add(entity);
			await context.SaveChangesAsync();
			await transaction.CommitAsync();
		}
		catch
		{
			await transaction.RollbackAsync();
			throw;
		}

		// Assert
		var savedEntity = await context.TestEntities.FindAsync(entity.Id);
		savedEntity.ShouldNotBeNull();
	}

	[Fact]
	public async Task WithTransactionSupport_Rollback_DoesNotThrow()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<TestDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.AddInMemoryTransactionSupport()
				.Options;

		using var context = new TestDbContext(options);

		// Act & Assert
		await using var transaction = await context.Database
				.BeginTransactionSafeAsync(IsolationLevel.Serializable);

		await Should.NotThrowAsync(() => transaction.RollbackAsync());
	}

	[Fact]
	public void WithTransactionSupport_SyncTransaction_WorksCorrectly()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<TestDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.AddInMemoryTransactionSupport()
				.Options;

		using var context = new TestDbContext(options);

		// Act & Assert - Sync transaction
		Should.NotThrow(() =>
		{
			using var transaction = context.Database.BeginTransactionSafe(IsolationLevel.Serializable);
			transaction.Commit();
		});
	}

	[Fact]
	public async Task WithTransactionSupport_MultipleTransactions_EachHasUniqueId()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<TestDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.AddInMemoryTransactionSupport()
				.Options;

		using var context = new TestDbContext(options);

		// Act
		await using var transaction1 = await context.Database
				.BeginTransactionSafeAsync(IsolationLevel.Serializable);
		var id1 = transaction1.TransactionId;
		await transaction1.CommitAsync();

		await using var transaction2 = await context.Database
				.BeginTransactionSafeAsync(IsolationLevel.Serializable);
		var id2 = transaction2.TransactionId;
		await transaction2.CommitAsync();

		// Assert
		id1.ShouldNotBe(Guid.Empty);
		id2.ShouldNotBe(Guid.Empty);
		id1.ShouldNotBe(id2);
	}
}
