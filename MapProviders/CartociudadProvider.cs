using Maps.Cartociudad;
using Maps.Google;
using Maps.Report;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Maps
{
    public class CartociudadProvider : IGeocodeProvider
    {


        #region Constants
        internal const string DefaultCartociudadEndPointBase = "http://www.cartociudad.es";
        internal const string DefaultNoProcess = "";
        internal const string DefaultCountryCode = "es";
        internal const string DefaultLimit = "15";
        internal const string DefaultType = "callejero";
        internal const string DefaultPortal = "";
        internal const string DefaultOutputFormat = "";
        internal const string CandidateEnpoint = "geocoder/api/geocoder/candidatesJsonp";
        internal const string FindEnpoint = "geocoder/api/geocoder/findJsonp";
        private const string STREET_ADDRESS = "street_address";
        private const string STREET_NUMBER = "street_number";
        private const string ROUTE = "route";
        private const string LOCALITY = "locality";
        private const string ADMINISTRATIVE_AREA_LEVEL_2 = "administrative_area_level_2";
        private const string COUNTRY = "country";
        private const string POSTAL_CODE = "postal_code";
        #endregion
        string Path = "CartociudadProvider.txt";
        IReport ReportComparer;

        public CartociudadProvider(IReport reportComparer)
        {
            ReportComparer = reportComparer;
        }
        public async Task GeocodeAsync(SourceAddress source)
        {
            var geocode = new CartociudadGeocodeResult();

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/x-javascript"));

                var uri = BuildCandidateEndPoint(source.FormattedAddress, source.Country);
                var responseCandidates = await client.GetAsync(uri);

                if (responseCandidates.IsSuccessStatusCode)
                {
                    string serialized = await responseCandidates.Content.ReadAsStringAsync();
                    serialized = serialized.Substring(9, serialized.Length - 9);
                    serialized = serialized.Substring(0, serialized.Length - 1);

                    List<CartociudadCandidatesResponse> candidates = JsonConvert.DeserializeObject<List<CartociudadCandidatesResponse>>(serialized);

                    if(candidates.Count>0)
                    {
                        var bestCandidate = candidates.FirstOrDefault();

                        geocode = ValidateGeocodeCandidatesMatch(bestCandidate, source);

                        if (string.IsNullOrEmpty(geocode.Status))
                        {
                            var geocodeAddress = await SelectBestGeocodeAddressMatchAsync(bestCandidate);

                            geocode = CartociudadAddressToGeocodeAddressResponse(geocodeAddress, bestCandidate, source);
                        }
                    }


                    string freeformAddress = geocode.Results == null ? "NULL" : geocode.Results[0].FormattedAddress;
                    string resultType = geocode.Results == null ? "NULL" : geocode.Results[0].Types[0];
                    string status= geocode.Results == null ? "ZERO_RESULTS" : geocode.Status;
                    string results = geocode.Results == null ? "0" : geocode.Results.Count.ToString();
                    ReportComparer.SaveAndDisplay(Path, $"Cartociudad Geocode;{source.Id};{source.Address};{source.Locality};{source.PostalCode};{source.Country};{resultType};{freeformAddress};{status};{results}");
                }
                else
                {
                    //var error = await response.Content.ReadAsAsync<ErrorResponse>();
                    Console.WriteLine("Cartociudad HTTP Error");

                    ReportComparer.SaveAndDisplay(Path, $"Cartociudad Geocode;{source.Id};{source.Address};{source.Locality};{source.PostalCode};{source.Country};NULL;NULL;ZERO_RESULTS;0");

                }
            }
        }


        private async Task<CartociudadFindResponse> SelectBestGeocodeAddressMatchAsync(CartociudadCandidatesResponse cartociudadCandidatesResponse)
        {
            var streetNumber = cartociudadCandidatesResponse.PortalNumber.ToString();
            var result = new CartociudadFindResponse();

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/x-javascript"));

                var response = await client.GetAsync(BuildFindEndPoint(cartociudadCandidatesResponse.Id,streetNumber));

                if (response.IsSuccessStatusCode)
                {
                    string serialized = await response.Content.ReadAsStringAsync();
                    serialized = serialized.Substring(9, serialized.Length - 9);
                    serialized = serialized.Substring(0, serialized.Length - 1);

                    result = JsonConvert.DeserializeObject<CartociudadFindResponse>(serialized);
                  
                }
                else
                {
                    //var error = await response.Content.ReadAsAsync<ErrorResponse>();
                    Console.WriteLine("Cartociudad HTTP Error");
                    result = null;
                }
            }


            return await Task.FromResult(result);
        }

        private CartociudadGeocodeResult CartociudadAddressToGeocodeAddressResponse(CartociudadFindResponse cartociudadFindResponse, CartociudadCandidatesResponse cartociudadCandidatesResponse, SourceAddress sourceAddress)
        {
            var result = new CartociudadGeocodeResult();

            var addressComponents = BuildAddressComponents(cartociudadFindResponse, cartociudadCandidatesResponse, sourceAddress);
            var formatedAddress = BuildFormattedAddress(addressComponents);
            var placeId = BuildPlaceId(cartociudadFindResponse);

            result.Results = new List<Result>()
                {
                    new Result()
                    {
                        AddressComponents=addressComponents,
                        FormattedAddress=formatedAddress,
                        PlaceId=placeId,
                        Types=new List<string>(){STREET_ADDRESS}
                    }
                };

            result.Status = BuildStatus(cartociudadFindResponse);

            return result;
        }

        private string BuildPlaceId(CartociudadFindResponse cartociudadFindResponse)
        {
            return $"{cartociudadFindResponse.Id}{cartociudadFindResponse.PortalNumber.ToString()}";
        }

        private string BuildStatus(CartociudadFindResponse cartociudadFindResponse)
        {
            var result = CartociudadToGeocodeStatusConverter.Convert(cartociudadFindResponse.State).ToString();

            return result;
        }

        private List<AddressComponent> BuildAddressComponents(CartociudadFindResponse cartociudadFindResponse, CartociudadCandidatesResponse cartociudadCandidatesResponse, SourceAddress sourceAddress)
        {
            var result = new List<AddressComponent>();

            var component = cartociudadFindResponse.PortalNumber.ToString();
            result.Add(BuildAddressComponent<AddressComponent>(STREET_NUMBER, component, component));

            component = UpperCaseConverter.Convert(CartoCiudadRouteConverter.Convert(cartociudadFindResponse.Address ?? cartociudadCandidatesResponse.Address, cartociudadFindResponse.PortalNumber.ToString()));

            result.Add(BuildAddressComponent<AddressComponent>(ROUTE, component, component));

            component = UpperCaseConverter.Convert(cartociudadFindResponse.Locality ?? cartociudadCandidatesResponse.Locality);
            result.Add(BuildAddressComponent<AddressComponent>(LOCALITY, component, component));

            component = UpperCaseConverter.Convert(cartociudadFindResponse.Province ?? cartociudadCandidatesResponse.Province);
            result.Add(BuildAddressComponent<AddressComponent>(ADMINISTRATIVE_AREA_LEVEL_2, component, component));

            component = UpperCaseConverter.Convert(CartoCiudadToGeocodeCountryConverter.CountryConvert(cartociudadFindResponse.CountryCode ?? cartociudadCandidatesResponse.CountryCode));
            var shortComponent = CartoCiudadToGeocodeCountryConverter.CountryCodeConvert(cartociudadFindResponse.CountryCode ?? cartociudadCandidatesResponse.CountryCode);
            result.Add(BuildAddressComponent<AddressComponent>(COUNTRY, shortComponent, component));

            component = (cartociudadFindResponse.PostalCode == null ? sourceAddress.PostalCode : string.Empty).ToString();
            result.Add(BuildAddressComponent<AddressComponent>(POSTAL_CODE, component, component));

            return result;

        }
 
        private string BuildFormattedAddress(List<AddressComponent> addressComponents)
        {

            var address = addressComponents.Where(ac => ac.Types[0].Equals(ROUTE)).FirstOrDefault().LongName.Trim();

            var streetNumber = addressComponents.FirstOrDefault(ac => ac.Types[0].Equals(STREET_NUMBER)).LongName.Trim();

            var postalCode = addressComponents.FirstOrDefault(ac => ac.Types[0].Equals(POSTAL_CODE)).LongName.Trim();

            var town = addressComponents.FirstOrDefault(ac => ac.Types[0].Equals(LOCALITY)).LongName.Trim();

            var province = addressComponents.FirstOrDefault(ac => ac.Types[0].Equals(ADMINISTRATIVE_AREA_LEVEL_2)).LongName.Trim();

            var country = addressComponents.FirstOrDefault(ac => ac.Types[0].Equals(COUNTRY)).LongName.Trim();

            var result = $"{address},{streetNumber},{postalCode} {town},{province},{country}";

            return result;
        }

        private CartociudadGeocodeResult ValidateGeocodeCandidatesMatch(CartociudadCandidatesResponse candidates, SourceAddress request)
        {
            var result = new CartociudadGeocodeResult();

            if (candidates == null)
            {
                result.Status = AddressStatusType.ZERO_RESULTS.ToString();
                return result;
            }

            if (candidates.State != CartociudadState.ValueOne)
            {
                result.Status = CartociudadToGeocodeStatusConverter.Convert(candidates.State).ToString();
                
                return result;
            }

            //Check if portal number is ready
            if (candidates.PortalNumber.Equals(0))
            {
                result.Status = AddressStatusType.TO_CHECK.ToString();
              
                return result;
            }

            if (!candidates.PostalCode.Equals(request.PostalCode))
            {
                result.Status = AddressStatusType.TO_CHECK.ToString();
          
                return result;
            }

            return result;

        }

        //TODO: Centralized this condition.
        private GoogleGeocodeResult ValidateCountryCodeAllowed(SourceAddress request)
        {
            const string COUNTRY_CODE_ALLOWED = "ES";
            var result = new GoogleGeocodeResult();

            if (!request.Country.Equals(COUNTRY_CODE_ALLOWED))
            {
                result.Status = AddressStatusType.OTHER.ToString();
                return result;
            }

            return result;

        }

        private T BuildAddressComponent<T>(string type, string shortName, string longName) where T : AddressComponent, new()
        {
            var result = new T
            {
                Types = new List<string>() { type },
                ShortName = shortName.Trim(),
                LongName = longName.Trim()
            };

            return result;
        }
        private string BuildCandidateEndPoint(string query, string countryCode = DefaultCountryCode)
        {
            var uriBase = new UriBuilder(DefaultCartociudadEndPointBase);

            var builder = new StringBuilder();
            builder.Append($"{uriBase}{CandidateEnpoint.Trim()}");

            if (!string.IsNullOrEmpty(query))
            {
                builder.Append("?q=");
                builder.Append(WebUtility.UrlEncode(query));
            }

            if (!string.IsNullOrEmpty(DefaultNoProcess))
            {
                builder.Append("&no_process=");
                builder.Append(WebUtility.UrlEncode(DefaultNoProcess));
            }

            if (!string.IsNullOrEmpty(countryCode))
            {
                builder.Append("&countrycodes=");
                builder.Append(WebUtility.UrlEncode(countryCode));
            }

            if (!string.IsNullOrEmpty(DefaultLimit))
            {
                builder.Append("&limit=");
                builder.Append(WebUtility.UrlEncode(DefaultLimit));
            }
            return builder.ToString();
        }

        private string BuildFindEndPoint(string id, string portal)
        {
            var uriBase = new UriBuilder(DefaultCartociudadEndPointBase);
            uriBase.Path = FindEnpoint;

            var builder = new StringBuilder();
            builder.Append(uriBase.ToString());

            if (!string.IsNullOrEmpty(id))
            {
                builder.Append("?id=");
                builder.Append(WebUtility.UrlEncode(id));
            }

            if (!string.IsNullOrEmpty(DefaultType))
            {
                builder.Append("&type=");
                builder.Append(WebUtility.UrlEncode(DefaultType));
            }

            if (!string.IsNullOrEmpty(portal))
            {
                builder.Append("&portal=");
                builder.Append(WebUtility.UrlEncode(portal));
            }

            if (!string.IsNullOrEmpty(DefaultOutputFormat))
            {
                builder.Append("&outputformat=");
                builder.Append(WebUtility.UrlEncode(DefaultOutputFormat));
            }
            return builder.ToString();
        }
    }
}
