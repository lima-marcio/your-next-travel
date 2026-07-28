namespace YourNextTravel.Api.Infrastructure.Destinations;

/// <summary>
/// Open-Meteo's geocoding API resolves a place name to country/lat/long but not to a
/// currency. None of the other agreed MVP sources cover currency-by-country either, so
/// this is a small curated lookup for common destinations, falling back to USD for
/// countries not listed — a documented simplification, not a silent gap.
/// </summary>
public static class CountryCurrencyLookup
{
    private static readonly Dictionary<string, string> CurrencyByIsoCode2 = new(StringComparer.OrdinalIgnoreCase)
    {
        ["IT"] = "EUR",
        ["FR"] = "EUR",
        ["DE"] = "EUR",
        ["ES"] = "EUR",
        ["PT"] = "EUR",
        ["NL"] = "EUR",
        ["GR"] = "EUR",
        ["AT"] = "EUR",
        ["BE"] = "EUR",
        ["IE"] = "EUR",
        ["GB"] = "GBP",
        ["US"] = "USD",
        ["CA"] = "CAD",
        ["BR"] = "BRL",
        ["JP"] = "JPY",
        ["CN"] = "CNY",
        ["AU"] = "AUD",
        ["CH"] = "CHF",
        ["MX"] = "MXN",
        ["AR"] = "ARS",
        ["CL"] = "CLP",
        ["ZA"] = "ZAR",
        ["AE"] = "AED",
        ["TH"] = "THB",
        ["IN"] = "INR",
        ["KR"] = "KRW",
    };

    public static string Resolve(string isoCode2)
    {
        return CurrencyByIsoCode2.GetValueOrDefault(isoCode2, "USD");
    }
}
