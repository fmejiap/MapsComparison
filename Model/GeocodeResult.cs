using System;

namespace Maps
{
    public class GeocodeResult<T> where T : new()
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public T Result { get; set; }
        public GeocodeResult()
        {
            Id = Guid.NewGuid();
        }
    }
}
