// In this example, we first set the connection string that specifies the database server, database name, username, and password. 
// We then create a new SqlConnection object and pass in the connection string. We open the connection using the Open() method.

// Next, we create a new SqlCommand object with the SQL query to execute, which in this case is a simple SELECT statement that retrieves data from the Customers table. 
// We execute the query using the ExecuteReader() method, which returns a SqlDataReader object that we can use to read the data.

// Finally, we loop through the rows in the SqlDataReader object and display the data using Console.WriteLine(). 
// We handle any errors using a try-catch block. Once we're done, we close the connection using the using statement, 
// which automatically disposes of the SqlConnection object and releases any resources used by the object.

using System;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        // Set the connection string
        string connectionString = "Data Source=myServer;Initial Catalog=myDatabase;User ID=myUsername;Password=myPassword";

        // Create a new SqlConnection object
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                // Open the connection
                connection.Open();

                // Create a new SqlCommand object
                using (SqlCommand command = new SqlCommand("SELECT * FROM Customers", connection))
                {
                    // Execute the query and get a DataReader object
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Loop through the rows in the DataReader and display the data
                        while (reader.Read())
                        {
                            Console.WriteLine("CustomerID: {0}", reader["CustomerID"]);
                            Console.WriteLine("CompanyName: {0}", reader["CompanyName"]);
                            Console.WriteLine("ContactName: {0}", reader["ContactName"]);
                            Console.WriteLine();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                Console.WriteLine(ex.Message);
            }
        }
    }
}
