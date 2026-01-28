# EfCore.InMemory.Transactions

[![NuGet](https://img.shields.io/nuget/v/EfCore.InMemory.Transactions.svg)](https://www.nuget.org/packages/EfCore.InMemory.Transactions/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Seamless transaction support for EF Core InMemory provider.** Eliminates "transactions with isolation level are not supported" errors in tests without changing production code.

## The Problem

EF Core's InMemory provider throws exceptions when using transactions with isolation levels:

```csharp
// This FAILS with InMemory provider:
await using var transaction = await _context.Database
    .BeginTransactionAsync(IsolationLevel.Serializable, ct);
// Exception: "Transactions with isolation level Serializable are not supported"
```

This forces you to either:
- ❌ Switch to SQLite in-memory (different provider, different behavior)
- ❌ Add `if (IsInMemory)` checks throughout your production code
- ❌ Restructure your code to not use transactions in tests

## The Solution

This package provides two approaches:

### Approach 1: Safe Extension Methods (Recommended for Direct DbContext Usage)

```csharp
// Instead of:
await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

// Use:
await context.Database.BeginTransactionSafeAsync(IsolationLevel.Serializable, ct);
// ✅ Works with both real database and InMemory!
```

### Approach 2: UnitOfWork Pattern (Recommended for Clean Architecture)

If you use the UnitOfWork pattern, integrate InMemory detection in your `UnitOfWork` class:

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly bool _isInMemoryDatabase;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        _isInMemoryDatabase = _context.Database.IsInMemoryDatabase(); // Extension method
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
    {
        if (_isInMemoryDatabase)
        {
            return Task.FromResult<IDbContextTransaction>(new NoOpDbContextTransaction());
        }
        return _context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }
}
```

## Installation

```bash
dotnet add package EfCore.InMemory.Transactions
```

Or via Package Manager:

```powershell
Install-Package EfCore.InMemory.Transactions
```

## Quick Start

### Step 1: Configure Test DbContext

```csharp
// In CustomWebApplicationFactory.cs or test setup:
services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase(Guid.NewGuid().ToString())
           .AddInMemoryTransactionSupport();  // Suppresses warnings
});
```

### Step 2: Use Safe Transaction Methods

```csharp
// In your code (works with both real DB and InMemory):
await using var transaction = await context.Database
    .BeginTransactionSafeAsync(IsolationLevel.Serializable, ct);

try
{
    // ... do work ...
    await context.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);
}
catch
{
    await transaction.RollbackAsync(ct);
    throw;
}
```

## API Reference

### Extension Methods

```csharp
// Check if using InMemory provider
bool isInMemory = context.Database.IsInMemoryDatabase();

// Safe transaction methods (work with any provider)
context.Database.BeginTransactionSafe();
context.Database.BeginTransactionSafe(IsolationLevel.Serializable);
await context.Database.BeginTransactionSafeAsync(ct);
await context.Database.BeginTransactionSafeAsync(IsolationLevel.Serializable, ct);

// Suppress transaction warnings in configuration
optionsBuilder.AddInMemoryTransactionSupport();
```

### NoOpDbContextTransaction

A no-op implementation of `IDbContextTransaction` for use in UnitOfWork patterns:

```csharp
// Use in your UnitOfWork when InMemory is detected:
if (_isInMemoryDatabase)
{
    return new NoOpDbContextTransaction();
}
```

## Full Example: ASP.NET Core Integration Tests

### CustomWebApplicationFactory.cs

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real database registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add InMemory database with transaction support
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(Guid.NewGuid().ToString())
                       .AddInMemoryTransactionSupport();
            });
        });
    }
}
```

### UnitOfWork.cs (if using UnitOfWork pattern)

```csharp
using EfCore.InMemory.Transactions;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly bool _isInMemoryDatabase;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        _isInMemoryDatabase = context.Database.IsInMemoryDatabase();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        if (_isInMemoryDatabase)
        {
            // Return no-op transaction for InMemory provider
            return new NoOpDbContextTransaction();
        }

        return await _context.Database.BeginTransactionAsync(
            isolationLevel, cancellationToken);
    }

    // ... rest of UnitOfWork implementation
}
```

## Compatibility

| EF Core Version | .NET Version | Supported |
|-----------------|--------------|-----------|
| 8.0             | .NET 8       | ✅        |
| 9.0             | .NET 9       | ✅        |
| 10.0            | .NET 10      | ✅        |

## Why Not Just Use SQLite?

SQLite in-memory is a valid alternative, but:

| Aspect | InMemory + This Package | SQLite In-Memory |
|--------|------------------------|------------------|
| Setup Complexity | Simple | More complex |
| Speed | Fastest | Fast |
| Provider Behavior | InMemory behavior | SQLite behavior |
| Real Transactions | No (no-op) | Yes |
| Foreign Keys | No | Optional |

**Use this package when**: You want fast tests and your transaction logic is already tested elsewhere.

**Use SQLite when**: You need real transaction semantics in tests.

## How It Works

1. **`IsInMemoryDatabase()`** - Checks if the provider name contains "InMemory"
2. **`BeginTransactionSafeAsync()`** - Returns `NoOpDbContextTransaction` for InMemory, real transaction otherwise
3. **`NoOpDbContextTransaction`** - Implements `IDbContextTransaction` with no-op Commit/Rollback/Dispose
4. **`AddInMemoryTransactionSupport()`** - Suppresses `TransactionIgnoredWarning`

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Inspired by the common pain point discussed in [EF Core Issue #2866](https://github.com/dotnet/efcore/issues/2866)
- Created to solve real-world testing challenges with EF Core InMemory provider
