using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BMIWebUI.Controllers {
    public class BaseApiController: ApiController {

        protected HttpResponseMessage ExecuteRequest(Action executeRequestDelegate) {
            HttpResponseMessage responseMessage = null;
            try {
                executeRequestDelegate.Invoke();
                responseMessage = Request.CreateResponse(HttpStatusCode.OK);
            } catch (Exception ex) {
                responseMessage = Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            return responseMessage;
        }
        protected async Task<HttpResponseMessage> ExecuteRequestAsync(Func<Task> executeRequestDelegate) {
            HttpResponseMessage responseMessage = null;
            try {
                await executeRequestDelegate.Invoke();
                responseMessage = Request.CreateResponse(HttpStatusCode.OK);
            } catch (Exception ex) {
                responseMessage = Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            return responseMessage;
        }

        protected HttpResponseMessage ExecuteRequest<T>(Func<T> executeRequestDelegate) where T : new() {
            HttpResponseMessage responseMessage = null;
            try {
                T data = executeRequestDelegate.Invoke();
                responseMessage = Request.CreateResponse(HttpStatusCode.OK, data);
            } catch (Exception ex) {
                responseMessage = Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            return responseMessage;
        }

        protected async Task<HttpResponseMessage> ExecuteRequestAsync<T>(Func<Task<T>> executeRequestDelegate) where T : new() {
            HttpResponseMessage responseMessage = null;
            try {
                T data = await executeRequestDelegate.Invoke();
                responseMessage = Request.CreateResponse(HttpStatusCode.OK, data);
            } catch (Exception ex) {
                responseMessage = Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            return responseMessage;
        }
    }
}
