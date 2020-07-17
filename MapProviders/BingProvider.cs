using Maps.Bing;
using Maps.Report;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Maps
{
    internal class BingProvider : IGeocodeProvider
    {
        string BingMapsAPIKey;
        string path = "BingMapsProvider.txt";
        BingMapsGeocodeMethodType MethodType;
        IReport ReportComparer;

        public BingProvider(string key, BingMapsGeocodeMethodType methodType, IReport reportComparer)
        {
            BingMapsAPIKey = key;
            MethodType = methodType;
            ReportComparer = reportComparer;
        }

        public async Task GeocodeAsync(SourceAddress source)
        {

            Uri geocodeRequest = new Uri(BuildUrlRequest(source));

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.GetAsync(geocodeRequest);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    BingMapsGeocodeResult responseBing = JsonConvert.DeserializeObject<BingMapsGeocodeResult>(content);

                    GeocodeResult<Resource> bestResult = GetBestGeocodeResult(responseBing, source);

                    string freeformAddress = bestResult.Result == null ? "NULL" : bestResult.Result.Address.FormattedAddress;
                    string resultType = bestResult.Result == null ? "NULL" : bestResult.Result.EntityType;
                    ReportComparer.SaveAndDisplay(path, $"Bing Maps;{source.Id};{source.Address};{source.Locality};{source.PostalCode};{source.Country};{resultType};{freeformAddress};{bestResult.Status};{responseBing.ResourceSets[0].EstimatedTotal}");

                }
                else
                {
                    //var error = await response.Content.ReadAsAsync<ErrorResponse>();
                    Console.WriteLine("Bing HTTP Error");
                    ReportComparer.SaveAndDisplay(path, $"Bing Maps;{source.Id};{source.Address};{source.Locality};{source.PostalCode};{source.Country};NULL;NULL;ZERO_RESULT;0");

                }
            }
        }

        private GeocodeResult<Resource> GetBestGeocodeResult(BingMapsGeocodeResult geocodeResult, SourceAddress source)
        {
            var result = new GeocodeResult<Resource>();


            if (geocodeResult.ResourceSets[0].Resources.Count == 0)
            {
                result.Status = "ZERO_RESULT";
                return result;
            }

            var best = geocodeResult.ResourceSets[0].Resources[0]; //The best result

            if (!best.Address.CountryRegion.Equals("Spain"))
            {
                result.Status = "ZERO_RESULT";
                return result;
            }

            /* EntityType:

              *  Neighborhood:A section of a populated place that is typically well-known, but often with indistinct boundaries.
              *  Roadblock: Like Street
              *  Address: indicates a precise street address
              * Ref:https://docs.microsoft.com/en-us/bingmaps/rest-services/locations/location-data
              * Ref:https://docs.microsoft.com/en-us/bingmaps/spatial-data-services/public-data-sources/poi-entity-types
            */

            if (best.EntityType.Equals("Address") || best.EntityType.Equals("Roadblock"))
            {
                if (best.MatchCodes[0].Equals("Good") && (best.Confidence.Equals("High") || best.Confidence.Equals("Medium")))
                    result.Status = best.Address.PostalCode.Equals(source.PostalCode) ? "OK" : "TO_CHECK";
                else
                    result.Status = "TO_CHECK";
            }
            else
                result.Status = "TO_CHECK";

            result.Result = best;
            return result;

        }

        private string BuildUrlRequest(SourceAddress source)
        {
            var result = default(string);
            switch (MethodType)
            {
                case BingMapsGeocodeMethodType.GeocodeFindLocationByQuery:
                    result = string.Format("http://dev.virtualearth.net/REST/v1/Locations/?q={0}&key={1}", source.FormattedAddress, BingMapsAPIKey);
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
