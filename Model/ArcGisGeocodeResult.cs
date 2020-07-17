using Newtonsoft.Json;
using System.Collections.Generic;

namespace Maps.ArcGis
{
    public partial class ArcGisGeocodeResult
    {
        [JsonProperty("spatialReference")]
        public SpatialReference SpatialReference { get; set; }

        [JsonProperty("candidates")]
        public List<Candidate> Candidates { get; set; }
    }

    public partial class Candidate
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("location")]
        public Location Location { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }

        [JsonProperty("attributes")]
        public Attributes Attributes { get; set; }

        [JsonProperty("extent")]
        public Extent Extent { get; set; }
    }

    public partial class Attributes
    {
        [JsonProperty("Match_addr")]
        public string MatchAddr { get; set; }

        [JsonProperty("Addr_type")]
        public string AddrType { get; set; }

        [JsonProperty("postal")]
        public string Postal { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }

    public partial class Extent
    {
        [JsonProperty("xmin")]
        public double Xmin { get; set; }

        [JsonProperty("ymin")]
        public double Ymin { get; set; }

        [JsonProperty("xmax")]
        public double Xmax { get; set; }

        [JsonProperty("ymax")]
        public double Ymax { get; set; }
    }

    public partial class Location
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }
    }

    public partial class SpatialReference
    {
        [JsonProperty("wkid")]
        public long Wkid { get; set; }

        [JsonProperty("latestWkid")]
        public long LatestWkid { get; set; }
    }
}
