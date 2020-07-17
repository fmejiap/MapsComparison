using Maps.Google;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Maps.Cartociudad
{
    public class CartociudadGeocodeResult:GoogleGeocodeResult
    {

    }
    public class CartociudadCandidatesResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("province")]
        public string Province { get; set; }

        [JsonProperty("muni")]
        public string Locality { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("postalCode")]
        public object PostalCode { get; set; }

        [JsonProperty("poblacion")]
        public string Town { get; set; }

        [JsonProperty("geom")]
        public string Geometry { get; set; }

        [JsonProperty("tip_via")]
        public string AddressType { get; set; }

        [JsonProperty("lat")]
        public double Latitude { get; set; }

        [JsonProperty("lng")]
        public double Longitude { get; set; }

        [JsonProperty("portalNumber")]
        public int PortalNumber { get; set; }

        [JsonProperty("stateMsg")]
        public string StateMessage { get; set; }

        [JsonProperty("state")]
        public CartociudadState State { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("refCatastral")]
        public string ReferenceCatastral { get; set; }
    }
    public class CartociudadFindResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("province")]
        public string Province { get; set; }

        [JsonProperty("muni")]
        public string Locality { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("postalCode")]
        public object PostalCode { get; set; }

        [JsonProperty("poblacion")]
        public string Town { get; set; }

        [JsonProperty("geom")]
        public string Geometry { get; set; }

        [JsonProperty("tip_via")]
        public string AddressType { get; set; }

        [JsonProperty("lat")]
        public double Latitude { get; set; }

        [JsonProperty("lng")]
        public double Longitude { get; set; }

        [JsonProperty("portalNumber")]
        public int PortalNumber { get; set; }

        [JsonProperty("stateMsg")]
        public string StateMessage { get; set; }

        [JsonProperty("state")]
        public CartociudadState State { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("refCatastral")]
        public string ReferenceCatastral { get; set; }
    }
    public class CartociudadGeometry
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("coordinates")]
        public List<List<List<double>>> Coordinates { get; set; }
    }
    public enum CartociudadState
    {
        ValueOne = 1,
        ValueTwo = 2,
        ValueThree = 3,
        ValueFour = 4,
        ValueFive = 5,
        ValueSix = 6,
        ValueTen = 10
    }
    public enum AddressStatusType
    {
        OK,
        TO_CHECK,
        ZERO_RESULTS,
        EXCEPTION,
        DAILY_LIMIT,
        AVG_LIMIT,
        USAGE_LIMIT,
        EMPTY_INPUT,
        PENDING,
        EXCEPTION_BEFORE_PROCESSED, //Exception thrown before process the address 
        OTHER
    }
}
