using System;
using Microsoft.Azure.Devices.Client;

namespace Kingston.IoTHub.Devices.Client.Core.RetryPolicies
{
    public class CustomRetryPolicy : IRetryPolicy
    {
        private readonly ExponentialBackoff _exponentialBackoff;

        public Action<int, Exception> OnRetryError;

        /// <summary>
        /// Creates an instance of ExponentialBackoff.
        /// </summary>
        /// <param name="retryTimes">The maximum number of retry attempts.</param>
        /// <param name="minBackoff">The minimum back-off time</param>
        /// <param name="maxBackoff">The maximum back-off time.</param>
        /// <param name="deltaBackoff">The value that will be used to calculate a random delta in the exponential delay between retries.</param>
        public CustomRetryPolicy(int retryTimes, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
        {
            _exponentialBackoff = new ExponentialBackoff(retryTimes, minBackoff, maxBackoff, deltaBackoff);
        }

        /// <summary>
        /// Returns true if, based on the parameters the operation should be retried.
        /// </summary>
        /// <param name="currentRetryCount">How many times the operation has been retried.</param>
        /// <param name="lastException">Operation exception.</param>
        /// <param name="retryInterval">Next retry should be performed after this time interval.</param>
        /// <returns>True if the operation should be retried, false otherwise.</returns>
        public bool ShouldRetry(int currentRetryCount, Exception lastException, out TimeSpan retryInterval)
        {
            OnRetryError?.Invoke(currentRetryCount, lastException);

            return _exponentialBackoff.ShouldRetry(currentRetryCount, lastException, out retryInterval);
        }
    }
}
