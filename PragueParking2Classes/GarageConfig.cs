using Spectre.Console;
using PragueParking2.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataAccess
{
    public class GarageConfig
    {
        public string GarageName { get; set; }
        public int GarageSize { get; set; }
        public int CarSize { get; set; }
        public int CarPricePerHour { get; set; }
        public int McSize { get; set; }
        public int McPricePerHour { get; set; }
        public int SpotSize { get; set; }

        private static readonly string fileName = "../../../garageconfig.json";

        public static GarageConfig LoadOrCreate()
        {
            if (File.Exists(fileName))
            {
                string json = File.ReadAllText(fileName);
                return JsonSerializer.Deserialize<GarageConfig>(json);
            }
            else
            {
                //Hårdkodade standardvärden
                var config = new GarageConfig
                {
                    GarageName = "Prague Parking",
                    GarageSize = 100,
                    CarSize = 4,
                    CarPricePerHour = 20,
                    McSize = 2,
                    McPricePerHour = 10,
                    SpotSize = 4
                };

                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(fileName, json);
                return config;
            }
        }

        public void EditConfig(ParkingGarage garage) //SelectionPrompt
        {
            GarageConfig config = LoadOrCreate();

            //Skapar en kopia av originalkonfigurationen för att kunna återställa vid avbryt

            bool editing = true;

            while (editing)
            {
                Console.Clear();
                AnsiConsole.MarkupLine("[yellow]Current Garage Configuration:[/]\n");
                AnsiConsole.MarkupLine($"[darkorange3]Garage[/] name: [cyan]{config.GarageName}[/]");
                AnsiConsole.MarkupLine($"[darkorange3]Garage[/] size: [cyan]{config.GarageSize}[/]");
                AnsiConsole.MarkupLine($"[yellow]Car[/] size: [cyan]{config.CarSize}[/]");
                AnsiConsole.MarkupLine($"[yellow]Car[/] price/hour: [cyan]{config.CarPricePerHour}[/]");
                AnsiConsole.MarkupLine($"[yellow]MC[/] size: [cyan]{config.McSize}[/]");
                AnsiConsole.MarkupLine($"[yellow]MC[/] price/hour: [cyan]{config.McPricePerHour}[/]");
                AnsiConsole.MarkupLine($"[orchid]Spot[/] size: [cyan]{config.SpotSize}[/]\n");

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow italic]Select the setting you want to edit:[/]")
                        .PageSize(7)
                        .HighlightStyle("SandyBrown")
                        .AddChoices(new[]
                        {
                            "[darkorange3]Garage[/] name",
                            "[darkorange3]Garage[/] size",
                            "[yellow]Car[/] size",
                            "[yellow]Car[/] price per hour",
                            "[yellow]MC[/] size",
                            "[yellow]MC[/] price per hour",
                            "[orchid]Spot[/] size",
                            "Return without saving",
                            "[italic]Save[/] settings"
                        }));
                switch (choice)
                {
                    case "[darkorange3]Garage[/] name":
                        config.GarageName = AnsiConsole.Ask<string>("Enter new [darkorange3]garage[/] name:");
                        break;
                    case "[darkorange3]Garage[/] size":
                        int newGarageSize = AnsiConsole.Ask<int>("Enter new [darkorange3]garage[/] size:");
                        int occupiedSpots = garage.GetOccupiedSpotsCount();
                        if (newGarageSize < occupiedSpots)
                        {
                            AnsiConsole.MarkupLine($"\n[red]Error:[/] The new garage size [cyan]{newGarageSize}[/] is smaller than the currently occupied size [cyan]{occupiedSpots}[/].");
                            AnsiConsole.MarkupLine("Press any key to continue...");
                            Console.ReadKey();
                        }
                        else
                        {
                            config.GarageSize = newGarageSize;
                        }
                        break;
                    case "[yellow]Car[/] size":
                        config.CarSize = AnsiConsole.Ask<int>("Enter new [yellow]car[/] size:");
                        break;
                    case "[yellow]Car[/] price per hour":
                        config.CarPricePerHour = AnsiConsole.Ask<int>("Enter new [yellow]car[/] price per hour:");
                        break;
                    case "[yellow]MC[/] size":
                        config.McSize = AnsiConsole.Ask<int>("Enter new [yellow]MC[/] size:");
                        break;
                    case "[yellow]MC[/] price per hour":
                        config.McPricePerHour = AnsiConsole.Ask<int>("Enter new [yellow]MC[/] price per hour:");
                        break;
                    case "[orchid]Spot[/] size":
                        config.SpotSize = AnsiConsole.Ask<int>("Enter new [orchid]spot[/] size:");
                        break;
                    case "Return without saving":
                        editing = false;
                        Console.Clear();
                        break;

                    case "[italic]Save[/] settings":
                        //Spara ändringar till fil
                        Console.Clear();
                        AnsiConsole.MarkupLine("\nYour current data will be updated. Are you sure you want to save?");
                        var choiceConfirm = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .PageSize(3)
                                .HighlightStyle("SandyBrown")
                                .AddChoices(new[]
                                {
                                    "[green]Yes, save changes[/]",
                                    "[red]No, return to editing[/]"
                                }));
                        switch (choiceConfirm)
                        {
                            case "[green]Yes, save changes[/]":
                                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                                File.WriteAllText(fileName, json);
                                ApplyConfigChanges(config, garage);
                                AnsiConsole.MarkupLine("[green]Configuration reloaded successfully![/]");
                                AnsiConsole.MarkupLine("\nPress any key to return to the main menu.");
                                Console.ReadKey();
                                editing = false;
                                Console.Clear();
                                break;
                            case "[red]No, return to editing[/]":
                                continue;
                        }
                        break;
                }
            }
        }

        private void ApplyConfigChanges(GarageConfig config, ParkingGarage garage)
        {
            //Justerar antal platser
            if (garage.Garage.Count < config.GarageSize)
            {
                //Lägger till nya platser
                for (int i = garage.Garage.Count + 1; i <= config.GarageSize; i++)
                {
                    garage.Garage.Add(new ParkingSpot(i, config));
                }
            }
            else if (garage.Garage.Count > config.GarageSize)
            {
                //Tar bort tomma platser från slutet av listan
                int spotsToRemove = garage.Garage.Count - config.GarageSize;
                for (int i = 0; i < spotsToRemove; i++)
                {
                    var lastSpot = garage.Garage[garage.Garage.Count - 1];
                    if (lastSpot.ParkedVehicles.Count == 0)
                    {
                        garage.Garage.RemoveAt(garage.Garage.Count - 1);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[yellow]Warning:[/] Could not remove spot {lastSpot.SpotNumber} - it contains vehicles.");
                        break;
                    }
                }
            }

            //Uppdaterar befintliga platser med ny spotSize och ledig storlek
            foreach (var spot in garage.Garage)
            {
                spot.Size = config.SpotSize;

                //Beräknar occupiedSize
                int occupiedSize = 0;
                foreach (var vehicle in spot.ParkedVehicles)
                {
                    occupiedSize += vehicle.Size;
                }

                spot.AvailableSize = spot.Size - occupiedSize;
                if (spot.AvailableSize < 0)
                    spot.AvailableSize = 0;
            }

            garage.Size = config.GarageSize;
        }

        public void PriceList()
        {
            Console.Clear();
            string path = "pricelist.txt";
            StreamReader reader = new StreamReader(path);

            using (reader)
            {
                string line = reader.ReadLine();
                while (line != null)
                {
                    AnsiConsole.MarkupLine($"[yellow]{line}[/]");
                    line = reader.ReadLine();
                }
            }
            AnsiConsole.MarkupLine("\nPress any key to return to the main menu.");
            Console.ReadKey();
            Console.Clear();
        }
    }
}
