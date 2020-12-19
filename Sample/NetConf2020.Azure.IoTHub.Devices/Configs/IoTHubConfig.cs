using System.Text;

namespace NetConf2020.Azure.IoTHub.Devices.Configs
{
    public class IoTHubConfig
    {
        /// <summary>
        /// IoT Hub connection string.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;
        /// <summary>
        /// Proxy address.
        /// </summary>
        public string ProxyAddress { get; set; } = string.Empty;
        /// <summary>
        /// Total retry times when sending message to IoT Hub.
        /// </summary>
        public int DeviceRetryTimes { get; set; } = 0;
        /// <summary>
        /// Whole sending operation time out seconds.
        /// </summary>
        public uint OperationTimeoutSeconds { get; set; } = 240;
        /// <summary>
        /// The wait seconds between two retries.
        /// </summary>
        public double RetryWaitSeconds { get; set; } = 30;
        /// <summary>
        /// The data content encode sent to IoT Hub.
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;
    }
}