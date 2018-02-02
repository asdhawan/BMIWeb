using BMIWebUI.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace BMIWebUI.Controllers {
    [RoutePrefix("api/documents")]
    public class DocumentsApiController : BaseApiController {
        [Route("")]
        [HttpGet]
        [ResponseType(typeof(IEnumerable<Document>))]
        public HttpResponseMessage GetDocuments() {
            return ExecuteRequest(() => {
                List<Document> dummyDocs = new List<Document>();
                dummyDocs.Add(new Document() { DocumentId = 1, DocumentName = "Declaracion de Buenas Salud VVV.V.pdf", DocuSignableYN = true, DocuSignTemplateId = "" });
                dummyDocs.Add(new Document() { DocumentId = 2, DocumentName = "BMI-Aviation-VVV.V.pdf", DocuSignableYN = true, DocuSignTemplateId = "" });
                dummyDocs.Add(new Document() { DocumentId = 3, DocumentName = "LI-Endorsements – Policy XXXXXXXXXXXX – Name – mm-dd-yyy-.pdf", DocuSignableYN = false, DocuSignTemplateId = "" });
                dummyDocs.Add(new Document() { DocumentId = 4, DocumentName = "declaracion de no fumador – declaration of non-smoker status bmic_1.pdf", DocuSignableYN = true, DocuSignTemplateId = "" });
                return dummyDocs;
            });
        }

    }
}
