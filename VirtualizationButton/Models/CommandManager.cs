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
        private static async Task RunCommand(string fileName, string argument, bool isLog , [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
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


            if(isLog) await Logger.LogProcess(process, memberName);
        }
        public static async void EnableVirtualization(bool isLog)
        {
            await CommandManager.RunCommand("powershell", "Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V –All -NoRestart", isLog);
            await CommandManager.RunCommand("cmd", "bcdedit /set hypervisorlaunchtype auto", isLog);
        }
        public static async void DisableVirtualization(bool isLog)
        {
            await CommandManager.RunCommand("cmd", "bcdedit /set hypervisorlaunchtype off", isLog);
            await CommandManager.RunCommand("powershell", "Disable-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V -NoRestart", isLog);
        }
        public static async void Reboot()=> await CommandManager.RunCommand("cmd", "shutdown -r -t 3", false);
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
