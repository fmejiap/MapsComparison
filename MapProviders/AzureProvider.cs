using Maps.Azure;
using Maps.Report;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Maps.Classes
{
    public class AzureProvider : IGeocodeProvider
    {
        string SubscriptionKey;
        string path = "AzureMapsProvider.txt";
        AzureMapsGeocodeMethodType MethodType;
        IReport ReportComparer;

        public AzureProvider(string key, AzureMapsGeocodeMethodType methodType, IReport reportComparer)
        {
            SubscriptionKey = key;
            MethodType = methodType;
            ReportComparer = reportComparer;
        }


        public async Task GeocodeAsync(SourceAddress source)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.GetAsync(BuildUrlRequest(source));

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();

                    AzureMapsGeocodeResult geocodeResult = JsonConvert.DeserializeObject<AzureMapsGeocodeResult>(content);
                    GeocodeResult<Result> bestResult = GetBestGeocodeResult(geocodeResult, source);

                    string freeformAddress = bestResult.Result == null ? "NULL" : bestResult.Result.Address.FreeformAddress;
                    string resultType = bestResult.Result == null ? "NULL" : bestResult.Result.Type;

                    ReportComparer.SaveAndDisplay(path, $"Azure Maps;{source.Id};{source.Address};{source.Locality};{source.PostalCode};{source.Country};{resultType};{freeformAddress};{bestResult.Status};{geocodeResult.Summary.NumResults}");

                }
                else
                {
                    //var error = await response.Content.ReadAsAsync<ErrorResponse>();
                    Console.WriteLine("Azure HTTP Error");
                    ReportComparer.SaveAndDisplay(path, $"Azure Maps;{source.Id};{source.Address};{source.Locality};{source.PostalCode};{source.Country};NULL;NULL;ZERO_RESULT;0");
                }
            }
        }

        private GeocodeResult<Result> GetBestGeocodeResult(AzureMapsGeocodeResult geocodeResult, SourceAddress source)
        {
            var result = new GeocodeResult<Result>();

            if (geocodeResult.Results.Count == 0)
            {
                result.Status = "ZERO_RESULT";
                return result;
            }

            var best = geocodeResult.Results[0]; //The best result
            /*types: 
                    *Point Address (PAD):Indicates a precise street address
                    * Points of Interest (POI):defines and locates residential, business and public postal addresses
                   
            */
            if (best.Type.Equals("Point Address") || best.Type.Equals("POI"))
                result.Status = best.Address.PostalCode.Equals(source.PostalCode) ? "OK" : "TO_CHECK";
            else
                result.Status = "TO_CHECK";

            result.Result = best;
            return result;

        }

        private string BuildUrlRequest(SourceAddress source, bool withExtendedPostalCodesFor = true)
        {
            const string extendedPostalCodesFor = "PAD,POI";
            var result = default(string);

            switch (MethodType)
            {
                case AzureMapsGeocodeMethodType.GeocodeSearchAddress:

                    if (withExtendedPostalCodesFor)
                        result = $"https://atlas.microsoft.com/search/address/json?api-version=1.0&countrySet=ES&subscription-key={SubscriptionKey}&typeahead=true&extendedPostalCodesFor={extendedPostalCodesFor}&query={source.FormattedAddress}";
                    else
                        result = $"https://atlas.microsoft.com/search/address/json?api-version=1.0&countrySet=ES&subscription-key={SubscriptionKey}&typeahead=true&query={source.FormattedAddress}";

                    break;
                case AzureMapsGeocodeMethodType.GeocodeSearchAddressStructured:
                    result = $"https://atlas.microsoft.com/search/address/structured/json?subscription-key={SubscriptionKey}&api-version=1.0&countryCode={source.Country}&streetName={source.Address}&municipality={source.Locality}&postalCode={source.PostalCode}";
                    break;
                case AzureMapsGeocodeMethodType.GeocodeSearchFuzzy:
                    result = $"https://atlas.microsoft.com/search/fuzzy/json?subscription-key={SubscriptionKey}&api-version=1.0&query={source.FormattedAddress}&openingHours=nextSevenDays";
                    break;
                default:
                    break;
            }

            return result;
        }

    }
}
