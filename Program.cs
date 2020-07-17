using Maps.Classes;
using Maps.Report;
using System;
using System.Collections.Generic;

namespace Maps
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start - GeoPosition comparison");

            string bingKey = " ";
            string azureKey = " ";
            string googleKey = " ";
            string argisKey = " ";

            List<SourceAddress> addressList = DataTest.GetAddress();
            IReport reportComparer = new ReportComparer();
            AzureMapsGeocodeMethodType azureMapsGeocodeMethodType = AzureMapsGeocodeMethodType.GeocodeSearchAddress;
            BingMapsGeocodeMethodType bingMapsGeocodeMethodType = BingMapsGeocodeMethodType.GeocodeFindLocationByQuery;
            GoogleGeocodeMethodType googleGeocodeMethodType=GoogleGeocodeMethodType.AddressAndComponentsWithPostalCode;

            AzureProvider azureMapsGeocoder = new AzureProvider(azureKey, azureMapsGeocodeMethodType, reportComparer);
            BingProvider bingMapsGeocoder = new BingProvider(bingKey, bingMapsGeocodeMethodType, reportComparer);
            GoogleProvider googleGeocoder = new GoogleProvider(googleKey, googleGeocodeMethodType, reportComparer);
            CartociudadProvider cartociudadGeocoder = new CartociudadProvider(reportComparer);
            ArcGisProvider arcGisProvider = new ArcGisProvider(argisKey,reportComparer);

            foreach (var address in addressList)
            {
                azureMapsGeocoder.GeocodeAsync(address).Wait();
                bingMapsGeocoder.GeocodeAsync(address).Wait();
                googleGeocoder.GeocodeAsync(address).Wait();
                cartociudadGeocoder.GeocodeAsync(address).Wait();
                arcGisProvider.GeocodeAsync(address).Wait();
            }

            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}
