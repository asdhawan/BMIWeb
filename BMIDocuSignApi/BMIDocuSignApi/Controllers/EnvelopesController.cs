using CommonWebUtils;
using DocuSignApiClasses;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;

namespace BMIDocuSignApi.Controllers {
    [RoutePrefix("api/envelopes")]
    [EnableCors("*", "*", "*")]
    public class EnvelopesController : BaseApiController {
        [Route("")]
        [HttpPost]
        [ResponseType(typeof(CreateEnvelopeResponse))]
        public HttpResponseMessage GetRecipients(CreateEnvelopeRequest request) {
            return ExecuteRequest(() => {
                return DocuSignHelper.ProcessNewDocuSignEnvelope(
                    request.DocumentUniqueId,
                    request.DocumentName,
                    request.DocumentPdfBase64Encoded,
                    request.DocuSignTemplateId,
                    request.EnvelopeSubject,
                    request.Signers,
                    request.SignInPlaceYN,
                    request.SignInPlaceReturnUrl);
            });
        }

        [Route("{docuSignEnvelopeId}/status")]
        [HttpGet]
        [ResponseType(typeof(EnvelopeStatus))]
        public HttpResponseMessage GetRecipients(string docuSignEnvelopeId) {
            return ExecuteRequest(() => {
                return DocuSignHelper.GetEnvelopeStatus(docuSignEnvelopeId);
            });
        }

        [Route("{docuSignEnvelopeId}/inplacesignurl/{clientUserId}")]
        [HttpGet]
        [ResponseType(typeof(InPlaceSigningResponse))]
        public HttpResponseMessage GetInPlaceSigningUrl(string signInPlaceReturnUrl, string docuSignEnvelopeId, string clientUserId) {
            return ExecuteRequest(() => {
                return new InPlaceSigningResponse() { SigningUrl = DocuSignHelper.GetRecipientViewUrl(signInPlaceReturnUrl, docuSignEnvelopeId, clientUserId) };
            });
        }
    }
}