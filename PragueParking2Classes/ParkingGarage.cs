using DataAccess;
using Spectre.Console;

namespace PragueParking2.Classes
{
    public class ParkingGarage
    {
        public List<ParkingSpot> Garage { get; set; }

        public ParkingGarage()
        {
            Garage = new List<ParkingSpot>();
        }

        public int Size { get; set; }

        public void InitializeFromConfig(GarageConfig config)
        {
            Size = config.GarageSize;

            Garage = new List<ParkingSpot>(Size);
            for (int i = 1; i <= Size; i++)
            {
                Garage.Add(new ParkingSpot(spotNumber: i, config));
            }
        }

        //-------------------------------------------------
        //Gemensam metod för att parkera fordon. Anropas från menyn med ParkVehicle()
        public void ParkVehicle(Vehicle.VehicleType vehicleType)
        {
            Console.Clear();

            //Fordonstyp för utskrift
            string vehicleName = vehicleType.ToString();

            AnsiConsole.Markup($"Your [yellow]vehicle[/] type is: [yellow]{vehicleName}[/].\n\nPlease enter your [cyan]license plate[/]: ");
            string regNumber = Console.ReadLine().ToUpper();

            //Villkor för registreringsnummer
            if (!LicensePlateValidation(regNumber) || !IsLicensePlateAlreadyParked(regNumber))
            {
                return;
            }

            var config = GarageConfig.LoadOrCreate();

            //Skapar rätt typ av fordon. Kan vid behov lägga till fler fordonstyper här
            Vehicle vehicle = null;
            if (vehicleType == Vehicle.VehicleType.Car)
            {
                vehicle = new Car(regNumber, config);
            }
            else if (vehicleType == Vehicle.VehicleType.MC)
            {
                vehicle = new MC(regNumber, config);
            }

            //Kontrollerar om fordonet är för stort för en plats med referens till config
            if (vehicle.Size > config.SpotSize)
            {
                Console.Clear();
                AnsiConsole.MarkupLine("Not enough space in the parking spot for this type of [yellow]vehicle[/].");
                AnsiConsole.MarkupLine("\n\nPress any [bold]key[/] to go back to the menu...");
                Console.ReadKey();
                Console.Clear();
                return;
            }

            //Hittar ledig plats
            ParkingSpot spot = FindAvailableSpot(vehicle);

            if (spot != null)
            {
                spot.AddVehicle(vehicle);
                Console.Clear();

                var panel = new Panel($"[yellow]{vehicleName}[/] with license plate \"[cyan]{regNumber}[/]\" has been parked on spot [orchid]{spot.SpotNumber}[/] at [darkorange3]{vehicle.Arrival}[/].");
                panel.Header("[green slowblink]Success![/]", Justify.Center);
                AnsiConsole.Write(panel);

                AnsiConsole.MarkupLine("\n\nPress any [bold]key[/] to go back to the menu...");
                Console.ReadKey();
                Console.Clear();
            }
            else
            {
                Console.Clear();
                AnsiConsole.MarkupLine("[red]No available parking spots for your vehicle at the moment.[/]");
                AnsiConsole.MarkupLine("\n\nPress any [bold]key[/] to go back to the menu...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        public void ParkVehicleMenu()
        {
            //Kan vid behov lägga till fler fordonstyper här
            Console.Clear();
            var menuChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("  Select your [yellow]vehicle[/] type")
                    .PageSize(3)
                    .HighlightStyle("SandyBrown")
                    .AddChoices("[yellow]Car[/]", "[yellow]MC[/]", "Go back to the menu"));
            if (menuChoice == "[yellow]Car[/]")
            {
                ParkVehicle(Vehicle.VehicleType.Car);
            }
            else if (menuChoice == "[yellow]MC[/]")
            {
                ParkVehicle(Vehicle.VehicleType.MC);
            }
            else if (menuChoice == "Go back to the menu")
            {
                Console.Clear();
            }
        }
        //-------------------------------------------------

        //Metod för att visa parkeringshuset
        public void DisplayParkinglot()
        {
            Console.Clear();

            AnsiConsole.MarkupLine("\t\t\t\t\t[bold underline]Displaying current parking garage status[/]\n");

            //Färgförklaring beroende på platsens status
            AnsiConsole.MarkupLine("\t\t\t\t\t     [lime]■[/] Empty   [gold1]■[/] Half full   [red]■[/] Full");

            int occupiedSpots = GetOccupiedSpotsCount();

            int totalVehicles = 0;

            foreach (var spot in Garage)
            {
                totalVehicles += spot.ParkedVehicles.Count;
            }
            AnsiConsole.MarkupLine($"\t\t\t\t\t        [bold]Occupied [orchid]spots[/]:[/] [red]{occupiedSpots}[/] / [green]{Garage.Count}[/]");
            AnsiConsole.MarkupLine($"\t\t\t\t\t\t  [bold]Total [yellow]vehicles[/]:[/] [red]{totalVehicles}[/]\n");

            int columns = 10;
            int totalSpots = Garage.Count;

            var grid = new Table();
            grid.Border(TableBorder.Rounded);
            grid.Centered();

            for (int i = 0; i < columns; i++)
                grid.AddColumn(new TableColumn(""));

            grid.HideHeaders();

            var currentRow = new List<Markup>();

            for (int i = 0; i < totalSpots; i++)
            {
                var spot = Garage[i];
                string color;

                //Tom plats
                if (spot.ParkedVehicles.Count == 0)
                {
                    color = "lime";
                }
                //Halvfull plats
                else if (spot.AvailableSize > 0)
                {
                    color = "gold1";
                }
                //Full plats
                else
                {
                    color = "red";
                }
                currentRow.Add(new Markup($"[bold {color}]{spot.SpotNumber}[/]"));

                if ((i + 1) % columns == 0 || i == totalSpots - 1)
                {
                    while (currentRow.Count < columns) currentRow.Add(new Markup(" "));
                    grid.AddRow(currentRow.ToArray());
                    currentRow.Clear();
                }
            }

            grid.ShowRowSeparators();
            AnsiConsole.Write(grid);

            //Forden som är parkerade men info om dem
            if (totalVehicles > 0)
            {
                var vehicleTable = new Table();
                vehicleTable.Centered();
                vehicleTable.AddColumn("[yellow italic]Vehicle type[/]");
                vehicleTable.AddColumn("[cyan italic]License plate[/]");
                vehicleTable.AddColumn("[darkorange3 italic]Time of entry[/]");
                vehicleTable.AddColumn("[orchid]Spot number[/]");

                foreach (var spot in Garage)
                {
                    foreach (var vehicle in spot.ParkedVehicles)
                    {
                        vehicleTable.AddRow(
                            $"[yellow]{vehicle.GetType().Name}[/]",
                            $"[cyan]{vehicle.RegNumber}[/]",
                            $"[darkorange3]{vehicle.Arrival}[/]",
                            $"[orchid]{spot.SpotNumber}[/]"
                        );
                    }
                }

                AnsiConsole.Write(vehicleTable);
            }
            else
            {
                AnsiConsole.MarkupLine("[green]No vehicles currently parked.[/]");
            }

            AnsiConsole.MarkupLine("\nPress any [bold]key[/] to go back to the menu...");
            Console.ReadKey(true);
            Console.Clear();
            Console.WriteLine("\x1b[3J");
            Console.Clear();
        }

        //-------------------------------------------------
        //Metoder för att flytta och ta bort fordon

        //Anropas i MoveOrRemoveVehicleMenu
        public void MoveVehicle()
        {
            Console.Clear();
            AnsiConsole.Markup("Enter the [cyan]license plate[/] number of the vehicle you want to move: ");
            string regNumber = Console.ReadLine().ToUpper();

            if (!LicensePlateValidation(regNumber))
            {
                return;
            }

            Vehicle vehicleToMove = null;
            ParkingSpot spotToMoveFrom = null;

            //Letar efter fordonet med hjälp av regnr i alla parkeringsplatser
            foreach (var spot in Garage)
            {
                foreach (var vehicle in spot.ParkedVehicles)
                {
                    if (vehicle.RegNumber == regNumber)
                    {
                        vehicleToMove = vehicle;
                        spotToMoveFrom = spot;
                        break;
                    }
                }
                if (vehicleToMove != null)
                {
                    break;
                }
            }

            if (vehicleToMove == null)
            {
                Console.Clear();
                AnsiConsole.MarkupLine($"[yellow]Vehicle[/] [red]with license plate [cyan]{regNumber}[/] not found.[/]");
                AnsiConsole.MarkupLine("\n\nPress any [bold]key[/] to go back to the menu...");
                Console.ReadKey();
                Console.Clear();
                return;
            }

            AnsiConsole.Markup("Enter the new parking [orchid]spot[/] number: ");
            if (!int.TryParse(Console.ReadLine(), out int newSpotNumber) || newSpotNumber <= 0 || newSpotNumber > Garage.Count)
            {
                Console.Clear();
                AnsiConsole.MarkupLine("[red]Invalid parking spot number.[/]");
                AnsiConsole.MarkupLine("\n\nPress any [bold]key[/] to go back to the menu...");
                Console.ReadKey();
                Console.Clear();
                return;
            }

            var spotToMoveTo = Garage[newSpotNumber - 1];

            //Kontrollerar om platsen har tillräckligt med utrymme för fordonet
            if (vehicleToMove.Size > spotToMoveTo.AvailableSize)
            {
                Console.Clear();
                AnsiConsole.MarkupLine($"[red]Cannot move [yellow]vehicle[/] to spot [orchid]{spotToMoveTo.SpotNumber}[/]. Not enough space.[/]");
                AnsiConsole.MarkupLine($"[yellow]Vehicle[/] size: {vehicleToMove.Size}, [green]Available space:[/] {spotToMoveTo.AvailableSize}");
                AnsiConsole.MarkupLine("\n\nPress any [bold]key[/] to go back to the menu...");
                Console.ReadKey();
                Console.Clear();
                return;
            }

            //Flyttar fordonet
            spotToMoveFrom.RemoveVehicle(vehicleToMove);
            spotToMoveTo.AddVehicle(vehicleToMove);

            Console.Clear();
            var panel = new Panel($"[yellow]{vehicleToMove.Type}[/] with license plate \"[cyan]{regNumber}[/]\" has been moved from spot [orchid]{spotToMoveFrom.SpotNumber}[/] to spot [orchid]{spotToMoveTo.SpotNumber}[/].");
            panel.Header("[green slowblink]Success![/]", Justify.Center);
            AnsiConsole.Write(panel);

            AnsiConsole.MarkupLine("\n\nPress any [bold]key[/] to go back to the menu...");
            Console.ReadKey();
            Console.Clear();
        }
        public void RemoveVehicle()
        {
            Console.Clear();
            AnsiConsole.Markup("Enter the [cyan]license plate[/] number of the [yellow]vehicle[/] you want to remove: ");
            string regNumber = Console.ReadLine().ToUpper();

            if (!LicensePlateValidation(regNumber))
            {
                return;
            }

            //Letar efter fordonet
            foreach (var spot in Garage)
            {
                foreach (var vehicle in spot.ParkedVehicles)
                {
                    //Om fordonet hittas
                    if (vehicle.RegNumber == regNumber)
                    {
                        Console.Clear();
                        AnsiConsole.MarkupLine($"[yellow]{vehicle.GetType().Name}[/] with license plate \"[cyan]{regNumber}[/]\" has been removed from spot [orchid]{spot.SpotNumber}[/]");

                        spot.ParkedTime(vehicle);
                        spot.RemoveVehicle(vehicle);

                        AnsiConsole.MarkupLine("\n\nPress any [bold]key[/] to go back to the menu...");
                        Console.ReadKey();
                        Console.Clear();
                        return;
                    }
                }
            }

            //Om fordonet inte hittas
            Console.Clear();
            AnsiConsole.MarkupLine($"[red]A vehicle with license plate [yellow]{regNumber}[/] does not exist in the parking lot.[/]");
            AnsiConsole.MarkupLine("\n\nPress any [bold]key[/] to go back to the menu...");
            Console.ReadKey();
            Console.Clear();
        }
        public void MoveOrRemoveVehicleMenu()
        {
            Console.Clear();
            var menuChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .PageSize(3)
                    .HighlightStyle("SandyBrown")
                    .AddChoices("Remove a [yellow]vehicle[/]", "Move a [yellow]vehicle[/]", "Go back to the menu"));
            if (menuChoice == "Remove a [yellow]vehicle[/]")
            {
                RemoveVehicle();
            }
            else if (menuChoice == "Move a [yellow]vehicle[/]")
            {
                MoveVehicle();
            }
            else if (menuChoice == "Go back to the menu")
            {
                Console.Clear();
            }
        }
        //-------------------------------------------------

        //Metod för att söka efter fordon
        public void SearchVehicle()
        {
            //Metod för att söka fordon
            Console.Clear();
            AnsiConsole.Markup("Enter the [cyan]license plate[/] number of the [yellow]vehicle[/] you want to search for: ");
            string regNumber = Console.ReadLine().ToUpper();
            if (!LicensePlateValidation(regNumber))
            {
                return;
            }
            foreach (var spot in Garage)
            {
                foreach (var vehicle in spot.ParkedVehicles)
                {
                    if (vehicle.RegNumber == regNumber)
                    {
                        Console.Clear();
                        AnsiConsole.MarkupLine($"[yellow]{vehicle.GetType().Name}[/] with license plate \"[cyan]{regNumber}[/]\" on parking spot [orchid]{spot.SpotNumber}[/] arrived at: [darkorange3]{vehicle.Arrival}[/].");
                        spot.ParkedTime(vehicle);
                        AnsiConsole.MarkupLine("\n\nPress any [bold]key[/] to go back to the menu...");
                        Console.ReadKey();
                        Console.Clear();
                        return;
                    }
                }
            }
            Console.Clear();
            AnsiConsole.MarkupLine($"[red]A [yellow]vehicle[/] with license plate \"{regNumber}\" does not exist in the parking lot.[/]");
            Console.ReadKey();
            Console.Clear();
        }
        //-------------------------------------------------

        //Villkor för registreringsnummer
        public bool LicensePlateValidation(string regNumber)
        {
            if (regNumber.Length > 10)
            {
                Console.Clear();
                AnsiConsole.MarkupLine("[red]Your license plate cannot contain more than 10 characters![/]\n");
                AnsiConsole.MarkupLine("\n\nPress any [bold]key[/] to go back to the menu...");
                Console.ReadKey();
                Console.Clear();
                return false;
            }
            if (regNumber.Length == 0)
            {
                Console.Clear();
                AnsiConsole.MarkupLine("[red]You must enter a license plate number![/]\n");
                AnsiConsole.MarkupLine("\n\nPress any [bold]key[/] to go back to the menu...");
                Console.ReadKey();
                Console.Clear();
                return false;
            }
            if (regNumber.Any(char.IsWhiteSpace))
            {
                Console.Clear();
                AnsiConsole.MarkupLine("[red]Your license plate cannot contain spaces![/]\n");
                AnsiConsole.MarkupLine("\n\nPress any [bold]key[/] to go back to the menu...");
                Console.ReadKey();
                Console.Clear();
                return false;
            }
            return true;
        }

        public bool IsLicensePlateAlreadyParked(string regNumber)
        {
            foreach (var spot in Garage)
            {
                foreach (var vehicle in spot.ParkedVehicles)
                {
                    if (vehicle.RegNumber == regNumber)
                    {
                        Console.Clear();
                        AnsiConsole.MarkupLine($"[red]A vehicle [yellow]({vehicle.GetType().Name})[/] with license plate \"[cyan]{vehicle.RegNumber}[/]\" is already parked in the parking lot![/]\n");
                        AnsiConsole.MarkupLine("\n\nPress any [bold]key[/] to go back to the menu...");
                        Console.ReadKey();
                        Console.Clear();
                        return false;
                    }
                }
            }
            return true;
        }

      
        //Räkna antal platser som innehåller minst ett fordon. Anropas i config
        public int GetOccupiedSpotsCount()
        {
            int occupiedSpots = 0;
            foreach (var spot in Garage)
            {
                if (spot.ParkedVehicles.Count > 0)
                {
                    occupiedSpots++;
                }
            }
            return occupiedSpots;
        }

        private ParkingSpot FindAvailableSpot(Vehicle vehicle)
        {
            foreach (var spot in Garage)
            {
                if (spot.IsThereRoomForVehicle(vehicle))
                {
                    return spot;
                }
            }
            //Om ingen ledig plats hittas så return null
            return null;
        }
    }
}
