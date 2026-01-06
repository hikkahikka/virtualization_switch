using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VirtualizationButton.Models
{
    public class Logger
    {
        public static bool IsRunning = false;
        public static async Task LogProcess(Process process, string memberName)
        {
            try
            {
                Logger.IsRunning = true;
                const string path = "log.txt";
                if (!File.Exists(path)) File.Create(path).Close();
                using StreamWriter writer = new(path, true);
                string content = memberName + "\n";
                content += await process.StandardOutput.ReadToEndAsync();
                await writer.WriteLineAsync(DateTime.Now.ToString() + "\t" + content);
                writer.Flush();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Logger error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Logger.IsRunning = false;

        }
    }
}
