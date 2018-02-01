namespace BMIWebUI.Models {
    public class Document {
        public int DocumentId { get; set; }
        public string DocumentName { get; set; }
        public bool DocuSignableYN { get; set; }
        public string DocuSignTemplateId { get; set; }
    }
}