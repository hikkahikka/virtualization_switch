using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualizationButton.Models
{
    public class Logger
    {
        
        public static async Task LogProcess(Process process, string memberName)
        {
            const string path = "log.txt";//vremenno
            if (!File.Exists(path)) File.Create(path).Close();
            using StreamWriter writer = new(path, true);

            string content = memberName+"\n";
            //process.OutputDataReceived += async (s, a) =>
            //{
            //    if (!string.IsNullOrEmpty(a.Data)) content += DateTime.Now.ToString() + "\t" + a.Data + "\n";

            //};
            //process.BeginOutputReadLine();
            //process.WaitForExit();

            content += await process.StandardOutput.ReadToEndAsync();
            await writer.WriteLineAsync(DateTime.Now.ToString() + "\t" + content);
            writer.Flush();
        }
    }
}
