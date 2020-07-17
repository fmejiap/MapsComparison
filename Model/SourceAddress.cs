using System;

namespace Maps
{
    public class SourceAddress
    {
        public string Id { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string Locality { get; set; }
        public string Country { get; set; }
        public string FormattedAddress => $"{Address},{PostalCode},{Locality}";

    }
}