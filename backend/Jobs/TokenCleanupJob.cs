using backend.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace backend.Jobs;

public class TokenCleanupJob: IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TokenCleanupJob> _logger;

    public TokenCleanupJob(IServiceScopeFactory scopeFactory, ILogger<TokenCleanupJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("[{Time}] -- Sprawdzanie tokenow", DateTime.Now);

        var dateToRemove = DateTimeOffset.UtcNow.AddDays(-3);

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var tokensToRemove = await dbContext.RefreshTokens
            .Where(rt => rt.RevokedAt <= dateToRemove || rt.ExpiresAt <= dateToRemove)
            .ToListAsync();

        var tokensFound = tokensToRemove.Count;

        if (tokensFound == 0)
        {
            _logger.LogInformation("Brak tokenow do usuniecia");
            return;
        }

        dbContext.RefreshTokens.RemoveRange(tokensToRemove);
        await dbContext.SaveChangesAsync();

        _logger.LogInformation("Usunieto {count} tokenow", tokensFound);
    }
}