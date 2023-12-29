using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
//using Newtonsoft.Json;
#nullable enable  // Add this line



// Interface for checking overlapping schedules
public interface IOverlappable
{
    bool Overlaps(Schedule other);
    bool Overlaps(Reservation other);
}

public class Schedule
  {
    public DateTime PickupDate { get; set; }
    public DateTime DropoffDate { get; set; }
    public decimal TotalPrice { get; set; } // Add this property

    // Implementation of the IOverlappable interface
    public bool Overlaps(Schedule other)
    {
        return !(DropoffDate < other.PickupDate || PickupDate > other.DropoffDate);
    }
}

public class Reservation : IOverlappable
{
    public Schedule Schedule { get; set; } = new Schedule(); // initialize Schedule
    public Driver Driver { get; set; } = new Driver(); // initialize Driver

    public bool Overlaps(Schedule other)
    {
        return Schedule.Overlaps(other);
    }

    public bool Overlaps(Reservation other)
    {
        return Schedule.Overlaps(other.Schedule);
    }
}

// Class representing driver information
public class Driver
{
     public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;

}

// Class representing a vehicle
public class Vehicle : IOverlappable, IComparable<Vehicle>
{
   public string RegistrationNumber { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public decimal DailyRentalPrice { get; set; }
    public List<Reservation> Reservations { get; set; } = new List<Reservation>();


    public bool Overlaps(Schedule other)
    {
        return Reservations.Any(r => r.Overlaps(other));
    }

    public bool Overlaps(Reservation other)
    {
        return Reservations.Any(r => r.Overlaps(other.Schedule));
    }

    // Implementation of the IComparable<Vehicle> interface for ordering by Make
    public int CompareTo(Vehicle? other)
    {
        if (other == null)
        {
            return 1; // If the other object is null, this object is greater
        }

        return string.Compare(Make, other.Make, StringComparison.OrdinalIgnoreCase);
    }
}

// Class representing the Westminster Rental Vehicle system
public class WestminsterRentalVehicle : IRentalManager, IRentalCustomer
{
        private List<Vehicle> vehicles; // Declare the collection without initialization
        private const string DataFilePath = "simpleData.json"; // Change the path as needed

    public WestminsterRentalVehicle()
    {
        // Initialize the vehicles collection
        vehicles = new List<Vehicle>();
        
        // Load data from the file
        LoadData();
    }

    // IRentalManager interface implementation
     public bool AddVehicle(Vehicle v)
{
    // Check if the vehicles collection is null
    if (vehicles is null)
    {
        Console.WriteLine("Error: Vehicles collection is null.");
        return false;
    }

    // Check if a vehicle with the same registration number already exists
    if (!vehicles.Any(vehicle => vehicle.RegistrationNumber == v.RegistrationNumber))
    {
        vehicles.Add(v);
        Console.WriteLine($"Vehicle with registration number {v.RegistrationNumber} added successfully.");
        Console.WriteLine($"Available parking lots: {50 - vehicles.Count}");
               SaveData(); // Save data after adding a vehicle
        return true;
    }
    else
    {
        Console.WriteLine($"Vehicle with registration number {v.RegistrationNumber} already exists.");
        return false;
    }
}

public bool DeleteVehicle(string number)
{
    var vehicleToDelete = vehicles.FirstOrDefault(vehicle => vehicle.RegistrationNumber == number);
    if (vehicleToDelete != null)
    {
        vehicles.Remove(vehicleToDelete);
        Console.WriteLine($"Vehicle {vehicleToDelete.Make} {vehicleToDelete.Model} with registration number {number} deleted.");
        Console.WriteLine($"Available parking lots: {50 - vehicles.Count}");

        // Save data after removing the vehicle
        SaveData();

        return true;
    }
    else
    {
        Console.WriteLine($"Vehicle with registration number {number} not found.");
        return false;
    }
}

public void ListVehicles()
{
    foreach (var vehicle in vehicles)
    {
        Console.WriteLine($"Registration Number: {vehicle.RegistrationNumber}, Make: {vehicle.Make}");

        if (vehicle.Reservations.Count > 0)
        {
            Console.WriteLine("Reservations:");
            foreach (var reservation in vehicle.Reservations.OfType<Reservation>())
            {
                Console.WriteLine($"  Pickup: {reservation.Schedule.PickupDate}, Dropoff: {reservation.Schedule.DropoffDate}, Total Price: {reservation.Schedule.TotalPrice:C}");
                Console.WriteLine($"  Driver: {reservation.Driver.Name} {reservation.Driver.Surname}, DOB: {reservation.Driver.DateOfBirth}, License: {reservation.Driver.LicenseNumber}");
            }
        }
        else
        {
            Console.WriteLine("No reservations for this vehicle.");
        }

        Console.WriteLine();
    }
}


