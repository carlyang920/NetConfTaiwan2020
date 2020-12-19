using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kingston.IoTHub.Devices.Client.Core.Interfaces
{
    public interface IIotHubService
    {
        Task SendEventAsync<T>(List<T> dataList) where T : class;
        Task SendEventAsync(string jsonArray);
    }
}
