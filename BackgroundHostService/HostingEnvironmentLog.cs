using Hardware.Info;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Udemy1.WebAPI.WritableOptions_Json;

namespace BackgroundHostService
{
    public class HostingEnvironmentLog : IHostedService
    {
        private readonly string fileName = "HostingEnvironment.txt";
        private readonly double gb = 1024 * 1024 * 1024;
        public HostingEnvironmentLog(IWebHostEnvironment webHost,
            IHardwareInfo hardwareInfo,
                IConfiguration configuration,
                    IWritableOptions<AppSettings> options)
        {
            WebHost = webHost;
            HardwareInfo = hardwareInfo;
            Configuration = configuration;
            _options = options;
        }
        public IWebHostEnvironment WebHost { get; }
        public IHardwareInfo HardwareInfo { get; }
        public IConfiguration Configuration { get; }

        private readonly IWritableOptions<AppSettings> _options;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            string isStored = Configuration.
                GetValue<string>("AppSettings:IsHardwareInfoStored");

            if (isStored == "0")
            {
                await SaveToFileAsync($"Date: {DateTime.Now}\n",
                    cancellationToken);

                var tasks = new Task<string>[5];
                tasks[0] = LogCPUInfoAsync();
                tasks[1] = LogMemoryInfoAsync();
                tasks[2] = LogMemoryStatusInfoAsync();
                tasks[3] = LogDriverInfoAsync();
                tasks[4] = LogBIOSInfoAsync();

                for (int i = 0; i < 5; i++)
                {
                    string result = await tasks[i];
                    await SaveToFileAsync(result,cancellationToken);
                }

                // recurring events can be added here
                // PeriodicTimer would be a great choice.

                _options.Update(opt =>
               {
                   opt.IsHardwareInfoStored = "1";
               });
            }
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            string isStored = Configuration.
                GetValue<string>("AppSettings:IsHardwareInfoStored");

            if (isStored == "0")
            {
                await SaveToFileAsync($"Date: {DateTime.Now} System Stopped!\n" +
                        $"-------------------------------------------------------",
                        cancellationToken); 
            }
        }
        private async Task SaveToFileAsync(string message,
            CancellationToken cancellation)
        {
            var path = $@"{WebHost.ContentRootPath}\{fileName}";
            await using var save = new StreamWriter(path, true);
            var stringBuilder = new StringBuilder(message);
            await save.WriteLineAsync(stringBuilder,cancellation);    
        }
        public Task<string> LogCPUInfoAsync()
        {
            HardwareInfo.RefreshCPUList();

            StringBuilder cpuInfo = new();
            foreach (var cpu in HardwareInfo.CpuList)
            {
                cpuInfo.AppendLine($"Caption: {cpu?.Caption}\n" +
                $"Logical Core Count: {cpu?.NumberOfLogicalProcessors}\n" +
                $"Physical Core Count: {cpu?.NumberOfCores}\n" +
                $"CurrentClockSpeed: {cpu?.CurrentClockSpeed}\n" +
                $"L2 Cache Size Per Core (KB): {cpu?.L2CacheSize}\n" +
                $"L3 Cache Size (KB): {cpu?.L3CacheSize}\n" +
                $"Manufacturer: {cpu?.Manufacturer}\n");
            }

            return Task.FromResult(cpuInfo.ToString());
        }
        public Task<string> LogMemoryInfoAsync()
        {
            HardwareInfo.RefreshMemoryList();

            StringBuilder memoryInfo = new();
            foreach (var memo in HardwareInfo.MemoryList)
            {
                double? capacity = memo?.Capacity / (gb);
                memoryInfo.AppendLine($"BankLabel: {memo?.BankLabel}\n" +
                $"Capacity (GB): {capacity:n}\n" +
                $"FormFactor: {memo?.FormFactor}\n" +
                $"Speed (MHz): {memo?.Speed}\n");
            }

            return Task.FromResult(memoryInfo.ToString());
        }
        public Task<string> LogMemoryStatusInfoAsync()
        {
            HardwareInfo.RefreshMemoryStatus();
            MemoryStatus ms = HardwareInfo.MemoryStatus;

            var totalPhysicalMemory = ms?.TotalPhysical / gb;
            var availablePhysicalMemory = ms?.AvailablePhysical / gb;

            var totalVirtualMemory = ms?.TotalVirtual / gb;
            var availableVirtualMemory = ms?.AvailableVirtual / gb;

            var totalPageFile = ms?.TotalPageFile / gb;
            var availablePageFile = ms?.AvailablePageFile / gb;

            var availableExtentedVirtual = 
                ms?.AvailableExtendedVirtual / gb;

            string memoryStatus = $"TotalPhysicalMemory (GB): {totalPhysicalMemory:n}\n" +
            $"AvailablePhysicalMemory (GB): {availablePhysicalMemory:n}\n" +
            $"Total Virtual Memory (GB): {totalVirtualMemory:n}\n" +
            $"Available Virtual Memory (GB): {availableVirtualMemory:n}\n" +
            $"Total Page File (GB): {totalPageFile:n}\n" +
            $"Available Page File (GB): {availablePageFile:n}\n" +
            $"Available Extended Virtual (GB): {availableExtentedVirtual:n}\n";

            return Task.FromResult(memoryStatus);
        }
        public Task<string> LogDriverInfoAsync()
        {
            HardwareInfo.RefreshDriveList();

            StringBuilder driverInfo = new();
            foreach (var drive in HardwareInfo.DriveList)
            {
                driverInfo.AppendLine($"Caption: {drive?.Caption}\n" +
                $"Description: {drive?.Description}\n" +
                $"Model: {drive?.Model}\n" +
                $"Size (GB) {drive?.Size / gb:n}\n");
            }

            return Task.FromResult(driverInfo.ToString());
        }
        public Task<string> LogBIOSInfoAsync()
        {
            HardwareInfo.RefreshBIOSList();

            StringBuilder biosInfo = new();
            foreach (var bios in HardwareInfo.BiosList)
            {
                biosInfo.AppendLine($"Caption: {bios?.Caption}\n" +
                $"Manufacturer: {bios?.Manufacturer}\n" +
                $"Version: {bios?.Version}\n");
            }

            return Task.FromResult(biosInfo.ToString());
        }
    }
}
