using Shouldly;
using Xunit;

namespace EfCore.InMemory.Transactions.Tests;

/// <summary>
/// Tests for <see cref="NoOpDbContextTransaction"/>.
/// </summary>
public class NoOpDbContextTransactionTests
{
	[Fact]
	public void TransactionId_ShouldReturnUniqueGuid()
	{
		// Arrange & Act
		var transaction1 = new NoOpDbContextTransaction();
		var transaction2 = new NoOpDbContextTransaction();

		// Assert
		transaction1.TransactionId.ShouldNotBe(Guid.Empty);
		transaction2.TransactionId.ShouldNotBe(Guid.Empty);
		transaction1.TransactionId.ShouldNotBe(transaction2.TransactionId);
	}

	[Fact]
	public void Commit_ShouldNotThrow()
	{
		// Arrange
		var transaction = new NoOpDbContextTransaction();

		// Act & Assert
		Should.NotThrow(() => transaction.Commit());
	}

	[Fact]
	public async Task CommitAsync_ShouldNotThrow()
	{
		// Arrange
		var transaction = new NoOpDbContextTransaction();

		// Act & Assert
		await Should.NotThrowAsync(() => transaction.CommitAsync());
	}

	[Fact]
	public void Rollback_ShouldNotThrow()
	{
		// Arrange
		var transaction = new NoOpDbContextTransaction();

		// Act & Assert
		Should.NotThrow(() => transaction.Rollback());
	}

	[Fact]
	public async Task RollbackAsync_ShouldNotThrow()
	{
		// Arrange
		var transaction = new NoOpDbContextTransaction();

		// Act & Assert
		await Should.NotThrowAsync(() => transaction.RollbackAsync());
	}

	[Fact]
	public void Dispose_ShouldNotThrow()
	{
		// Arrange
		var transaction = new NoOpDbContextTransaction();

		// Act & Assert
		Should.NotThrow(() => transaction.Dispose());
	}

	[Fact]
	public async Task DisposeAsync_ShouldNotThrow()
	{
		// Arrange
		var transaction = new NoOpDbContextTransaction();

		// Act & Assert
		await Should.NotThrowAsync(async () => await transaction.DisposeAsync());
	}

	[Fact]
	public void CreateSavepoint_ShouldNotThrow()
	{
		// Arrange
		var transaction = new NoOpDbContextTransaction();

		// Act & Assert
		Should.NotThrow(() => transaction.CreateSavepoint("test"));
	}

	[Fact]
	public async Task CreateSavepointAsync_ShouldNotThrow()
	{
		// Arrange
		var transaction = new NoOpDbContextTransaction();

		// Act & Assert
		await Should.NotThrowAsync(() => transaction.CreateSavepointAsync("test"));
	}

	[Fact]
	public void RollbackToSavepoint_ShouldNotThrow()
	{
		// Arrange
		var transaction = new NoOpDbContextTransaction();

		// Act & Assert
		Should.NotThrow(() => transaction.RollbackToSavepoint("test"));
	}

	[Fact]
	public async Task RollbackToSavepointAsync_ShouldNotThrow()
	{
		// Arrange
		var transaction = new NoOpDbContextTransaction();

		// Act & Assert
		await Should.NotThrowAsync(() => transaction.RollbackToSavepointAsync("test"));
	}

	[Fact]
	public void ReleaseSavepoint_ShouldNotThrow()
	{
		// Arrange
		var transaction = new NoOpDbContextTransaction();

		// Act & Assert
		Should.NotThrow(() => transaction.ReleaseSavepoint("test"));
	}

	[Fact]
	public async Task ReleaseSavepointAsync_ShouldNotThrow()
	{
		// Arrange
		var transaction = new NoOpDbContextTransaction();

		// Act & Assert
		await Should.NotThrowAsync(() => transaction.ReleaseSavepointAsync("test"));
	}

	[Fact]
	public void SupportsSavepoints_ShouldReturnFalse()
	{
		// Arrange
		var transaction = new NoOpDbContextTransaction();

		// Act & Assert
		transaction.SupportsSavepoints.ShouldBeFalse();
	}

	[Fact]
	public void MultipleOperations_ShouldNotThrow()
	{
		// Arrange
		var transaction = new NoOpDbContextTransaction();

		// Act & Assert - simulate typical usage pattern
		Should.NotThrow(() =>
		{
			transaction.CreateSavepoint("sp1");
			transaction.RollbackToSavepoint("sp1");
			transaction.ReleaseSavepoint("sp1");
			transaction.Commit();
			transaction.Dispose();
		});
	}

	[Fact]
	public async Task AsyncMultipleOperations_ShouldNotThrow()
	{
		// Arrange
		await using var transaction = new NoOpDbContextTransaction();

		// Act & Assert - simulate typical async usage pattern
		await Should.NotThrowAsync(async () =>
		{
			await transaction.CreateSavepointAsync("sp1");
			await transaction.RollbackToSavepointAsync("sp1");
			await transaction.ReleaseSavepointAsync("sp1");
			await transaction.CommitAsync();
		});
	}
}