    public void ListOrderedVehicles()
    {
        var orderedVehicles = vehicles.OrderBy(vehicle => vehicle.Make);
        foreach (var vehicle in orderedVehicles)
        {
            Console.WriteLine($"Registration Number: {vehicle.RegistrationNumber}, Make: {vehicle.Make}, Reservations: {vehicle.Reservations.Count}");
        }
    }

public void GenerateReport(string? fileName)
{
    // Check if fileName is null
    if (fileName is null)
    {
        Console.WriteLine("File name cannot be null. Report generation aborted.");
        return;
    }

    // Check if there are any vehicles in the system
    if (vehicles.Count == 0)
    {
        Console.WriteLine("There is no data to generate a report. Add vehicles and reservations first.");
        return; // Exit the method if there is no data
    }

    try
    {
        // Get the desktop path for the current user
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        // Combine the desktop path with the file name and .txt extension to get the full file path for the report
        string reportFilePath = Path.Combine(desktopPath, $"{fileName}.txt");

        // Write the content to the report file
        using (StreamWriter writer = new StreamWriter(reportFilePath))
        {
            foreach (var vehicle in vehicles)
            {
                writer.WriteLine($"Registration Number: {vehicle.RegistrationNumber}, Make: {vehicle.Make}, Model: {vehicle.Model}");
                writer.WriteLine("Bookings:");

                // Check if the vehicle has reservations
                if (vehicle.Reservations.Count > 0)
                {
                    foreach (var reservation in vehicle.Reservations.OfType<Reservation>().OrderBy(r => r.Schedule.PickupDate))
                    {
                        writer.WriteLine($"  Pickup: {reservation.Schedule.PickupDate}, Dropoff: {reservation.Schedule.DropoffDate}, Total Price: {reservation.Schedule.TotalPrice:C}");
                        // Include driver's details
                        writer.WriteLine($"  Driver: {reservation.Driver.Name} {reservation.Driver.Surname}, DOB: {reservation.Driver.DateOfBirth}, License: {reservation.Driver.LicenseNumber}");
                    }
                }
                else
                {
                    writer.WriteLine("  No bookings for this vehicle.");
                }

                writer.WriteLine();
            }
        }

        Console.WriteLine($"Report generated successfully and saved to {reportFilePath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while generating the report: {ex.Message}");
    }
}


public void ListAvailableVehicles(Schedule wantedSchedule, string make) {
{
        // var availableVehicles = vehicles
        // .Where(vehicle => vehicle.Make == make && !vehicle.Overlaps(wantedSchedule))
        // .OrderBy(vehicle => vehicle.DailyRentalPrice);

var availableVehicles = vehicles
    .Where(vehicle => vehicle.Make == make && !vehicle.Reservations.Any(r => r.Overlaps(wantedSchedule)))
    .OrderBy(vehicle => vehicle.DailyRentalPrice);



    if (availableVehicles.Any())
    {
        foreach (var vehicle in availableVehicles)
        {
            Console.WriteLine($"Registration Number: {vehicle.RegistrationNumber}, Make: {vehicle.Make}, Daily Rental Price: {vehicle.DailyRentalPrice:C}");
        }
    }
    else
    {
        Console.WriteLine($"No available {make} vehicles for the specified schedule.");
    }
}
}


public bool AddReservation(string number, Schedule wantedSchedule)
{
    var vehicle = vehicles.FirstOrDefault(v => v.RegistrationNumber == number);

    if (vehicle != null && !vehicle.Overlaps(wantedSchedule))
    {
        var totalPrice = (decimal)(wantedSchedule.DropoffDate - wantedSchedule.PickupDate).TotalDays * vehicle.DailyRentalPrice;
        wantedSchedule.TotalPrice = totalPrice;

        var reservation = new Reservation
        {
            Schedule = wantedSchedule,
            Driver = new Driver() // You might want to add logic to get driver information
        };

        vehicle.Reservations.Add(reservation); // Add the reservation, not just the schedule

        Console.WriteLine($"Reservation for vehicle {vehicle.Make} {vehicle.Model} with registration number {number} made successfully.");
        Console.WriteLine($"Total Price: {totalPrice}");

        SaveData(); // Save data after adding a reservation
        return true;
    }
    else
    {
        Console.WriteLine($"Vehicle with registration number {number} not found or already booked for the specified schedule.");
        return false;
    }
}


public bool ChangeReservation(string number, Schedule oldSchedule, Schedule newSchedule)
{
    var vehicle = vehicles.FirstOrDefault(v => v.RegistrationNumber == number);

    if (vehicle != null)
    {
        var reservation = vehicle.Reservations.FirstOrDefault(r => r.Overlaps(oldSchedule));

        if (reservation != null && !vehicle.Reservations.Any(r => r.Overlaps(newSchedule)))
        {
            reservation.Schedule.PickupDate = newSchedule.PickupDate;
            reservation.Schedule.DropoffDate = newSchedule.DropoffDate;

            // Save data after modifying the reservation
            SaveData();

            Console.WriteLine($"Reservation for vehicle {vehicle.Make} {vehicle.Model} with registration number {number} modified successfully.");
            return true;
        }
    }

    Console.WriteLine($"Unable to modify reservation for vehicle with registration number {number}.");
    return false;
}

public bool DeleteReservation(string number, Schedule schedule)
{
    var vehicle = vehicles.FirstOrDefault(v => v.RegistrationNumber == number);

    if (vehicle != null)
    {
        var reservation = vehicle.Reservations.FirstOrDefault(r => r.Overlaps(schedule));

        if (reservation != null)
        {
            vehicle.Reservations.Remove(reservation);

            // Save data after deleting the reservation
            SaveData();

            Console.WriteLine($"Reservation for vehicle {vehicle.Make} {vehicle.Model} with registration number {number} deleted successfully.");
            return true;
        }
    }

    Console.WriteLine($"Unable to delete reservation for vehicle with registration number {number}.");
    return false;
}

public void SaveData()
{
    try
    {
        // Use JsonSerializer with an option for indented formatting
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string jsonData = System.Text.Json.JsonSerializer.Serialize(vehicles, jsonOptions);

        File.WriteAllText(DataFilePath, jsonData);
        Console.WriteLine($"Data saved successfully to {DataFilePath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error saving data: {ex.Message}");
    }
}

// public void SaveData()
// {
//     try
//     {
//         // Use JsonSerializer with an option for indented formatting
//         var jsonOptions = new JsonSerializerOptions
//         {
//             WriteIndented = true
//         };

//         string jsonData = System.Text.Json.JsonSerializer.Serialize(vehicles, jsonOptions);

//         File.WriteAllText(DataFilePath, jsonData);
//         Console.WriteLine($"Data saved successfully to {DataFilePath}");
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"Error saving data: {ex.Message}");
//     }
// }


// public void SaveData()
// {
//     try
//     {
//         // Use Formatting.Indented for a more readable JSON format
//         string jsonData = JsonConvert.SerializeObject(vehicles, Formatting.Indented);
//         File.WriteAllText(DataFilePath, jsonData);
//         Console.WriteLine($"Data saved successfully to {DataFilePath}");
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"Error saving data: {ex.Message}");
//     }
// }


