using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using NetConf2020.Azure.IoTHub.Devices.Configs;
using NetConf2020.Azure.IoTHub.Devices.Interfaces;
using NetConf2020.Azure.IoTHub.Devices.RetryPolicies;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetConf2020.Azure.IoTHub.Devices.Services
{
    public class IotHubService : IIotHubService
    {
        private readonly DeviceClient _deviceClient;
        private readonly IoTHubConfig _ioTHubConfig;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="ioTHubConfig">IoT Hub Configuration</param>
        /// <param name="onRetryError">The delegating action for each retry error</param>
        public IotHubService(
            IoTHubConfig ioTHubConfig,
            Action<int, Exception> onRetryError = null
            )
        {
            if (string.IsNullOrEmpty(ioTHubConfig.ConnectionString))
            {
                throw new ArgumentNullException($"[{nameof(IotHubService)}] {nameof(IotHubService)}(): Connection String is empty");
            }

            _ioTHubConfig = ioTHubConfig;

            _deviceClient =
                DeviceClient.CreateFromConnectionString(
                    _ioTHubConfig.ConnectionString,
                    InitTransportSettings()
                );

            //The time out milliseconds for whole operation, default is 240000
            _deviceClient.OperationTimeoutInMilliseconds = _ioTHubConfig.OperationTimeoutSeconds * 1000;

            if (0 < _ioTHubConfig.DeviceRetryTimes)
            {
                var retryPolicy = new CustomRetryPolicy(
                    _ioTHubConfig.DeviceRetryTimes,
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(_ioTHubConfig.RetryWaitSeconds),
                    TimeSpan.FromSeconds(1)
                )
                {
                    OnRetryError = onRetryError
                };

                _deviceClient.SetRetryPolicy(retryPolicy);
            }
            else
            {
                _deviceClient.SetRetryPolicy(new NoRetry());
            }
        }

        #region private functions

        private ITransportSettings[] InitTransportSettings()
        {
            return new ITransportSettings[]
            {
                string.IsNullOrEmpty(_ioTHubConfig.ProxyAddress)
                    ? new Http1TransportSettings()
                    : new Http1TransportSettings{ Proxy = new WebProxy(_ioTHubConfig.ProxyAddress) }
            };
        }

        #endregion

        #region public functions

        public async Task SendEventAsync<T>(List<T> dataList) where T :class
        {
            if (null == dataList)
                dataList = new List<T>();

            await SendEventAsync(JsonConvert.SerializeObject(dataList, Formatting.None));
        }

        /// <summary>
            /// 非同步傳送事件訊息
            /// </summary>
            /// <param name="jsonArray">JSON List</param>
            /// <param name="customProperties">自訂義屬性，會加入該批資料的Hub自訂義屬性中。</param>
            /// <returns></returns>
            public async Task SendEventAsync(string jsonArray)
        {
            var inserted = new List<JObject>();

            try
            {
                var dataList = JsonConvert.DeserializeObject<List<JObject>>(jsonArray);

                var dividedList = new List<JObject>();
                var batchList = new List<List<JObject>>();
                var byteCount = 0;

                //Divided send data list
                dataList.ForEach(p =>
                {
                    var data = _ioTHubConfig.Encoding.GetBytes(p.ToString());

                    //less than 256 KB
                    if (262144 >= (byteCount + data.Length))
                    {
                        byteCount += data.Length;
                        dividedList.Add(JsonConvert.DeserializeObject<JObject>(p.ToString()));
                    }
                    else
                    {
                        batchList.Add(dividedList.ToList());
                        dividedList.Clear();
                        dividedList.Add(JsonConvert.DeserializeObject<JObject>(p.ToString()));

                        byteCount = data.Length;
                    }
                });

                //rest of data
                if (0 < dividedList.Count)
                {
                    batchList.Add(dividedList.ToList());
                    dividedList.Clear();
                }

                foreach (var send in batchList)
                {
                    var message = new Message(
                        _ioTHubConfig.Encoding.GetBytes(JsonConvert.SerializeObject(send))
                        );

                    await _deviceClient.SendEventAsync(message);

                    inserted.AddRange(send);
                }

                batchList.Clear();
            }
            catch (Exception e)
            {
                if (0 < inserted.Count)
                    e.Data.Add("Inserted", JsonConvert.SerializeObject(inserted));

                throw;
            }
        }

        #endregion
    }
}
