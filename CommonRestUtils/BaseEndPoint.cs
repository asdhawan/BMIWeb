using RestSharp;
using System.Threading.Tasks;

namespace CommonRestUtils {
    public class BaseEndPoint {
        protected RestClient RestClient { get; set; }

        public BaseEndPoint(ref RestClient restClient) {
            RestClient = restClient;
        }

        protected virtual RestRequest NewRestRequest(string resource, Method method = Method.GET) {
            return RestUtils.NewRestRequest(resource, method);
        }

        protected virtual void ExecuteRequest(RestRequest oRequest, RestUtils.RestResponseHandler restResponseHandler = null) {
            if (RestClient != null) {
                RestClient.ExecuteRequest(oRequest, restResponseHandler);
            }
        }

        protected virtual T ExecuteRequest<T>(RestRequest oRequest, RestUtils.RestResponseHandler<T> restResponseHandler = null) where T : new() {
            T retVal = default(T);
            if (RestClient != null) {
                retVal = RestClient.ExecuteRequest<T>(oRequest, restResponseHandler);
            }
            return retVal;
        }

        protected virtual async Task ExecuteRequestAsync(RestRequest oRequest, RestUtils.RestResponseHandler restResponseHandler = null) {
            if (RestClient != null) {
                await RestClient.ExecuteRequestAsync(oRequest, restResponseHandler);
            }
        }

        protected virtual async Task<T> ExecuteRequestAsync<T>(RestRequest oRequest, RestUtils.RestResponseHandler<T> restResponseHandler = null) where T : new() {
            T retVal = default(T);
            if (RestClient != null) {
                retVal = await RestClient.ExecuteRequestAsync<T>(oRequest, restResponseHandler);
            }
            return retVal;
        }
    }
}
