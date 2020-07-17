using System;
using System.Collections.Generic;

namespace Maps
{
    public class DataTest
    {
        public static List<SourceAddress> GetAddress()
        {
            var addressList = new List<SourceAddress>(); 
            addressList.Add(new SourceAddress() { Id = "1", Address = "CL CARRER DE LAUREÀ MIRÓ, 20", Locality = "ESPLUGUES DE LLOBREGAT", PostalCode = "08950", Country = "ES" });
            
            return addressList;
        }
    }
}