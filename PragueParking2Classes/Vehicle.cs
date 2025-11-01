namespace PragueParking2.Classes
{
    public class Vehicle
    {
        //enum för att kunna identifiera fordonstyp vid deserialisering
        public enum VehicleType
        {
            Car,
            MC
            //Kan lägga till fler fordonstyper här vid behov
        }
        public VehicleType Type { get; set; }
        public string? RegNumber { get; set; }
        public int Size { get; set; }
        public DateTime Arrival { get; set; } = DateTime.Now;
        public int PricePerHour { get; set; }
        public Vehicle(string regNumber)
        {
            RegNumber = regNumber;
        }
    }
}