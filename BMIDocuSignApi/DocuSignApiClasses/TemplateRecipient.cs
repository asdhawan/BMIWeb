namespace DocuSignApiClasses {
    public class TemplateRecipient {
        public string TemplateID { get; set; }
        public string RoleName { get; set; }
        public string RoutingOrder { get; set; }
        public string RecipientID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool RequiredYN { get; set; }
        public bool LockedYN { get; set; }
    }
}
