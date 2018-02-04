using System;

namespace DocuSignApiClasses {
    public class RecipientStatus {
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime? Delivered { get; set; }
        public DateTime? Signed { get; set; }
        public Enums.EnvelopeStatusCode Status { get; set; }
    }
}
