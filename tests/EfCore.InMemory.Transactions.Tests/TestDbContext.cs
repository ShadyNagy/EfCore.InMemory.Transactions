using Microsoft.EntityFrameworkCore;

namespace EfCore.InMemory.Transactions.Tests;

/// <summary>
/// A simple DbContext for testing purposes.
/// </summary>
public class TestDbContext : DbContext
{
	public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
	{
	}

	public DbSet<TestEntity> TestEntities => Set<TestEntity>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<TestEntity>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Name).HasMaxLength(100);
		});
	}
}

/// <summary>
/// A simple entity for testing purposes.
/// </summary>
public class TestEntity
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; }
}