// public void LoadData()
// {
//     try
//     {
//         if (File.Exists(DataFilePath))
//         {
//             string jsonData = File.ReadAllText(DataFilePath);

//             // Deserialize the data, handle the case where it might be null
//             vehicles = JsonConvert.DeserializeObject<List<Vehicle>>(jsonData) ?? new List<Vehicle>();

//             Console.WriteLine($"Data loaded successfully from {DataFilePath}");
//         }
//         else
//         {
//             // If the file doesn't exist, initialize the vehicles as a new list
//             vehicles = new List<Vehicle>();
//             Console.WriteLine($"Data file not found. Created a new empty data file.");
//         }
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"Error loading data: {ex.Message}");
//     }
// }

// }
public void LoadData()
{
    try
    {
        if (File.Exists(DataFilePath))
        {
            string jsonData = File.ReadAllText(DataFilePath);

            // Deserialize the data, handle the case where it might be null
            vehicles = JsonSerializer.Deserialize<List<Vehicle>>(jsonData) ?? new List<Vehicle>();

            Console.WriteLine($"Data loaded successfully from {DataFilePath}");
        }
        else
        {
            // If the file doesn't exist, initialize the vehicles as a new list
            vehicles = new List<Vehicle>();
            Console.WriteLine($"Data file not found. Created a new empty data file.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error loading data: {ex.Message}");
    }
}
}

// Interface for admin functionalities
public interface IRentalManager
{
    bool AddVehicle(Vehicle v);
    bool DeleteVehicle(string number);
    void ListVehicles();
    void ListOrderedVehicles();
    void GenerateReport(string fileName);
}

// Interface for customer functionalities
public interface IRentalCustomer
{
    void ListAvailableVehicles(Schedule wantedSchedule, string make);
    bool AddReservation(string number, Schedule wantedSchedule);
    bool ChangeReservation(string number, Schedule oldSchedule, Schedule newSchedule);
    bool DeleteReservation(string number, Schedule schedule);
}

class Program
{

