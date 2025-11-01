using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PragueParking2.Classes
{
    public class Car : Vehicle
    {
        public Car(string regNumber, GarageConfig config) : base(regNumber)
        {
            Size = config.CarSize;
            PricePerHour = config.CarPricePerHour;
            Type = VehicleType.Car;
        }
        //Konstruktor för enhetstester
        public Car(string regNumber, int size, int pricePerHour) : base(regNumber)
        {
            Size = size;
            PricePerHour = pricePerHour;
            Type = VehicleType.Car;
        }
    }
}