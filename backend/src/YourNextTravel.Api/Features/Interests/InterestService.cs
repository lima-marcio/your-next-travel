using Microsoft.EntityFrameworkCore;
using YourNextTravel.Api.Domain.Users;
using YourNextTravel.Api.Infrastructure.Persistence;

namespace YourNextTravel.Api.Features.Interests;

public class InterestService : IInterestService
{
    private readonly AppDbContext _dbContext;

    public InterestService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<InterestResponse>> ListAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Interests
            .Where(i => i.UserId == userId)
            .Select(i => new InterestResponse(i.Id, i.Category, i.Detail))
            .ToListAsync(cancellationToken);
    }

    public async Task<InterestResponse> AddAsync(Guid userId, CreateInterestRequest request, CancellationToken cancellationToken)
    {
        var alreadyExists = await _dbContext.Interests.AnyAsync(
            i => i.UserId == userId && i.Category == request.Category && i.Detail == request.Detail,
            cancellationToken);

        if (alreadyExists)
        {
            throw new InvalidOperationException("This interest is already registered.");
        }

        var interest = Interest.Create(userId, request.Category, request.Detail);
        _dbContext.Interests.Add(interest);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new InterestResponse(interest.Id, interest.Category, interest.Detail);
    }

    public async Task RemoveAsync(Guid userId, Guid interestId, CancellationToken cancellationToken)
    {
        var interest = await _dbContext.Interests
            .SingleOrDefaultAsync(i => i.Id == interestId && i.UserId == userId, cancellationToken);

        if (interest is null)
        {
            throw new KeyNotFoundException("Interest not found.");
        }

        _dbContext.Interests.Remove(interest);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
