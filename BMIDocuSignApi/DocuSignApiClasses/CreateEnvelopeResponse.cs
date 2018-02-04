using System.Collections.Generic;

namespace DocuSignApiClasses {
    public class CreateEnvelopeResponse {
        public string DocumentUniqueId { get; set; }
        public bool SuccessYN { get; set; }
        public List<InPlaceSigner> InPlaceSigners { get; set; }
    }
}
