using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DocuSignApiClasses {
    public class CreateEnvelopeRequest {
        [Required]
        public string EnvelopeRequestId { get; set; }
        [Required]
        public string DocuSignTemplateId { get; set; }
        [Required]
        public string EnvelopeSubject { get; set; }
        [Required]
        public List<TemplateRecipient> Signers { get; set; }
        [Required]
        public bool SignInPlaceYN { get; set; }
        public string SignInPlaceReturnUrl { get; set; }
        public string DocumentName { get; set; }
        public string DocumentPdfBase64Encoded { get; set; }
    }
}
