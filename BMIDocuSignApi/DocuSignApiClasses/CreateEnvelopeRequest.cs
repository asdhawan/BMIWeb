using System.Collections.Generic;

namespace DocuSignApiClasses {
    public class CreateEnvelopeRequest {
        public string DocumentUniqueId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentPdfBase64Encoded { get; set; }
        public string DocuSignTemplateId { get; set; }
        public string EnvelopeSubject { get; set; }
        public List<TemplateRecipient> Signers { get; set; }
        public bool SignInPlaceYN { get; set; }
        public string SignInPlaceReturnUrl { get; set; }
    }
}
