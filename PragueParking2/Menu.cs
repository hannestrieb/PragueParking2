using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking2
{
    public class Menu
    {
        public string ShowMenu()
        {
            //Meny med valmöjligheter
            //Har jobbat lite ihop med Philip och Noah
            List<string> menuOptions = new List<string>
            {
                "Park [yellow]vehicles[/]",
                "Display all [yellow]vehicles[/] in the parking lot",
                "Remove or move [yellow]vehicles[/] in the parking lot",
                "Search for [yellow]vehicles[/]", "Price list", "Settings",
                "Exit"
            };
            var menuChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow italic]  ╔═════════════════════════════════════╗ \n  ║\t         ╔═══════╗" +
                    "              ║\n  ╠══════════════Main═menu══════════════╣" +
                    "\n  ║\t         ╚═══════╝              ║\n  ╚═════════════════════════════════════╝[/]" +
                    "\n\n[italic SandyBrown]  Use the arrow keys [yellow slowblink]UP[/] and [yellow slowblink]DOWN[/] to navigate the menu\n  " +
                    "Press [yellow slowblink]ENTER[/] or [yellow slowblink]SPACE[/] to select an option[/]")
                    .PageSize(4)
                    .HighlightStyle("SandyBrown")
                    .AddChoices(menuOptions));
            return menuChoice;
        }
    }
}
