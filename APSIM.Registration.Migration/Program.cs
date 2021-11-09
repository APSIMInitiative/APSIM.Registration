using System;

namespace APSIM.Registration.Migration
{
    class Program
    {
        private const string oldConnectionStringVar = "OLD_CONN_STRING";
        private const string newConnectionStringVar = "NEW_CONN_STRING";

        static void Main(string[] args)
        {
            string oldConnectionString = Environment.GetEnvironmentVariable(oldConnectionStringVar);
            if (string.IsNullOrEmpty(oldConnectionString))
                throw new Exception($"Old connection string not set");

            string newConnectionString = Environment.GetEnvironmentVariable(newConnectionStringVar);
            if (string.IsNullOrEmpty(newConnectionString))
                throw new Exception($"New connection string not set");
            Migrator migrator = new Migrator(oldConnectionString, newConnectionString);
            ushort num = args.Length > 0 ? ushort.Parse(args[0]) : (ushort)0;
            migrator.Migrate(num);
            Console.WriteLine("Migration Successful.");
        }
    }
}
