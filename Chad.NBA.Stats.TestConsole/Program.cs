using System;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Linq;

using Chad.NBA.Stats;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Chad.NBA.Stats.TestConsole
{
    class Program
    {

        private static String _sqlConnectionString;

        static void Main(string[] args)
        {
            try
            {
                // Read from configuration
                var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                var config = builder.Build();

                _sqlConnectionString = config.GetConnectionString("DefaultConnection");

                DoStuff();

            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void DoStuff()
        {
            using (NBAStatsUpdater updater = new NBAStatsUpdater(_sqlConnectionString))
            {
                updater.UpdateForSeason(SeasonType.Preseason, 2018);
            }
        }
    }
}
