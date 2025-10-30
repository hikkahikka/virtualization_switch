using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VirtualizationButton.Models
{
    public class CommandManager
    {
        private static void RunCommand(string fileName, string argument)
        {
            string outputLanguageSettingsPowershell = "[Threading.Thread]::CurrentThread.CurrentCulture = 'en-US'; [Threading.Thread]::CurrentThread.CurrentUICulture = 'en-US'; ";
            string outputLanguageSettingsCmd = "chcp 65001 &&";
            string fullArgument = (fileName == "cmd" ? "/c "+outputLanguageSettingsCmd : "/command "+outputLanguageSettingsPowershell)+argument;
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                Verb = "runas",
                LoadUserProfile = true,
                FileName = fileName,
                Arguments = fullArgument,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process? process = Process.Start(processInfo);
            Logger.LogProcess(process);
        }
        public static void EnableVirtualization()
        {
            CommandManager.RunCommand("powershell", "Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V –All -NoRestart");
            CommandManager.RunCommand("cmd", "bcdedit /set hypervisorlaunchtype auto");
        }
        public static void DisableVirtualization()
        {
            CommandManager.RunCommand("powershell", "Disable-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V -NoRestart");
            CommandManager.RunCommand("cmd", "bcdedit /set hypervisorlaunchtype off");

        }
        public static void Reboot()=>CommandManager.RunCommand("cmd", "/c shutdown -r -t 3");
        public static Dictionary<string, bool?> GetVirtualizationStatus()
        {
            Process process = Process.Start(new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = "Get-ComputerInfo -Property \"Hyper*\"",
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            Dictionary<string, bool?> values = new Dictionary<string, bool?>();
            foreach (var s in process.StandardOutput.ReadToEnd().Trim().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var buff = s.Split(new char[] { ':' });
                if (bool.TryParse(buff[1], out bool value)) values.Add(buff[0].Trim(), value);
                else values.Add(buff[0].Trim(), null);
            }
            return values;
        }
    }
}
