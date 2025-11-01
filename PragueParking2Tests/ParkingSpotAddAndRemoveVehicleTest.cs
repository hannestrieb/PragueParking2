using DataAccess;
using PragueParking2.Classes;
namespace PragueParking2Tests
{
    [TestClass]
    public sealed class ParkingSpotAddAndRemoveVehicleTest
    {
        [TestMethod]
        public void AddVehicle_ReducesAvailableSize_WithoutConfig()
        {
            //Utan config med hjälp av konstruktor för enhetstester

            //Skapa en parkeringsplats med size 4
            var spot = new ParkingSpot(spotNumber: 1, spotSize: 4);

            //Skapa en bil med size 4 och price per hour 20
            var car = new Car("TEST123", 4, 20);

            //Parkera bilen på parkeringsplatsen
            spot.AddVehicle(car);

            //Tillgänglig storlek ska vara 0 efter parkering
            Assert.AreEqual(0, spot.AvailableSize); 
            Assert.IsTrue(spot.ParkedVehicles.Contains(car));
        }

        [TestMethod]
        public void AddVehicle_ReducesAvailableSize_WithConfig()
        {
            //Skapar config med värden
            var config = new GarageConfig
            {
                SpotSize = 4,
                CarSize = 4,
                CarPricePerHour = 20
            };

            //Skapar parkeringsplats med config
            var spot = new ParkingSpot(spotNumber: 1, config);

            //Skapar en bil med config
            var car = new Car("TEST123", config);

            //Parkerar bilen på parkeringsplatsen
            spot.AddVehicle(car);

            //Tillgänglig storlek ska vara 0 efter parkering
            Assert.AreEqual(0, spot.AvailableSize);
            Assert.IsTrue(spot.ParkedVehicles.Contains(car));
        }

        [TestMethod]
        public void RemoveVehicle_ExistingVehicle_RemovesFromList()
        {
            //Skapar config med värden
            var config = new GarageConfig
            {
                SpotSize = 4,
                CarSize = 4,
                CarPricePerHour = 20
            };
            //Skapar parkeringsplats och bil med config
            var spot = new ParkingSpot(spotNumber: 1, config);
            var car = new Car("ABC123", config);

            //Parkerar bilen på parkeringsplatsen
            spot.AddVehicle(car);

            Assert.AreEqual(1, spot.ParkedVehicles.Count);
            Assert.AreEqual(0, spot.AvailableSize);

            //Tar bort bilen från parkeringsplatsen
            spot.RemoveVehicle(car);

            Assert.AreEqual(0, spot.ParkedVehicles.Count);
            Assert.IsFalse(spot.ParkedVehicles.Contains(car));
            Assert.AreEqual(4, spot.AvailableSize);
        }
    }
}
