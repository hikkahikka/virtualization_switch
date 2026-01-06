using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualizationButton.Models
{
    public class RegistryManager
    {
        private const string REGISTRY_PATH = @"Software\VirtualizationButton";
        private const string VALUE_NAME = "IsLoggingOn";
        public static bool LoadFromRegistry()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(REGISTRY_PATH))
                {
                    if (key != null)
                    {
                        object value = key.GetValue(VALUE_NAME);
                       return value != null && value.ToString() == "1";
                    }
                    else
                    {
                        return false; 
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static void SaveToRegistry(bool value)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(REGISTRY_PATH))
                {
                    key.SetValue(VALUE_NAME, value ? 1 : 0, RegistryValueKind.DWord);
                }
            }
            catch { }
        }
    }
}
