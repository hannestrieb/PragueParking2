using DataAccess;
using Spectre.Console;

namespace PragueParking2.Classes
{
    public class ParkingSpot
    {
        public int Size { get; set; }
        public int AvailableSize { get; set; }
        public int SpotNumber { get; set; }
        public List<Vehicle> ParkedVehicles { get; set; }
        public ParkingSpot(int spotNumber, GarageConfig config)
        {
            Size = config.SpotSize;
            SpotNumber = spotNumber;
            AvailableSize = Size;
            ParkedVehicles = new List<Vehicle>();
        }

        //Konstruktor för enhetstester
        public ParkingSpot(int spotNumber, int spotSize)
        {
            SpotNumber = spotNumber;
            Size = spotSize;
            AvailableSize = spotSize;
            ParkedVehicles = new List<Vehicle>();
        }

        //Parameterlös konstruktor för deserialisering
        public ParkingSpot()
        {
            ParkedVehicles = new List<Vehicle>();
        }

        public void AddVehicle(Vehicle vehicle)
        {
            ParkedVehicles.Add(vehicle);
            AvailableSize -= vehicle.Size;
        }
        public void RemoveVehicle(Vehicle vehicle)
        {
            ParkedVehicles?.Remove(vehicle);
            AvailableSize += vehicle.Size;
        }
        public void ParkedTime(Vehicle vehicle)
        {
            Console.WriteLine("");
            TimeSpan parkedTime = DateTime.Now - vehicle.Arrival;
            int totalHoursForPrice = (int)Math.Ceiling(parkedTime.TotalHours);
            //Första 10 minuter gratis
            int totalMinutes = (int)parkedTime.TotalMinutes;
            int totalHoursRoundedDown = (int)parkedTime.TotalHours;
            int andMinutes = parkedTime.Minutes;
            int totalPrice = totalHoursForPrice * vehicle.PricePerHour;
            string panelText;
            if (totalMinutes <= 10)
            {
                panelText = $"Total park time: [darkorange3]{totalMinutes}[/] [italic]minutes[/]. Total price: [pink3]0 CZK[/]";
            }
            else if (totalMinutes < 60)
            {
                panelText = $"Total park time: [darkorange3]{totalMinutes}[/] [italic]minutes[/]. Total price: [pink3]{vehicle.PricePerHour} CZK[/]";
            }
            else if (totalHoursRoundedDown == 1)
            {
                panelText = $"Total park time: [darkorange3]{totalHoursRoundedDown}[/] [italic]hour[/] [darkorange3]{andMinutes}[/] [italic]minutes[/]. Total price: [pink3]{totalPrice} CZK[/]";
            }
            else
            {
                panelText = $"Total park time: [darkorange3]{totalHoursRoundedDown}[/] [italic]hours[/] [darkorange3]{andMinutes}[/] [italic]minutes[/]. Total price: [pink3]{totalPrice} CZK[/]";
            }
            Panel panel = new Panel(panelText)
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Tan),
                Padding = new Padding(1, 1, 2, 1),
                Header = new PanelHeader("[yellow slowblink]Parking info[/]", Justify.Center)

            };
            AnsiConsole.Write(panel);
        }
        public bool IsThereRoomForVehicle(Vehicle vehicle)
        {
            return (vehicle.Size <= AvailableSize);
        }
        public bool CheckForRegNumber(string regNumber)
        {
            foreach (var vehicle in ParkedVehicles)
            {
                if (vehicle.RegNumber == regNumber)
                    return true;
            }
            return false;
        }
    }
}