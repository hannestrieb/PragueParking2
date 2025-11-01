using DataAccess;

namespace PragueParking2.Classes
{
    public class MC : Vehicle
    {
        public MC(string regNumber, GarageConfig config) : base(regNumber)
        {
            Size = config.McSize;
            PricePerHour = config.McPricePerHour;
            Type = VehicleType.MC;
        }
    }
}