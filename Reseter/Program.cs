using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AndroidReseter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Airplan mode restarter");
            Process.Start(new ProcessStartInfo("adb.exe", "shell")
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            });
            Task.Delay(TimeSpan.FromSeconds(10.0)).GetAwaiter().GetResult();
            while (true)
            {
                Program.Enable();
                TaskAwaiter awaiter = Task.Delay(TimeSpan.FromSeconds(5.0)).GetAwaiter();
                awaiter.GetResult();
                Program.Disable();
                awaiter = Task.Delay(TimeSpan.FromSeconds(30.0)).GetAwaiter();
                awaiter.GetResult();
            }
        }

        private static void Enable()
        {
            Process.Start(new ProcessStartInfo("adb.exe", "shell settings put global airplane_mode_on 1")
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            });
            Task.Delay(1000).GetAwaiter().GetResult();
            Process.Start(new ProcessStartInfo("adb.exe", "shell am broadcast -a android.intent.action.AIRPLANE_MODE")
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            });
        }

        private static void Disable()
        {
            Process.Start(new ProcessStartInfo("adb.exe", "shell settings put global airplane_mode_on 0")
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            });
            Task.Delay(5000).GetAwaiter().GetResult();
            Process.Start(new ProcessStartInfo("adb.exe", "shell am broadcast -a android.intent.action.AIRPLANE_MODE")
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            });
        }
    }
}
