using System.Collections.Generic;

namespace DocuSignApiClasses {
    public class EnvelopeStatus {
        public string EnvelopeID { get; set; }
        public string Subject { get; set; }
        public Enums.EnvelopeStatusCode Status { get; set; }
        public List<RecipientStatus> RecipientStatuses { get; set; }
        public EnvelopeStatus() { RecipientStatuses = new List<RecipientStatus>(); }
    }
}
