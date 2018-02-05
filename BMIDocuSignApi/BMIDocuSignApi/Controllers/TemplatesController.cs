using CommonWebUtils;
using DocuSignApiClasses;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;

namespace BMIDocuSignApi.Controllers {
    [RoutePrefix("api/templates")]
    [EnableCors("*", "*", "*")]
    public class TemplatesController : BaseApiController {
        [Route("{docuSignTemplateId}/recipients")]
        [HttpGet]
        [ResponseType(typeof(List<TemplateRecipient>))]
        public HttpResponseMessage GetRecipients(string docuSignTemplateId) {
            return ExecuteRequest(() => {
                return DocuSignHelper.GetTemplateRecipients(docuSignTemplateId).ToList();
            });
        }
    }
}