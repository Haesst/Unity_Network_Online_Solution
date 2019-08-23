using System;
using MySql.Data.MySqlClient;

namespace Unity_Network_Server
{
    class MySQL
    {
        public static MySQLSettings mySQLSettings;

        public static void ConnectToMySQL()
        {
            mySQLSettings.connection = new MySqlConnection(CreateConnectionString());
            ConnectToMySQLServer();
        }

        public static void ConnectToMySQLServer()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("\n[" + string.Format("{0:HH:mm:ss}", DateTime.Now) + "] [System] Connecting to MYSQL Server...");
            Console.ResetColor();
            try
            {
                mySQLSettings.connection.Open();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(" successfull!\n");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(" failed!\n" + ex + "\n");
                Console.ResetColor();
                throw;
            }
        }
        public static void CloseConnection()
        {
            mySQLSettings.connection.Close();
        }

        private static string CreateConnectionString()
        {
            var db = mySQLSettings;
            string connectionString = "SERVER=" + db.server + ";" + "DATABASE=" + db.database + ";" + "UID=" + db.user + ";" + "PASSWORD=" + db.password + ";";
            return connectionString;
        }

        public struct MySQLSettings
        {
            public MySqlConnection connection;
            public string server;
            public string database;
            public string user;
            public string password;
        }

    }
}
