using CommonWebUtils;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using DocuSignApiClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BMIDocuSignApi {
    public static class DocuSignHelper {

        public static EnvelopeStatus GetEnvelopeStatus(string docuSignEnvelopeId) {
            EnvelopeStatus retVal = new EnvelopeStatus();
            InitializeApiClient();
            EnvelopesApi envelopesApi = new EnvelopesApi();
            Envelope envelope = envelopesApi.GetEnvelope(SystemConfiguration.DocuSignAccountId, docuSignEnvelopeId);
            Recipients recipients = envelopesApi.ListRecipients(SystemConfiguration.DocuSignAccountId, docuSignEnvelopeId);
            retVal.EnvelopeID = envelope.EnvelopeId;
            retVal.Subject = envelope.EmailSubject;
            retVal.Status = ConvertDocuSignEnvelopeStatusCode(envelope.Status);

            List<RecipientStatus> recipientStatusList = new List<RecipientStatus>();
            foreach (Signer s in recipients.Signers) {
                RecipientStatus recipientStatus = new RecipientStatus();
                recipientStatus.UserName = s.Name;
                recipientStatus.Email = s.Email;
                recipientStatus.Status = ConvertDocuSignEnvelopeStatusCode(s.Status);
                DateTime dtSent = DateTime.MinValue;
                DateTime dtSigned = DateTime.MinValue;
                if (DateTime.TryParse(s.DeliveredDateTime, out dtSent))
                    recipientStatus.Delivered = dtSent;
                if (DateTime.TryParse(s.SignedDateTime, out dtSigned))
                    recipientStatus.Signed = dtSigned;
                recipientStatusList.Add(recipientStatus);
            }

            retVal.RecipientStatuses = recipientStatusList;
            return retVal;
        }

        private static IEnumerable<Signer> GetTemplateSigners(string docuSignTemplateID) {
            InitializeApiClient();
            TemplatesApi api = new TemplatesApi();
            return api.ListRecipients(SystemConfiguration.DocuSignAccountId, docuSignTemplateID).Signers.OrderBy(x => x.RoutingOrder);
        }

        public static IEnumerable<TemplateRecipient> GetTemplateRecipients(string docuSignTemplateID) {
            return GetTemplateRecipients(docuSignTemplateID, GetTemplateSigners(docuSignTemplateID));
        }

        public static IEnumerable<TemplateRecipient> GetTemplateRecipients(string docuSignTemplateID, IEnumerable<Signer> signers) {
            return signers.Where(x => !string.IsNullOrEmpty(x.RoleName)).Select(x => new TemplateRecipient() {
                TemplateID = docuSignTemplateID,
                RecipientID = x.RecipientId,
                UserName = x.Name,
                RoleName = x.RoleName,
                Email = x.Email,
                RoutingOrder = x.RoutingOrder,
                LockedYN = Convert.ToBoolean(x.TemplateLocked),
                RequiredYN = Convert.ToBoolean(x.TemplateRequired)
            });
        }

        private static EnvelopeSummary CreateAndSendEnvelope(string envelopeRequestId, string docuSignTemplateId, string subject, List<Signer> signers, string documentName = null, string documentPdfBase64Encoded =null) {
            EnvelopeSummary retVal = null;

            InitializeApiClient();

            TemplatesApi templatesApi = new TemplatesApi();

            EnvelopeTemplate envTemplate = null;
            if ((envTemplate = templatesApi.Get(SystemConfiguration.DocuSignAccountId, docuSignTemplateId)) == null ||
                 envTemplate.EnvelopeTemplateDefinition == null ||
                 envTemplate.Documents == null ||
                 envTemplate.Documents.Count != 1)
                throw new Exception("Error loading template information from DocuSign");
            else {
                List<Document> documentList = new List<Document>();

                EnvelopeDefinition envDef = new EnvelopeDefinition() {
                    EmailSubject = subject,
                    TemplateId = docuSignTemplateId,
                    TemplateRoles = new List<TemplateRole>()
                };

                if (documentName != null && documentPdfBase64Encoded != null) {
                    envDef.Documents = BuildDocumentList(envTemplate.Documents[0].DocumentId, documentName, documentPdfBase64Encoded);
                } else {
                    envDef.Documents = envTemplate.Documents;
                }

                EnvelopesApi envelopesApi = new EnvelopesApi();

                //create a draft envelope
                retVal = envelopesApi.CreateEnvelope(SystemConfiguration.DocuSignAccountId, envDef);

                //get all recipients
                Recipients allRecipients = envelopesApi.ListRecipients(SystemConfiguration.DocuSignAccountId, retVal.EnvelopeId);

                Dictionary<string, Signer> documentSignersDict = signers.ToDictionary(x => x.RoleName, StringComparer.InvariantCultureIgnoreCase);

                //update recipients with names and e-mails and delete recipients that are not needed
                Recipients recipientsToDelete = new Recipients();
                recipientsToDelete.Signers = new List<Signer>();

                Recipients recipientsToUpdate = new Recipients();
                recipientsToUpdate.Signers = new List<Signer>();

                foreach (Signer s in allRecipients.Signers) {
                    Signer existing = null;
                    if (!documentSignersDict.TryGetValue(s.RoleName, out existing))
                        recipientsToDelete.Signers.Add(s);
                    else {
                        s.Name = existing.Name;
                        s.Email = existing.Email;
                        s.ClientUserId = existing.ClientUserId;
                        recipientsToUpdate.Signers.Add(s);
                    }
                }

                if (recipientsToDelete.Signers.Count != 0)
                    envelopesApi.DeleteRecipients(SystemConfiguration.DocuSignAccountId, retVal.EnvelopeId, recipientsToDelete);

                if (recipientsToUpdate.Signers.Count != 0)
                    envelopesApi.UpdateRecipients(SystemConfiguration.DocuSignAccountId, retVal.EnvelopeId, recipientsToUpdate);

                //send the envelope
                string authHeaderString = CreateRestAuthenticationHeaderString();
                envelopesApi.UpdateStatus(SystemConfiguration.DocuSignAccountId, retVal.EnvelopeId, "sent", authHeaderString);
            }

            return retVal;
        }

        private static void UpdateStatus(this EnvelopesApi envelopesApi, string accountId, string envelopeId, string newStatus, string restAuthHeaderString) {
            //send the envelope
            RestSharp.RestClient rc = new RestSharp.RestClient(envelopesApi.GetBasePath());
            RestSharp.RestRequest updateReq = new RestSharp.RestRequest(string.Format("/v2/accounts/{0}/envelopes/{1}", accountId, envelopeId), RestSharp.Method.PUT);
            updateReq.AddHeader("X-DocuSign-Authentication", restAuthHeaderString);
            updateReq.JsonSerializer = new NewtonsoftJsonSerializer();
            updateReq.RequestFormat = RestSharp.DataFormat.Json;
            updateReq.AddJsonBody(new { status = newStatus });
            rc.Execute(updateReq);
        }

        private static List<Document> BuildDocumentList(string documentId, string documentName, string documentPdfBase64Encoded) {
            List<Document> dsDocList = new List<Document>();
            dsDocList.Add(new Document() {
                DocumentBase64 = documentPdfBase64Encoded,
                DocumentId = documentId,
                Name = documentName
            });
            return dsDocList;
        }

        private static void InitializeApiClient() {
            Configuration.Default.ApiClient = new ApiClient(SystemConfiguration.DocuSignRestApiBasePath);
            Configuration.Default.DefaultHeader.Clear();
            Configuration.Default.AddDefaultHeader("X-DocuSign-Authentication", CreateRestAuthenticationHeaderString());
        }

        private static string CreateRestAuthenticationHeaderString() {
            return string.Format(
                "{{\"Username\":\"{0}\", \"Password\":\"{1}\", \"IntegratorKey\":\"{2}\"}}",
                SystemConfiguration.DocuSignUsername,
                SystemConfiguration.DocuSignPassword,
                SystemConfiguration.DocuSignIntegratorKey);
        }

        private static Enums.EnvelopeStatusCode ConvertDocuSignEnvelopeStatusCode(string envelopeStatus) {
            Enums.EnvelopeStatusCode retVal = Enums.EnvelopeStatusCode.Any;
            switch (envelopeStatus) {
                case "any": retVal = Enums.EnvelopeStatusCode.Any; break;
                case "completed": retVal = Enums.EnvelopeStatusCode.Completed; break;
                case "created": retVal = Enums.EnvelopeStatusCode.Created; break;
                case "declined": retVal = Enums.EnvelopeStatusCode.Declined; break;
                case "deleted": retVal = Enums.EnvelopeStatusCode.Deleted; break;
                case "delivered": retVal = Enums.EnvelopeStatusCode.Delivered; break;
                case "processing": retVal = Enums.EnvelopeStatusCode.Processing; break;
                case "sent": retVal = Enums.EnvelopeStatusCode.Sent; break;
                case "signed": retVal = Enums.EnvelopeStatusCode.Signed; break;
                case "template": retVal = Enums.EnvelopeStatusCode.Template; break;
                case "timedOut": retVal = Enums.EnvelopeStatusCode.TimedOut; break;
                case "voided": retVal = Enums.EnvelopeStatusCode.Voided; break;
            }
            return retVal;
        }

        public static CreateEnvelopeResponse ProcessNewDocuSignEnvelope(
            string envelopeRequestId,
            string docuSignTemplateId,
            string subject,
            List<TemplateRecipient> selectedRecipients,
            string documentName = null,
            string documentPdfBase64Encoded = null,
            bool signInPlace = false,
            string inPlaceSigningPageUrl = null) {
            CreateEnvelopeResponse createEnvelopeResponse = new CreateEnvelopeResponse() {
                EnvelopeRequestId = envelopeRequestId,
                InPlaceSigners = new List<InPlaceSigner>()
            };
            try {
                InitializeApiClient();

                IEnumerable<Signer> signers = GetTemplateSigners(docuSignTemplateId);
                List<Signer> signerList = new List<Signer>();

                //find and add the primary recipient as a captive user
                string primaryRecipientClientUserId = Guid.NewGuid().ToString();
                Dictionary<string, Signer> recipientsDict = signers.ToDictionary(x => x.RoleName, StringComparer.InvariantCultureIgnoreCase);
                foreach (TemplateRecipient r in selectedRecipients) {
                    Signer matchingSigner = null;
                    if (!recipientsDict.TryGetValue(r.RoleName, out matchingSigner))
                        throw new Exception(string.Format("Error loading recipient information for Role = {0}", r.RoleName));
                    else {
                        matchingSigner.Name = r.UserName;
                        matchingSigner.Email = r.Email;
                        matchingSigner.RoutingOrder = r.RoutingOrder;
                        if (signInPlace) {
                            matchingSigner.ClientUserId = Guid.NewGuid().ToString();
                            createEnvelopeResponse.InPlaceSigners.Add(new InPlaceSigner() {
                                RoleName = r.RoleName,
                                ClientUserId = matchingSigner.ClientUserId
                            });
                        }
                        signerList.Add(matchingSigner);
                    }
                }

                if (signerList.Count == 0)
                    throw new Exception("Not enough recipients. At least 1 signer is required to proceed.");

                EnvelopeSummary envSummary = CreateAndSendEnvelope(envelopeRequestId, docuSignTemplateId, subject, signerList, documentName, documentPdfBase64Encoded);
                
                createEnvelopeResponse.SuccessYN = envSummary.EnvelopeId != null;
                createEnvelopeResponse.EnvelopeId = envSummary.EnvelopeId;
            } catch { /*Do something*/ }
            return createEnvelopeResponse;
        }

        public static string GetRecipientViewUrl(string inPlaceSigningPageUrl, string docuSignEnvelopeId, string clientUserId) {
            string retUrl = null;

            InitializeApiClient();

            EnvelopesApi envelopesApi = new EnvelopesApi();
            Recipients recipients = envelopesApi.ListRecipients(SystemConfiguration.DocuSignAccountId, docuSignEnvelopeId);
            Signer signer = recipients.Signers.FirstOrDefault(x => x.ClientUserId == clientUserId);
            if (signer != null) {
                retUrl = GetRecipientViewUrl(inPlaceSigningPageUrl, docuSignEnvelopeId, clientUserId, signer.UserId);
            }
            return retUrl;
        }

        public static string GetNextRecipientViewUrl(string inPlaceSigningPageUrl, string docuSignEnvelopeId) {
            string retUrl = null;

            InitializeApiClient();

            EnvelopesApi envelopesApi = new EnvelopesApi();
            Recipients recipients = envelopesApi.ListRecipients(SystemConfiguration.DocuSignAccountId, docuSignEnvelopeId);
            Signer signer = recipients.Signers.FirstOrDefault(x => !string.IsNullOrEmpty(x.ClientUserId) && x.Status != "completed");
            if (signer != null) {
                retUrl = GetRecipientViewUrl(inPlaceSigningPageUrl, docuSignEnvelopeId, signer.ClientUserId, signer.UserId);
            }
            return retUrl;
        }

        private static string GetRecipientViewUrl(string inPlaceSigningPageUrl, string docuSignEnvelopeId, string clientUserId, string userId) {
            RecipientViewRequest viewOptions = new RecipientViewRequest() {
                ReturnUrl = string.Format("{0}?envelopeId={1}", inPlaceSigningPageUrl, docuSignEnvelopeId),
                ClientUserId = clientUserId,
                UserId = userId,
                AuthenticationMethod = "none",                
            };

            EnvelopesApi envelopesApi = new EnvelopesApi();
            ViewUrl recipientView = envelopesApi.CreateRecipientView(SystemConfiguration.DocuSignAccountId, docuSignEnvelopeId, viewOptions);
            return recipientView.Url;
        }
    }
}