using DataAccess;
using PragueParking2.Classes;
using PragueParking2;
using Spectre.Console;
using System.Data.SqlTypes;

class Program
{
    static void Main(string[] args)
    {
        var storage = new GarageStorage();
        var garage = storage.LoadGarage();
        Menu menu = new Menu();
        ParkingGarage parkingGarage = garage;
        GarageConfig config = new GarageConfig();
        bool programRunning = true;

        while (programRunning)
        {
            string choice = menu.ShowMenu();
            switch (choice)
            {
                case "Park [yellow]vehicles[/]":
                    parkingGarage.ParkVehicleMenu();
                    storage.SaveGarage(garage);
                    break;

                case "Display all [yellow]vehicles[/] in the parking lot":
                    parkingGarage.DisplayParkinglot();
                    break;

                case "Remove or move [yellow]vehicles[/] in the parking lot":
                    parkingGarage.MoveOrRemoveVehicleMenu();
                    storage.SaveGarage(garage);
                    break;

                case "Search for [yellow]vehicles[/]":
                    parkingGarage.SearchVehicle();
                    break;

                case "Price list":
                    config.PriceList();
                    break;
                case "Settings":
                    config.EditConfig(garage);
                    break;

                case "Exit":
                    Console.Clear();
                    storage.SaveGarage(garage);
                    AnsiConsole.MarkupLine("[red]The program has exited.[/]");
                    programRunning = false;
                    break;
            }
        }
    }
}