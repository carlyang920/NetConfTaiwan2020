using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetConf2020.Azure.IoTHub.Devices.Interfaces
{
    public interface IIotHubService
    {
        Task SendEventAsync<T>(List<T> dataList) where T : class;
        Task SendEventAsync(string jsonArray);
    }
}