    static void Main()
    {
       
         WestminsterRentalVehicle rentalSystem = new WestminsterRentalVehicle();
        rentalSystem.LoadData();

        while (true)
        {
            Console.WriteLine("Select user type:");
            Console.WriteLine("1. Customer");
            Console.WriteLine("2. Admin");
            Console.WriteLine("3. Exit");

            int choice = Convert.ToInt32(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    CustomerMenu(rentalSystem);
                    break;
                case 2:
                    AdminMenu(rentalSystem);
                    break;
                case 3:
                    rentalSystem.SaveData(); // Save data before exiting
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }


    static void CustomerMenu(IRentalCustomer rentalSystem)
    {
        while (true)
        {
            Console.WriteLine("Customer Menu:");
            Console.WriteLine("1. List Available Vehicles");
            Console.WriteLine("2. Add Reservation");
            Console.WriteLine("3. Change Reservation");
            Console.WriteLine("4. Delete Reservation");
           // Console.WriteLine("5. Switch to Admin Menu");
            Console.WriteLine("6. Exit");

            int choice = Convert.ToInt32(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    Console.WriteLine("Enter pickup date (dd/MM/yyyy): ");
                    DateTime pickupDate = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
                    Console.WriteLine("Enter dropoff date (dd/MM/yyyy): ");
                    DateTime dropoffDate = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
                    Schedule wantedSchedule = new Schedule { PickupDate = pickupDate, DropoffDate = dropoffDate };
                    Console.WriteLine("Enter vehicle make: ");
                    string make = Console.ReadLine();
                    rentalSystem.ListAvailableVehicles(wantedSchedule, make);
                    break;
                case 2:
                    Console.WriteLine("Enter vehicle registration number: ");
                    string regNumberAdd = Console.ReadLine();
                    Console.WriteLine("Enter pickup date (dd/MM/yyyy): ");
                    DateTime pickupDateAdd = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
                    Console.WriteLine("Enter dropoff date (dd/MM/yyyy): ");
                    DateTime dropoffDateAdd = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
                    Schedule scheduleAdd = new Schedule { PickupDate = pickupDateAdd, DropoffDate = dropoffDateAdd };
                    rentalSystem.AddReservation(regNumberAdd, scheduleAdd);
                    break;
                case 3:
                    Console.WriteLine("Enter vehicle registration number: ");
                    string regNumberChange = Console.ReadLine();
                    Console.WriteLine("Enter old pickup date (dd/MM/yyyy): ");
                    DateTime oldPickupDate = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
                    Console.WriteLine("Enter old dropoff date (dd/MM/yyyy): ");
                    DateTime oldDropoffDate = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
                    Schedule oldSchedule = new Schedule { PickupDate = oldPickupDate, DropoffDate = oldDropoffDate };
                    Console.WriteLine("Enter new pickup date (dd/MM/yyyy): ");
                    DateTime newPickupDate = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
                    Console.WriteLine("Enter new dropoff date (dd/MM/yyyy): ");
                    DateTime newDropoffDate = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
                    Schedule newSchedule = new Schedule { PickupDate = newPickupDate, DropoffDate = newDropoffDate };
                    rentalSystem.ChangeReservation(regNumberChange, oldSchedule, newSchedule);
                    break;
                case 4:
                    Console.WriteLine("Enter vehicle registration number: ");
                    string regNumberDelete = Console.ReadLine();
                    Console.WriteLine("Enter pickup date (dd/MM/yyyy): ");
                    DateTime pickupDateDelete = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
                    Console.WriteLine("Enter dropoff date (dd/MM/yyyy): ");
                    DateTime dropoffDateDelete = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
                    Schedule scheduleDelete = new Schedule { PickupDate = pickupDateDelete, DropoffDate = dropoffDateDelete };
                    rentalSystem.DeleteReservation(regNumberDelete, scheduleDelete);
                    break;
                // case 5:
                //     return;
                case 5:
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    static void AdminMenu(IRentalManager rentalSystem)
    {
        while (true)
        {
            Console.WriteLine("Admin Menu:");
            Console.WriteLine("1. Add Vehicle");
            Console.WriteLine("2. Delete Vehicle");
            Console.WriteLine("3. List Vehicles");
            Console.WriteLine("4. List Ordered Vehicles");
            Console.WriteLine("5. Generate Report");
            //Console.WriteLine("6. Switch to Customer Menu");
            Console.WriteLine("6. Exit");

            int choice = Convert.ToInt32(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    Vehicle newVehicle = new Vehicle();
                    Console.WriteLine("Enter registration number: ");
                    newVehicle.RegistrationNumber = Console.ReadLine();
                    Console.WriteLine("Enter make: ");
                    newVehicle.Make = Console.ReadLine();
                    Console.WriteLine("Enter model: ");
                    newVehicle.Model = Console.ReadLine();
                    Console.WriteLine("Enter daily rental price: ");
                    newVehicle.DailyRentalPrice = Convert.ToDecimal(Console.ReadLine());
                    rentalSystem.AddVehicle(newVehicle);
                    break;
                case 2:
                    Console.WriteLine("Enter registration number to delete: ");
                    string regNumberDelete = Console.ReadLine();
                    rentalSystem.DeleteVehicle(regNumberDelete);
                    break;
                case 3:
                    rentalSystem.ListVehicles();
                    break;
                case 4:
                    rentalSystem.ListOrderedVehicles();
                    break;
                case 5:
                    Console.WriteLine("Enter file name for the report: ");
                    string fileName = Console.ReadLine();
                    rentalSystem.GenerateReport(fileName);
                    break;
                // case 6:
                //     return;
                case 6:
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

   
}

































// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using Newtonsoft.Json;



// // Interface for checking overlapping schedules
// public interface IOverlappable
// {
//     bool Overlaps(Schedule other);
//     bool Overlaps(Reservation other);
// }

// public class Schedule
//   {
//     public DateTime PickupDate { get; set; }
//     public DateTime DropoffDate { get; set; }
//     public decimal TotalPrice { get; set; } // Add this property

//     // Implementation of the IOverlappable interface
//     public bool Overlaps(Schedule other)
//     {
//         return !(DropoffDate < other.PickupDate || PickupDate > other.DropoffDate);
//     }
// }

// public class Reservation : IOverlappable
// {
//     public Schedule Schedule { get; set; }
//     public Driver Driver { get; set; }

//     public bool Overlaps(Schedule other)
//     {
//         return Schedule.Overlaps(other);
//     }

//     public bool Overlaps(Reservation other)
//     {
//         return Schedule.Overlaps(other.Schedule);
//     }
// }

// // Class representing driver information
// public class Driver
// {
//     public string Name { get; set; }
//     public string Surname { get; set; }
//     public DateTime DateOfBirth { get; set; }
//     public string LicenseNumber { get; set; }
// }

// // Class representing a vehicle
// public class Vehicle : IOverlappable, IComparable<Vehicle>
// {
//     public string RegistrationNumber { get; set; }
//     public string Make { get; set; }
//     public string Model { get; set; }
//     public decimal DailyRentalPrice { get; set; }
//     public List<Reservation> Reservations { get; set; } = new List<Reservation>();

//     public bool Overlaps(Schedule other)
//     {
//         return Reservations.Any(r => r.Overlaps(other));
//     }

//     public bool Overlaps(Reservation other)
//     {
//         return Reservations.Any(r => r.Overlaps(other.Schedule));
//     }

//     // Implementation of the IComparable<Vehicle> interface for ordering by Make
//     public int CompareTo(Vehicle other)
//     {
//         if (other == null)
//         {
//             return 1; // If the other object is null, this object is greater
//         }

//         return string.Compare(Make, other.Make, StringComparison.OrdinalIgnoreCase);
//     }
// }

// // Class representing the Westminster Rental Vehicle system
// public class WestminsterRentalVehicle : IRentalManager, IRentalCustomer
// {
//         private List<Vehicle> vehicles; // Declare the collection without initialization
//         private const string DataFilePath = "simpleData.json"; // Change the path as needed

//     public WestminsterRentalVehicle()
//     {
//         // Initialize the vehicles collection
//         vehicles = new List<Vehicle>();
        
//         // Load data from the file
//         LoadData();
//     }

//     // IRentalManager interface implementation
//      public bool AddVehicle(Vehicle v)
// {
//     // Check if the vehicles collection is null
//     if (vehicles is null)
//     {
//         Console.WriteLine("Error: Vehicles collection is null.");
//         return false;
//     }

//     // Check if a vehicle with the same registration number already exists
//     if (!vehicles.Any(vehicle => vehicle.RegistrationNumber == v.RegistrationNumber))
//     {
//         vehicles.Add(v);
//         Console.WriteLine($"Vehicle with registration number {v.RegistrationNumber} added successfully.");
//         Console.WriteLine($"Available parking lots: {50 - vehicles.Count}");
//                SaveData(); // Save data after adding a vehicle
//         return true;
//     }
//     else
//     {
//         Console.WriteLine($"Vehicle with registration number {v.RegistrationNumber} already exists.");
//         return false;
//     }
// }

// public bool DeleteVehicle(string number)
// {
//     var vehicleToDelete = vehicles.FirstOrDefault(vehicle => vehicle.RegistrationNumber == number);
//     if (vehicleToDelete != null)
//     {
//         vehicles.Remove(vehicleToDelete);
//         Console.WriteLine($"Vehicle {vehicleToDelete.Make} {vehicleToDelete.Model} with registration number {number} deleted.");
//         Console.WriteLine($"Available parking lots: {50 - vehicles.Count}");

//         // Save data after removing the vehicle
//         SaveData();

//         return true;
//     }
//     else
//     {
//         Console.WriteLine($"Vehicle with registration number {number} not found.");
//         return false;
//     }
// }

// public void ListVehicles()
// {
//     foreach (var vehicle in vehicles)
//     {
//         Console.WriteLine($"Registration Number: {vehicle.RegistrationNumber}, Make: {vehicle.Make}");

//         if (vehicle.Reservations.Count > 0)
//         {
//             Console.WriteLine("Reservations:");
//             foreach (var reservation in vehicle.Reservations.OfType<Reservation>())
//             {
//                 Console.WriteLine($"  Pickup: {reservation.Schedule.PickupDate}, Dropoff: {reservation.Schedule.DropoffDate}, Total Price: {reservation.Schedule.TotalPrice:C}");
//                 Console.WriteLine($"  Driver: {reservation.Driver.Name} {reservation.Driver.Surname}, DOB: {reservation.Driver.DateOfBirth}, License: {reservation.Driver.LicenseNumber}");
//             }
//         }
//         else
//         {
//             Console.WriteLine("No reservations for this vehicle.");
//         }

//         Console.WriteLine();
//     }
// }


//     public void ListOrderedVehicles()
//     {
//         var orderedVehicles = vehicles.OrderBy(vehicle => vehicle.Make);
//         foreach (var vehicle in orderedVehicles)
//         {
//             Console.WriteLine($"Registration Number: {vehicle.RegistrationNumber}, Make: {vehicle.Make}, Reservations: {vehicle.Reservations.Count}");
//         }
//     }

// public void GenerateReport(string? fileName)
// {
//     // Check if fileName is null
//     if (fileName is null)
//     {
//         Console.WriteLine("File name cannot be null. Report generation aborted.");
//         return;
//     }

//     // Check if there are any vehicles in the system
//     if (vehicles.Count == 0)
//     {
//         Console.WriteLine("There is no data to generate a report. Add vehicles and reservations first.");
//         return; // Exit the method if there is no data
//     }

//     try
//     {
//         // Get the desktop path for the current user
//         string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

//         // Combine the desktop path with the file name and .txt extension to get the full file path for the report
//         string reportFilePath = Path.Combine(desktopPath, $"{fileName}.txt");

//         // Write the content to the report file
//         using (StreamWriter writer = new StreamWriter(reportFilePath))
//         {
//             foreach (var vehicle in vehicles)
//             {
//                 writer.WriteLine($"Registration Number: {vehicle.RegistrationNumber}, Make: {vehicle.Make}, Model: {vehicle.Model}");
//                 writer.WriteLine("Bookings:");

//                 // Check if the vehicle has reservations
//                 if (vehicle.Reservations.Count > 0)
//                 {
//                     foreach (var reservation in vehicle.Reservations.OfType<Reservation>().OrderBy(r => r.Schedule.PickupDate))
//                     {
//                         writer.WriteLine($"  Pickup: {reservation.Schedule.PickupDate}, Dropoff: {reservation.Schedule.DropoffDate}, Total Price: {reservation.Schedule.TotalPrice:C}");
//                         // Include driver's details
//                         writer.WriteLine($"  Driver: {reservation.Driver.Name} {reservation.Driver.Surname}, DOB: {reservation.Driver.DateOfBirth}, License: {reservation.Driver.LicenseNumber}");
//                     }
//                 }
//                 else
//                 {
//                     writer.WriteLine("  No bookings for this vehicle.");
//                 }

//                 writer.WriteLine();
//             }
//         }

//         Console.WriteLine($"Report generated successfully and saved to {reportFilePath}");
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"An error occurred while generating the report: {ex.Message}");
//     }
// }


// public void ListAvailableVehicles(Schedule wantedSchedule, string make) {
// {
//         // var availableVehicles = vehicles
//         // .Where(vehicle => vehicle.Make == make && !vehicle.Overlaps(wantedSchedule))
//         // .OrderBy(vehicle => vehicle.DailyRentalPrice);

// var availableVehicles = vehicles
//     .Where(vehicle => vehicle.Make == make && !vehicle.Reservations.Any(r => r.Overlaps(wantedSchedule)))
//     .OrderBy(vehicle => vehicle.DailyRentalPrice);



//     if (availableVehicles.Any())
//     {
//         foreach (var vehicle in availableVehicles)
//         {
//             Console.WriteLine($"Registration Number: {vehicle.RegistrationNumber}, Make: {vehicle.Make}, Daily Rental Price: {vehicle.DailyRentalPrice:C}");
//         }
//     }
//     else
//     {
//         Console.WriteLine($"No available {make} vehicles for the specified schedule.");
//     }
// }
// }


// public bool AddReservation(string number, Schedule wantedSchedule)
// {
//     var vehicle = vehicles.FirstOrDefault(v => v.RegistrationNumber == number);

//     if (vehicle != null && !vehicle.Overlaps(wantedSchedule))
//     {
//         var totalPrice = (decimal)(wantedSchedule.DropoffDate - wantedSchedule.PickupDate).TotalDays * vehicle.DailyRentalPrice;
//         wantedSchedule.TotalPrice = totalPrice;

//         var reservation = new Reservation
//         {
//             Schedule = wantedSchedule,
//             Driver = new Driver() // You might want to add logic to get driver information
//         };

//         vehicle.Reservations.Add(reservation); // Add the reservation, not just the schedule

//         Console.WriteLine($"Reservation for vehicle {vehicle.Make} {vehicle.Model} with registration number {number} made successfully.");
//         Console.WriteLine($"Total Price: {totalPrice}");

//         SaveData(); // Save data after adding a reservation
//         return true;
//     }
//     else
//     {
//         Console.WriteLine($"Vehicle with registration number {number} not found or already booked for the specified schedule.");
//         return false;
//     }
// }


// public bool ChangeReservation(string number, Schedule oldSchedule, Schedule newSchedule)
// {
//     var vehicle = vehicles.FirstOrDefault(v => v.RegistrationNumber == number);

//     if (vehicle != null)
//     {
//         var reservation = vehicle.Reservations.FirstOrDefault(r => r.Overlaps(oldSchedule));

//         if (reservation != null && !vehicle.Reservations.Any(r => r.Overlaps(newSchedule)))
//         {
//             reservation.Schedule.PickupDate = newSchedule.PickupDate;
//             reservation.Schedule.DropoffDate = newSchedule.DropoffDate;

//             // Save data after modifying the reservation
//             SaveData();

//             Console.WriteLine($"Reservation for vehicle {vehicle.Make} {vehicle.Model} with registration number {number} modified successfully.");
//             return true;
//         }
//     }

//     Console.WriteLine($"Unable to modify reservation for vehicle with registration number {number}.");
//     return false;
// }

// public bool DeleteReservation(string number, Schedule schedule)
// {
//     var vehicle = vehicles.FirstOrDefault(v => v.RegistrationNumber == number);

//     if (vehicle != null)
//     {
//         var reservation = vehicle.Reservations.FirstOrDefault(r => r.Overlaps(schedule));

//         if (reservation != null)
//         {
//             vehicle.Reservations.Remove(reservation);

//             // Save data after deleting the reservation
//             SaveData();

//             Console.WriteLine($"Reservation for vehicle {vehicle.Make} {vehicle.Model} with registration number {number} deleted successfully.");
//             return true;
//         }
//     }

//     Console.WriteLine($"Unable to delete reservation for vehicle with registration number {number}.");
//     return false;
// }

// public void SaveData()
// {
//     try
//     {
//         // Use Formatting.Indented for a more readable JSON format
//         string jsonData = JsonConvert.SerializeObject(vehicles, Formatting.Indented);
//         File.WriteAllText(DataFilePath, jsonData);
//         Console.WriteLine($"Data saved successfully to {DataFilePath}");
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"Error saving data: {ex.Message}");
//     }
// }


// public void LoadData()
// {
//     try
//     {
//         if (File.Exists(DataFilePath))
//         {
//             string jsonData = File.ReadAllText(DataFilePath);

//             // Deserialize the data, handle the case where it might be null
//             vehicles = JsonConvert.DeserializeObject<List<Vehicle>>(jsonData) ?? new List<Vehicle>();

//             Console.WriteLine($"Data loaded successfully from {DataFilePath}");
//         }
//         else
//         {
//             // If the file doesn't exist, initialize the vehicles as a new list
//             vehicles = new List<Vehicle>();
//             Console.WriteLine($"Data file not found. Created a new empty data file.");
//         }
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"Error loading data: {ex.Message}");
//     }
// }

// }

// // Interface for admin functionalities
// public interface IRentalManager
// {
//     bool AddVehicle(Vehicle v);
//     bool DeleteVehicle(string number);
//     void ListVehicles();
//     void ListOrderedVehicles();
//     void GenerateReport(string fileName);
// }

// // Interface for customer functionalities
// public interface IRentalCustomer
// {
//     void ListAvailableVehicles(Schedule wantedSchedule, string make);
//     bool AddReservation(string number, Schedule wantedSchedule);
//     bool ChangeReservation(string number, Schedule oldSchedule, Schedule newSchedule);
//     bool DeleteReservation(string number, Schedule schedule);
// }

// class Program
// {

//     static void Main()
//     {
       
//          WestminsterRentalVehicle rentalSystem = new WestminsterRentalVehicle();
//         rentalSystem.LoadData();

//         while (true)
//         {
//             Console.WriteLine("Select user type:");
//             Console.WriteLine("1. Customer");
//             Console.WriteLine("2. Admin");
//             Console.WriteLine("3. Exit");

//             int choice = Convert.ToInt32(Console.ReadLine());

//             switch (choice)
//             {
//                 case 1:
//                     CustomerMenu(rentalSystem);
//                     break;
//                 case 2:
//                     AdminMenu(rentalSystem);
//                     break;
//                 case 3:
//                     rentalSystem.SaveData(); // Save data before exiting
//                     Environment.Exit(0);
//                     break;
//                 default:
//                     Console.WriteLine("Invalid choice. Please try again.");
//                     break;
//             }
//         }
//     }


//     static void CustomerMenu(IRentalCustomer rentalSystem)
//     {
//         while (true)
//         {
//             Console.WriteLine("Customer Menu:");
//             Console.WriteLine("1. List Available Vehicles");
//             Console.WriteLine("2. Add Reservation");
//             Console.WriteLine("3. Change Reservation");
//             Console.WriteLine("4. Delete Reservation");
//            // Console.WriteLine("5. Switch to Admin Menu");
//             Console.WriteLine("6. Exit");

//             int choice = Convert.ToInt32(Console.ReadLine());

//             switch (choice)
//             {
//                 case 1:
//                     Console.WriteLine("Enter pickup date (dd/MM/yyyy): ");
//                     DateTime pickupDate = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
//                     Console.WriteLine("Enter dropoff date (dd/MM/yyyy): ");
//                     DateTime dropoffDate = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
//                     Schedule wantedSchedule = new Schedule { PickupDate = pickupDate, DropoffDate = dropoffDate };
//                     Console.WriteLine("Enter vehicle make: ");
//                     string make = Console.ReadLine();
//                     rentalSystem.ListAvailableVehicles(wantedSchedule, make);
//                     break;
//                 case 2:
//                     Console.WriteLine("Enter vehicle registration number: ");
//                     string regNumberAdd = Console.ReadLine();
//                     Console.WriteLine("Enter pickup date (dd/MM/yyyy): ");
//                     DateTime pickupDateAdd = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
//                     Console.WriteLine("Enter dropoff date (dd/MM/yyyy): ");
//                     DateTime dropoffDateAdd = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
//                     Schedule scheduleAdd = new Schedule { PickupDate = pickupDateAdd, DropoffDate = dropoffDateAdd };
//                     rentalSystem.AddReservation(regNumberAdd, scheduleAdd);
//                     break;
//                 case 3:
//                     Console.WriteLine("Enter vehicle registration number: ");
//                     string regNumberChange = Console.ReadLine();
//                     Console.WriteLine("Enter old pickup date (dd/MM/yyyy): ");
//                     DateTime oldPickupDate = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
//                     Console.WriteLine("Enter old dropoff date (dd/MM/yyyy): ");
//                     DateTime oldDropoffDate = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
//                     Schedule oldSchedule = new Schedule { PickupDate = oldPickupDate, DropoffDate = oldDropoffDate };
//                     Console.WriteLine("Enter new pickup date (dd/MM/yyyy): ");
//                     DateTime newPickupDate = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
//                     Console.WriteLine("Enter new dropoff date (dd/MM/yyyy): ");
//                     DateTime newDropoffDate = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
//                     Schedule newSchedule = new Schedule { PickupDate = newPickupDate, DropoffDate = newDropoffDate };
//                     rentalSystem.ChangeReservation(regNumberChange, oldSchedule, newSchedule);
//                     break;
//                 case 4:
//                     Console.WriteLine("Enter vehicle registration number: ");
//                     string regNumberDelete = Console.ReadLine();
//                     Console.WriteLine("Enter pickup date (dd/MM/yyyy): ");
//                     DateTime pickupDateDelete = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
//                     Console.WriteLine("Enter dropoff date (dd/MM/yyyy): ");
//                     DateTime dropoffDateDelete = DateTime.ParseExact(Console.ReadLine(), "dd/MM/yyyy", null);
//                     Schedule scheduleDelete = new Schedule { PickupDate = pickupDateDelete, DropoffDate = dropoffDateDelete };
//                     rentalSystem.DeleteReservation(regNumberDelete, scheduleDelete);
//                     break;
//                 // case 5:
//                 //     return;
//                 case 5:
//                     Environment.Exit(0);
//                     break;
//                 default:
//                     Console.WriteLine("Invalid choice. Please try again.");
//                     break;
//             }
//         }
//     }

//     static void AdminMenu(IRentalManager rentalSystem)
//     {
//         while (true)
//         {
//             Console.WriteLine("Admin Menu:");
//             Console.WriteLine("1. Add Vehicle");
//             Console.WriteLine("2. Delete Vehicle");
//             Console.WriteLine("3. List Vehicles");
//             Console.WriteLine("4. List Ordered Vehicles");
//             Console.WriteLine("5. Generate Report");
//             //Console.WriteLine("6. Switch to Customer Menu");
//             Console.WriteLine("6. Exit");

//             int choice = Convert.ToInt32(Console.ReadLine());

//             switch (choice)
//             {
//                 case 1:
//                     Vehicle newVehicle = new Vehicle();
//                     Console.WriteLine("Enter registration number: ");
//                     newVehicle.RegistrationNumber = Console.ReadLine();
//                     Console.WriteLine("Enter make: ");
//                     newVehicle.Make = Console.ReadLine();
//                     Console.WriteLine("Enter model: ");
//                     newVehicle.Model = Console.ReadLine();
//                     Console.WriteLine("Enter daily rental price: ");
//                     newVehicle.DailyRentalPrice = Convert.ToDecimal(Console.ReadLine());
//                     rentalSystem.AddVehicle(newVehicle);
//                     break;
//                 case 2:
//                     Console.WriteLine("Enter registration number to delete: ");
//                     string regNumberDelete = Console.ReadLine();
//                     rentalSystem.DeleteVehicle(regNumberDelete);
//                     break;
//                 case 3:
//                     rentalSystem.ListVehicles();
//                     break;
//                 case 4:
//                     rentalSystem.ListOrderedVehicles();
//                     break;
//                 case 5:
//                     Console.WriteLine("Enter file name for the report: ");
//                     string fileName = Console.ReadLine();
//                     rentalSystem.GenerateReport(fileName);
//                     break;
//                 // case 6:
//                 //     return;
//                 case 6:
//                     Environment.Exit(0);
//                     break;
//                 default:
//                     Console.WriteLine("Invalid choice. Please try again.");
//                     break;
//             }
//         }
//     }

   
// }