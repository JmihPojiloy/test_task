using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TestWPF
{

    class ConfigLoad
    {
        private const string ConfigFilePath = "config.txt";
        public string? serverAdress;
        public string? portAdress;

        public string ServerAdress => serverAdress;
        public string PortAdress => portAdress;

        public ConfigLoad() 
        {
            ReadConfigFile();
        }

        private void ReadConfigFile()
        {
            try
            {
                var lines = File.ReadAllLines(ConfigFilePath);
                foreach (var line in lines)
                {
                    if (line.StartsWith("Server address:", StringComparison.OrdinalIgnoreCase))
                    {
                        var address = line.Substring("Server address:".Length).Trim();
                        // Set the server address in the UI
                        serverAdress = address;
                    }
                    else if (line.StartsWith("Port:", StringComparison.OrdinalIgnoreCase))
                    {
                        var port = line.Substring("Port:".Length).Trim();
                        // Set the port in the UI
                        portAdress = port;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading config file: {ex.Message}", "Config Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
