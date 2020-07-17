using Maps.ArcGis;
using Maps.Report;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Maps
{
    public class ArcGisProvider : IGeocodeProvider
    {
        readonly string SubscriptionKey;
        readonly IReport ReportComparer;
        readonly string Path = "ArcGisProvider.txt";
        List<string> TypesAlloweds = new List<string> { "PointAddress", "StreetAddress", "StreetInt", "POI","StreetAddressExt" };

        public ArcGisProvider(string key, IReport reportComparer)
        {
            SubscriptionKey = key;
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

                    ArcGisGeocodeResult geocode = JsonConvert.DeserializeObject<ArcGisGeocodeResult>(content);
                    GeocodeResult<Candidate> bestResult = GetBestGeocodeResult(geocode, source);

                    string freeformAddress = bestResult.Result == null ? "NULL" : bestResult.Result.Address;
                    string resultType = bestResult.Result == null ? "NULL" : bestResult.Result.Attributes.AddrType;

                    ReportComparer.SaveAndDisplay(Path, $"ArcGis;{source.Id};{source.Address};{source.Locality};{source.PostalCode};{source.Country};{resultType};{freeformAddress};{bestResult.Status};{geocode.Candidates.Count};{CountByScore(geocode)}");

                }
                else
                {
                    //var error = await response.Content.ReadAsAsync<ErrorResponse>();
                    Console.WriteLine("ArcGis HTTP Error");
                    ReportComparer.SaveAndDisplay(Path, $"ArcGis;{source.Id};{source.Address};{source.Locality};{source.PostalCode};{source.Country};NULL;NULL;ZERO_RESULT;0");
                }
            }
        }
        private GeocodeResult<Candidate> GetBestGeocodeResult(ArcGisGeocodeResult geocodeResult, SourceAddress source)
        {
            var result = new GeocodeResult<Candidate>();

            if (geocodeResult.Candidates.Count == 0)
            {
                result.Status = "ZERO_RESULTS";
                return result;
            }

            var best = geocodeResult.Candidates[0]; //The best result

            if (string.IsNullOrEmpty(best.Attributes.Country))
            {
                result.Status = "ZERO_RESULT";
                return result;
            }

            if (!best.Attributes.Country.Equals("ESP"))
            {
                
                result.Status = "ZERO_RESULT";
                return result;
            }

            var typeAllowed = best.Attributes.AddrType.Equals(TypesAlloweds.FirstOrDefault(x => x.Equals(best.Attributes.AddrType)) ?? string.Empty);
            if (typeAllowed)
            {
                result.Status = best.Attributes.Postal.Equals(source.PostalCode) ? "OK" : "TO_CHECK";
            }
            else
            {
                result.Status = "TO_CHECK";
            }

            result.Result = best;
            return result;

        }
        private string BuildUrlRequest(SourceAddress source)
        {
            string result = $"https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/findAddressCandidates?f=json&singleLine={source.FormattedAddress}&outFields=Match_addr,Addr_type,postal,country";
           
            return result;
        }

        private string CountByScore (ArcGisGeocodeResult geocode)
        {
            var result = from candidates in geocode.Candidates
                     group candidates by candidates.Score into grp
                     select new { key = grp.Key, cnt = grp.Count() };

            var str = string.Empty;
            foreach (var item in result)
            {
                str += $"score:{item.key}-cnt:{item.cnt}|";
            }

            str = str.Remove(str.Length - 1, 1);
            return str;
        }
    }
}
