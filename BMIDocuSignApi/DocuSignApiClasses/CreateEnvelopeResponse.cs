﻿using System.Collections.Generic;

namespace DocuSignApiClasses {
    public class CreateEnvelopeResponse {
        public string EnvelopeRequestId { get; set; }
        public bool SuccessYN { get; set; }
        public string EnvelopeId { get; set; }
        public List<InPlaceSigner> InPlaceSigners { get; set; }
    }
}
