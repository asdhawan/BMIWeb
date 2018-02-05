using BMIWebUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Hosting;
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
                return DataStore.DocumentsRepository;
            });
        }

        [Route("{documentId}")]
        [HttpGet]
        [ResponseType(typeof(Document))]
        public HttpResponseMessage GetDocument(int documentId) {
            return ExecuteRequest(() => {
                return DataStore.DocumentsRepository.Where(x => x.DocumentId == documentId).FirstOrDefault();
            });
        }

        [Route("{documentId}/blob")]
        [HttpGet]
        [ResponseType(typeof(DocumentBlob))]
        public HttpResponseMessage GetDocuments(int documentId) {
            return ExecuteRequest(() => {
                string appDataRoot = HostingEnvironment.MapPath("~/App_Data");
                Document d = DataStore.DocumentsRepository.Where(x => x.DocumentId == documentId).FirstOrDefault();
                return new DocumentBlob() {
                    DocumentPdfBase64Encoded = Convert.ToBase64String(File.ReadAllBytes(appDataRoot + Path.DirectorySeparatorChar + d.DocumentName))
                };
            });
        }
    }
}
