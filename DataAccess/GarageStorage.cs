using System.Text.Json;
using PragueParking2.Classes;

namespace DataAccess
{
    public class GarageStorage
    {
        string fileName = "../../../garage.json";

        //Sparar garage till fil
        public void SaveGarage(ParkingGarage garage)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(garage, options);
            File.WriteAllText(fileName, json);
        }

        //Laddar garage från fil
        public ParkingGarage LoadGarage()
        {
            var config = GarageConfig.LoadOrCreate();
            ParkingGarage garage;

            if (!File.Exists(fileName))
            {
                garage = new ParkingGarage();
                garage.InitializeFromConfig(config);
                return garage;
            }

            string json = File.ReadAllText(fileName);
            garage = JsonSerializer.Deserialize<ParkingGarage>(json);

            if (garage == null || garage.Garage == null || garage.Garage.Count == 0)
            {
                garage = new ParkingGarage();
                garage.InitializeFromConfig(config);
            }
            //För att kunna återställa rätt typ av fordon (Car eller MC) efter deserialisering. Kan lägga till fler fordonstyper här vid behov.
            foreach (var spot in garage.Garage)
            {
                for (int i = 0; i < spot.ParkedVehicles.Count; i++)
                {
                    var vehicle = spot.ParkedVehicles[i];

                    if (vehicle.Type == Vehicle.VehicleType.Car)
                    {
                        spot.ParkedVehicles[i] = new Car(vehicle.RegNumber, config)
                        {
                            Arrival = vehicle.Arrival
                        };
                    }
                    else if (vehicle.Type == Vehicle.VehicleType.MC)
                    {
                        spot.ParkedVehicles[i] = new MC(vehicle.RegNumber, config)
                        {
                            Arrival = vehicle.Arrival
                        };
                    }
                }
                //Återställ AvailableSize
                int usedSize = 0;
                foreach (var v in spot.ParkedVehicles)
                {
                    usedSize += v.Size;
                }
                spot.AvailableSize = spot.Size - usedSize;
            }
            return garage;
        }
    }
}

