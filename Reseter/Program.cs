using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AndroidReseter
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Airplan mode restarter");
            ExecuteCommandWithoutOutput("start-server");
            ExecuteCommandWithoutOutput("shell");
            Task.Delay(TimeSpan.FromSeconds(10.0)).GetAwaiter().GetResult();

            while (true)
            {
                var devices = await GetDeviceIdsAsync();
                var task = new List<Task>();
                foreach (var device in devices)
                {
                    task.Add(DoRestart(device));
                }

                Task.WaitAll(task.ToArray());
                await Task.Delay(TimeSpan.FromSeconds(15));
            }
        }

        private static async Task<bool> CheckIsBlock(string deviceId)
        {
            var spyderFilePath = $"spyder-{deviceId}.txt";
            var commandResult = await ExecuteCommandAsync($"-s {deviceId} pull /storage/emulated/legacy/spyder.txt {spyderFilePath}");

            if (!commandResult.Contains("file pulled"))
                await ExecuteCommandAsync($"-s {deviceId} pull /storage/emulated/0/spyder.txt {spyderFilePath}");

            if (File.Exists(spyderFilePath))
            {
                var rawContent = File.ReadAllText(spyderFilePath);
                Console.WriteLine(rawContent);
                var jsonContent = JObject.Parse(rawContent);
                return jsonContent["IsBlock"].Value<bool>();
            }

            return false;
        }

        private static async Task UpdateSpyderFile(string deviceId)
        {
            var spyderFilePath = $"spyder-{deviceId}.txt";
            var rawContent = File.ReadAllText(spyderFilePath);
            var jsonContent = JObject.Parse(rawContent);
            jsonContent["IsBlock"] = false;
            File.WriteAllText(spyderFilePath, jsonContent.ToString());
            var commandResult = await ExecuteCommandAsync($"-s {deviceId} push {spyderFilePath} /storage/emulated/legacy/spyder.txt");
            if (!commandResult.Contains("file pushed"))
            {
                await ExecuteCommandAsync($"-s {deviceId} push {spyderFilePath} /storage/emulated/0/spyder.txt");
            }
        }

        private static async Task DoRestart(string deviceId)
        {
            if (await CheckIsBlock(deviceId))
            {
                await EnableFlithMode(deviceId);
                await Task.Delay(5000);
                await DisableFlightMode(deviceId);
                await UpdateSpyderFile(deviceId);
            }
        }

        private static async Task EnableFlithMode(string deviceId)
        {
            await ExecuteCommandAsync($"-s {deviceId} shell settings put global airplane_mode_on 1");
            await Task.Delay(1000);
            await ExecuteCommandAsync($"-s {deviceId} shell am broadcast -a android.intent.action.AIRPLANE_MODE");
        }

        private static async Task DisableFlightMode(string deviceId)
        {
            await ExecuteCommandAsync($"-s {deviceId} shell settings put global airplane_mode_on 0");
            await Task.Delay(5000);
            await ExecuteCommandAsync($"-s {deviceId} shell am broadcast -a android.intent.action.AIRPLANE_MODE");
        }

        private static async Task<List<string>> GetDeviceIdsAsync()
        {
            var rawData = await ExecuteCommandAsync("devices");
            var result = new List<string>();
            var devices = rawData.Split(new[] { System.Environment.NewLine }, StringSplitOptions.None);

            return devices.Skip(1).Select(s => (s.Split(new[] { "device" }, StringSplitOptions.None)[0]).Trim()).Where(y => !string.IsNullOrWhiteSpace(y)).ToList();
        }

        private static void Log(string message)
        {
            Console.WriteLine($"{DateTimeOffset.Now:HH:mm:ss}: {message}");
        }

        private static async Task<string> ExecuteCommandAsync(string command)
        {
            Log(command);
            var process = Process.Start(new ProcessStartInfo("adb.exe", command)
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            });

            return await process.StandardOutput.ReadToEndAsync();
        }

        private static void ExecuteCommandWithoutOutput(string command)
        {
            Log(command);
            var process = Process.Start(new ProcessStartInfo("adb.exe", command)
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            });
        }
    }
}
