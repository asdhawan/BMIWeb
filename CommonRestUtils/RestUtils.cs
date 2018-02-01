using RestSharp;
using System;
using System.Threading.Tasks;

namespace CommonRestUtils {
    public static class RestUtils {
        public class RestException : Exception {
            public IRestRequest Request { get; set; }
            public IRestResponse Response { get; set; }
            public RestException(string message, IRestRequest request, IRestResponse response) : base(message) {
                Request = request;
                Response = response;
            }
        }

        public delegate void RestResponseHandler(IRestRequest oRequest, IRestResponse oResponse);
        public delegate void RestResponseHandler<T>(IRestRequest oRequest, IRestResponse<T> oResponse);
        public delegate void RestExceptionHandler(RestException exception);

        public static RestRequest NewRestRequest(string resource, Method method = Method.GET) {
            RestRequest oRequest = new RestRequest(resource, method);
            oRequest.JsonSerializer = new NewtonsoftJsonSerializer();
            oRequest.RequestFormat = DataFormat.Json;
            return oRequest;
        }

        public static void ExecuteRequest(this RestClient client, RestRequest oRequest, RestResponseHandler restResponseHandler = null) {
            if (client != null) {
                var oResponse = client.Execute(oRequest);
                HandleRestResponse(oRequest, oResponse, restResponseHandler);
            }
        }

        public static async Task ExecuteRequestAsync(this RestClient client, RestRequest oRequest, RestResponseHandler restResponseHandler = null) {
            if (client != null) {
                var oResponse = await client.ExecuteTaskAsync(oRequest);
                HandleRestResponse(oRequest, oResponse, restResponseHandler);
            }
        }

        public static T ExecuteRequest<T>(this RestClient client, RestRequest oRequest, RestResponseHandler<T> restResponseHandler = null) where T : new() {
            T retVal = default(T);
            if (client != null) {
                var oResponse = client.Execute<T>(oRequest);
                HandleRestResponse(oRequest, oResponse, restResponseHandler);
                retVal = oResponse.Data;
            }
            return retVal;
        }

        public static async Task<T> ExecuteRequestAsync<T>(this RestClient client, RestRequest oRequest, RestResponseHandler<T> restResponseHandler = null) where T : new() {
            T retVal = default(T);
            if (client != null) {
                var oResponse = await client.ExecuteTaskAsync<T>(oRequest);
                HandleRestResponse(oRequest, oResponse, restResponseHandler);
                retVal = oResponse.Data;
            }
            return retVal;
        }

        private static void HandleRestResponse(IRestRequest oRequest, IRestResponse oResponse, RestResponseHandler restResponseHandler = null) {
            if (restResponseHandler != null) {
                restResponseHandler.Invoke(oRequest, oResponse);
            } else {
                try {
                    HandleRestResponse(oResponse);
                } catch (Exception ex) {
                    throw new RestException(ex.Message, oRequest, oResponse);
                }
            }
        }

        private static void HandleRestResponse<T>(IRestRequest oRequest, IRestResponse<T> oResponse, RestResponseHandler<T> restResponseHandler = null) {
            if (restResponseHandler != null) {
                restResponseHandler.Invoke(oRequest, oResponse);
            } else {
                try {
                    HandleRestResponse(oResponse);
                } catch (Exception ex) {
                    throw new RestException(ex.Message, oRequest, oResponse);
                }
            }
        }

        public static void HandleRestResponse(IRestResponse oResponse) {
            if (oResponse.ErrorException != null)
                throw oResponse.ErrorException;

            switch (oResponse.ResponseStatus) {
                case ResponseStatus.Error:
                case ResponseStatus.TimedOut:
                    throw new Exception(oResponse.ErrorMessage);
            }

            //Final error check
            ProcessStatusCode(oResponse);
        }

        public static void ProcessStatusCode(IRestResponse oResponse) {
            //If the response code is >= 400 then we throw an error
            if ((int)oResponse.StatusCode >= 400) {
                string sErrorMessage = string.Format("Response Code:{0} {1}, {2}", oResponse.StatusCode, oResponse.StatusDescription, oResponse.Content);
                throw new Exception(sErrorMessage);
            }
        }

        public static void ExecuteRequest(Action executeRequestDelegate, RestExceptionHandler restExceptionHandler) {
            try {
                executeRequestDelegate.Invoke();
            } catch (Exception ex) {
                if (ex is RestException && restExceptionHandler != null)
                    restExceptionHandler.Invoke((RestException)ex);
                else
                    throw ex;
            }
        }

        public static async void ExecuteRequestAsync(Func<Task> executeRequestDelegate, RestExceptionHandler restExceptionHandler) {
            try {
                await executeRequestDelegate.Invoke();
            } catch (Exception ex) {
                if (ex is RestException && restExceptionHandler != null)
                    restExceptionHandler.Invoke((RestException)ex);
                else
                    throw ex;
            }
        }

        public static T ExecuteRequest<T>(Func<T> executeRequestDelegate, RestExceptionHandler restExceptionHandler) where T : new() {
            T retVal = default(T);
            try {
                retVal = executeRequestDelegate.Invoke();
            } catch (Exception ex) {
                if (ex is RestException && restExceptionHandler != null)
                    restExceptionHandler.Invoke((RestException)ex);
                else
                    throw ex;
            }
            return retVal;
        }

        public static async Task<T> ExecuteRequestAsync<T>(Func<Task<T>> executeRequestDelegate, RestExceptionHandler restExceptionHandler) where T : new() {
            T retVal = default(T);
            try {
                retVal = await executeRequestDelegate.Invoke();
            } catch (Exception ex) {
                if (ex is RestException && restExceptionHandler != null)
                    restExceptionHandler.Invoke((RestException)ex);
                else
                    throw ex;
            }
            return retVal;
        }
    }
}
