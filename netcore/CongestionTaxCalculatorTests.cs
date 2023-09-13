using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CongestionCalculator.Tests
{
    [TestClass]
    public class CongestionTaxCalculatorTests
    {
        private CongestionTaxCalculator calculator;

        [TestInitialize]
        public void Setup()
        {
            // Initialize the calculator with an in-memory database for testing
            calculator = new CongestionTaxCalculator();
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Dispose of the calculator and close the database connection
            calculator.Dispose();
        }

        [TestMethod]
        public void CalculateTax_WithValidDatesAndVehicle_ReturnsCorrectTax()
        {
            // Arrange
            var vehicle = new Car(); // Assuming you have a Car class
            var dates = new[]
            {
                new DateTime(2013, 2, 7, 6, 0, 0),
                new DateTime(2013, 2, 7, 7, 30, 0),
                new DateTime(2013, 2, 7, 16, 0, 0)
            };

            // Act
            int tax = calculator.GetTax(vehicle, dates);

            // Assert
            Assert.AreEqual(29, tax); // Adjust the expected tax based on your test scenario
        }

        [TestMethod]
        public void CalculateTax_WithNullVehicle_ReturnsZeroTax()
        {
            // Arrange
            Vehicle vehicle = null;
            var dates = new[]
            {
                new DateTime(2013, 2, 7, 6, 0, 0),
                new DateTime(2013, 2, 7, 7, 30, 0),
                new DateTime(2013, 2, 7, 16, 0, 0)
            };

            // Act
            int tax = calculator.GetTax(vehicle, dates);

            // Assert
            Assert.AreEqual(0, tax);
        }

        // You can write more test methods to cover other scenarios

        [TestMethod]
        public void CalculateTax_WithInvalidDates_ReturnsZeroTax()
        {
            // Arrange
            var vehicle = new Car();
            var dates = new DateTime[] { }; // Empty date array

            // Act
            int tax = calculator.GetTax(vehicle, dates);

            // Assert
            Assert.AreEqual(0, tax);
        }
    }
}
