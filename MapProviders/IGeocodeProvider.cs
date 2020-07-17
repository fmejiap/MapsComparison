using System.Threading.Tasks;

namespace Maps
{
    public interface IGeocodeProvider
    {
        public Task GeocodeAsync(SourceAddress source);

    }
}
