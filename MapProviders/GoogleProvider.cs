using Maps.Google;
using Maps.Report;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Maps
{
    public class GoogleProvider : IGeocodeProvider
    {
        private const string BaseGoogleApiUrl = "https://maps.googleapis.com/maps/api/geocode/json?";
        private const string AddressAndComponentsAllGeocodeUrl = BaseGoogleApiUrl + "address={0}&components=country:{3}|postal_code:{2}|locality:{1}"; //option: A + C = all
        private const string AddressAndComponentsWithPostalCodeGeocodeUrl = BaseGoogleApiUrl + "address={0},{1}&region={3}&components=postal_code:{2}"; //option: A + C = CP 
        private const string OnlyAddressGeocodeUrl = BaseGoogleApiUrl + "address={0}, {2}, {1}&region={3}"; //option: Only A

        string GoogleMapsAPIKey;
        string Path = "GoogleMapsProvider.txt";
        private readonly GoogleGeocodeMethodType MethodType;
        List<string> TypesAlloweds = new List<string> { "point_of_interest", "premise", "street_address", "route", "neighborhood" };

        /*
         * Types:
             * "point_of_interest": Indicates a named point of interest. Typically, these "POI"s are prominent local entities that don't easily fit in another category, such as "Empire State Building" or "Eiffel Tower".
             * "premise": Indicates a named location, usually a building or collection of buildings with a common name
             * "street_address":Indicates a precise street address.
             * "route": Indicates a named route (such as "US 101").
             * "neighborhood": Indicates a named neighborhood
         * LocationType:
            (*) "ROOFTOP" returns only the addresses for which Google has location information accurate down to street address precision.
             * "RANGE_INTERPOLATED" returns only the addresses that reflect an approximation (usually on a road) interpolated between two precise points (such as intersections). An interpolated range generally indicates that rooftop geocodes are unavailable for a street address.
             * "GEOMETRIC_CENTER" returns only geometric centers of a location such as a polyline (for example, a street) or polygon (region).
             * "APPROXIMATE" returns only the addresses that are characterized as approximate.
         */
        IReport ReportComparer;

        public GoogleProvider(string key, GoogleGeocodeMethodType methodType , IReport reportComparer)
        {
            GoogleMapsAPIKey = key;
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
                    var geocodeResult = JsonConvert.DeserializeObject<GoogleGeocodeResult>(content);
                    
                    GeocodeResult<Result> bestResult = GetBestGeocodeResult(geocodeResult, source);

                    string freeformAddress = bestResult.Result == null ? "NULL" : bestResult.Result.FormattedAddress;
                    string resultType = bestResult.Result == null ? "NULL" : bestResult.Result.Types[0];
                    ReportComparer.SaveAndDisplay(Path, $"Google Geocode;{source.Id};{source.Address};{source.Locality};{source.PostalCode};{source.Country};{resultType};{freeformAddress};{bestResult.Status};{geocodeResult.Results.Count}");

                }
                else
                {
                    //var error = await response.Content.ReadAsAsync<ErrorResponse>();
                    Console.WriteLine("Google HTTP Error");
                    ReportComparer.SaveAndDisplay(Path, $"Google Geocode;{source.Id};{source.Address};{source.Locality};{source.PostalCode};{source.Country};NULL;NULL;ZERO_RESULTS;0");

                }
            }
        }
        private GeocodeResult<Result> GetBestGeocodeResult(GoogleGeocodeResult geocodeResult, SourceAddress source)
        {
            var result = new GeocodeResult<Result>();

            if (geocodeResult.Results.Count == 0)
            {
                result.Status = geocodeResult.Status;
                return result;
            }

            var best = geocodeResult.Results[0]; //The best result

            var typeAllowed = best.Types.Contains(TypesAlloweds.FirstOrDefault(x => x.Equals(best.Types[0])));
            if (typeAllowed)
            {
                var postalCode = best.AddressComponents.Where(x => x.Types[0].Equals("postal_code")).FirstOrDefault();
                if (postalCode != null)
                    result.Status = postalCode.LongName.Equals(source.PostalCode) && best.Geometry.LocationType.Equals("ROOFTOP") ? "OK" : "TO_CHECK";
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


            string[] parameters =
            {
                source.Address.Replace(" ", "+"),
                source.Locality.Replace(" ", "+"),
                source.PostalCode, source.Country
            };

            switch (MethodType)
            {
                //if was processed with "AddressAndComponentsAll" use the next less strict method
                case GoogleGeocodeMethodType.AddressAndComponentsAll:
                    result = string.Format(AddressAndComponentsWithPostalCodeGeocodeUrl, (parameters));
                    break;

                //if was processed with "AddressAndComponentsWithPostalCode" use the next less strict method 
                case GoogleGeocodeMethodType.AddressAndComponentsWithPostalCode:
                    result = string.Format(OnlyAddressGeocodeUrl, (parameters));
                    break;

                default:
                    result = string.Format(AddressAndComponentsAllGeocodeUrl, (parameters));
                    break;
            }

            result = $"{result}&key={GoogleMapsAPIKey}";

            return result;
        }
    }
}
