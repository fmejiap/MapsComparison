namespace Maps
{
    public enum GoogleGeocodeMethodType
    {
        None = 0, //It´s a new address
        AddressAndComponentsAll = 1, //A + C = all
        AddressAndComponentsWithPostalCode = 2, //A + C = CP 
        OnlyAddress = 3, //Only A
    }
}
