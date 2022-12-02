using PterodactylPavlovServerController.Models;
using RestSharp;
using System.Text.Json;

namespace PterodactylPavlovServerController.Services;

public class CountryService
{
    private Dictionary<string, CountryModel> cache = new();

    public async Task<CountryModel> GetCountry(string code)
    {
        if (!cache.ContainsKey(code.ToLower()))
        {
            RestClient restClient = new RestClient("https://restcountries.com/v3.1/");
            RestResponse response = await restClient.GetAsync(new RestRequest($"alpha/{code.ToLower()}"), new CancellationTokenSource(500).Token);
            if (!response.IsSuccessStatusCode || response.Content == null)
            {
                throw new Exception($"Could not query countries API for country code \"{code}\"");
            }

            JsonElement countryData = JsonDocument.Parse(response.Content).RootElement.EnumerateArray().First();
            cache.Add(code.ToLower(), new CountryModel(countryData.GetProperty("cca2").GetString()!, countryData.GetProperty("name").GetProperty("common").GetString()!, countryData.GetProperty("flags").GetProperty("png").GetString()!));
        }

        return cache[code.ToLower()];
    }
}
