using Microsoft.EntityFrameworkCore;
using YourNextTravel.Api.Domain.Destinations;
using YourNextTravel.Api.Domain.Events;
using YourNextTravel.Api.Domain.LegalHealth;
using YourNextTravel.Api.Infrastructure.Destinations;
using YourNextTravel.Api.Infrastructure.Persistence;

namespace YourNextTravel.Api.Features.Admin;

public class AdminService : IAdminService
{
    private readonly AppDbContext _dbContext;
    private readonly IDestinationResolver _destinationResolver;

    public AdminService(AppDbContext dbContext, IDestinationResolver destinationResolver)
    {
        _dbContext = dbContext;
        _destinationResolver = destinationResolver;
    }

    public async Task<IReadOnlyList<LegalHealthRequirementResponse>> ListLegalHealthRequirementsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.LegalHealthRequirements
            .Join(_dbContext.Countries, r => r.CountryId, c => c.Id, (r, c) => new LegalHealthRequirementResponse(
                r.Id, c.IsoCode2, r.VisaRequirementText, r.VaccinationRequirementText, r.OtherHealthNotes, r.SourceNote, r.LastReviewedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<LegalHealthRequirementResponse> UpsertLegalHealthRequirementAsync(
        UpsertLegalHealthRequirementRequest request, CancellationToken cancellationToken)
    {
        var isoCode2 = request.CountryIsoCode2.ToUpperInvariant();

        var country = await _dbContext.Countries.FirstOrDefaultAsync(c => c.IsoCode2 == isoCode2, cancellationToken);
        if (country is null)
        {
            country = Country.Create(isoCode2, request.CountryName, request.CurrencyCode);
            _dbContext.Countries.Add(country);
        }

        var requirement = await _dbContext.LegalHealthRequirements
            .FirstOrDefaultAsync(r => r.CountryId == country.Id, cancellationToken);

        if (requirement is null)
        {
            requirement = LegalHealthRequirement.Create(
                country.Id, request.VisaRequirementText, request.VaccinationRequirementText, request.OtherHealthNotes, request.SourceNote);
            _dbContext.LegalHealthRequirements.Add(requirement);
        }
        else
        {
            requirement.Update(request.VisaRequirementText, request.VaccinationRequirementText, request.OtherHealthNotes, request.SourceNote);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LegalHealthRequirementResponse(
            requirement.Id, isoCode2, requirement.VisaRequirementText, requirement.VaccinationRequirementText,
            requirement.OtherHealthNotes, requirement.SourceNote, requirement.LastReviewedAtUtc);
    }

    public async Task<IReadOnlyList<CuratedEventResponse>> ListCuratedEventsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.EventListings
            .Where(e => e.ProviderName == EventProviderNames.Curated)
            .Select(e => new CuratedEventResponse(
                e.Id, e.Category, e.Title, e.Description, e.VenueName, e.StartUtc, e.EndUtc, e.ExternalUrl))
            .ToListAsync(cancellationToken);
    }

    public async Task<CuratedEventResponse> CreateCuratedEventAsync(CreateCuratedEventRequest request, CancellationToken cancellationToken)
    {
        Guid? cityId = null;
        if (!string.IsNullOrWhiteSpace(request.CityName))
        {
            var city = await _destinationResolver.ResolveAsync(request.CityName, cancellationToken);
            cityId = city?.Id;
        }

        var eventListing = EventListing.Create(
            request.Category, request.Title, request.Description, cityId, request.VenueName,
            request.StartUtc, request.EndUtc, EventProviderNames.Curated, Guid.NewGuid().ToString(), request.ExternalUrl);

        _dbContext.EventListings.Add(eventListing);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CuratedEventResponse(
            eventListing.Id, eventListing.Category, eventListing.Title, eventListing.Description,
            eventListing.VenueName, eventListing.StartUtc, eventListing.EndUtc, eventListing.ExternalUrl);
    }

    public async Task DeleteCuratedEventAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var eventListing = await _dbContext.EventListings.FirstOrDefaultAsync(
            e => e.Id == eventId && e.ProviderName == EventProviderNames.Curated, cancellationToken);

        if (eventListing is null)
        {
            throw new KeyNotFoundException("Curated event not found.");
        }

        _dbContext.EventListings.Remove(eventListing);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
