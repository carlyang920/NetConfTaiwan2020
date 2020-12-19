using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetConf2020.Azure.IoTHub.Devices.Configs;
using NetConf2020.Azure.IoTHub.Devices.Interfaces;
using NetConf2020.Azure.IoTHub.Devices.Services;
using Newtonsoft.Json;

namespace NetConf2020.Azure.IoTHub.Devices
{
    public class App
    {
        private readonly IIotHubService _service;

        public App()
        {
            const string iotHubConnectionString = @"Enter your IoT Hub connection";

            _service = new IotHubService(
                new IoTHubConfig()
                {
                    ConnectionString = iotHubConnectionString,
                    DeviceRetryTimes = 10,
                    OperationTimeoutSeconds = 110,
                    RetryWaitSeconds = 10,
                    Encoding = Encoding.UTF8
                },
                OnRetryError
            );

            void OnRetryError(int retryCount, Exception e)
            {
                Console.WriteLine(string.Empty);
                Console.WriteLine($"CurrentTime: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                Console.WriteLine($"RetryCount: {retryCount}");
                Console.WriteLine($"Exception: {e}");
            }
        }

        private async Task SendEventAsync<T>(List<T> dataList) where T : class
        {
            try
            {
                await _service.SendEventAsync(dataList);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Run()
        {
            var random = new Random();

            do
            {
                var currentTime = DateTime.Now;

                var dataList = new List<dynamic>
                {
                    new { HappenTime = currentTime, O2 = random.Next(5000, 9000) }
                };

                Console.WriteLine(JsonConvert.SerializeObject(dataList, Formatting.None));

                SendEventAsync(dataList)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                Thread.Sleep(10000);
            } while (true);
        }
    }
}
