using YourNextTravel.Api.Domain.Destinations;

namespace YourNextTravel.Api.Infrastructure.Lodging;

public interface ILodgingPriceProvider
{
    /// <summary>
    /// Refreshes the typical-nightly-price snapshot for a city. No-ops (leaves the
    /// previous snapshot in place) if the source returns no usable data — sandbox
    /// coverage is limited, so this is expected for many cities until a production
    /// partner account is approved (see Domain.Pricing.LodgingPriceEstimate).
    /// </summary>
    Task RefreshPriceEstimateAsync(City city, CancellationToken cancellationToken);
}
