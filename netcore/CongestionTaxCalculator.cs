using System;
using System.Collections.Generic;
using System.Linq;

namespace CongestionCalculator
{
    public class CongestionTaxCalculator
    {
        private readonly IDbConnection connection;

        public CongestionTaxCalculator()
        {
            // Create an in-memory SQLite database connection
            connection = new SQLiteConnection("Data Source=:memory:");
            connection.Open();

            // Create tables and insert sample data (you would need to define your schema)
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var cmd = new SQLiteCommand(connection as SQLiteConnection))
            {
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS TollRates (
                        Hour INTEGER PRIMARY KEY,
                        Rate INTEGER
                    );

                    CREATE TABLE IF NOT EXISTS TollFreeVehicles (
                        VehicleType TEXT PRIMARY KEY
                    );

                    -- Insert sample data (customize as needed)
                    INSERT INTO TollRates (Hour, Rate) VALUES
                        (6, 8),
                        (7, 18),
                        (8, 13),
                        (15, 18),
                        (16, 18);

                    INSERT INTO TollFreeVehicles (VehicleType) VALUES
                        ('Motorbike'),
                        ('Tractor'),
                        ('Emergency'),
                        ('Diplomat'),
                        ('Foreign'),
                        ('Military');
                ";

                cmd.ExecuteNonQuery();
            }
        }

        public void Dispose()
        {
            connection.Close();
            connection.Dispose();
        }
        private readonly Dictionary<DayOfWeek, int> FreeDays = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Saturday, 0 },
            { DayOfWeek.Sunday, 0 }
        };

        private readonly Dictionary<int, int> TollRates = new Dictionary<int, int>
        {
            { 6, 8 },
            { 7, 18 },
            { 8, 13 },
            { 15, 18 },
            { 16, 18 }
        };

        private readonly int MaxDailyTax = 60;

        public int GetTax(IVehicle vehicle, DateTime[] dates)
        {
            if (vehicle == null || dates == null || dates.Length == 0)
            {
                return 0;
            }

            int totalFee = 0;

            var groupedDates = dates.GroupBy(date => GetTollFee(date, vehicle));

             foreach (DateTime date in dates)
            {
                int nextFee = GetTollFeeFromDatabase(date, vehicle);
                totalFee += nextFee;
            }

            return Math.Min(totalFee, MaxDailyTax);
        }

        private int GetTollFeeFromDatabase(DateTime date, Vehicle vehicle)
        {
            // Query the database for toll fee based on date, vehicle type, and time
            using (var cmd = new SQLiteCommand(connection as SQLiteConnection))
            {
                cmd.CommandText = @"
                    SELECT MAX(TR.Rate)
                    FROM TollRates TR
                    JOIN TollFreeVehicles TFV ON TR.Hour = strftime('%H', @date, 'localtime')
                    WHERE TFV.VehicleType = @vehicleType;
                ";

                cmd.Parameters.AddWithValue("@date", date);
                cmd.Parameters.AddWithValue("@vehicleType", vehicle.GetVehicleType());

                var result = cmd.ExecuteScalar();

                if (result is int fee)
                {
                    return fee;
                }
            }

            return 0; // Default to 0 if no matching toll fee is found
        }


        private bool IsTollFreeVehicle(IVehicle vehicle)
        {
            if (vehicle == null)
            {
                return false;
            }

            string vehicleType = vehicle.GetVehicleType();
            string[] tollFreeTypes = { "Motorbike", "Tractor", "Emergency", "Diplomat", "Foreign", "Military" };

            return tollFreeTypes.Contains(vehicleType, StringComparer.OrdinalIgnoreCase);
        }

        public int GetTollFee(DateTime date, IVehicle vehicle)
        {
            if (IsTollFreeDate(date) || IsTollFreeVehicle(vehicle))
            {
                return 0;
            }

            int hour = date.Hour;

            if (TollRates.ContainsKey(hour))
            {
                return TollRates[hour];
            }

            return 0;
        }

        private bool IsTollFreeDate(DateTime date)
        {
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;

            if (FreeDays.ContainsKey(date.DayOfWeek))
            {
                return true;
            }

            if (year == 2013 && (
                (month == 1 && day == 1) ||
                (month == 3 && (day == 28 || day == 29)) ||
                (month == 4 && (day == 1 || day == 30)) ||
                (month == 5 && (day == 1 || day == 8 || day == 9)) ||
                (month == 6 && (day == 5 || day == 6 || day == 21)) ||
                (month == 7) ||
                (month == 11 && day == 1) ||
                (month == 12 && (day == 24 || day == 25 || day == 26 || day == 31))
            ))
            {
                return true;
            }

            return false;
        }
    }
}
