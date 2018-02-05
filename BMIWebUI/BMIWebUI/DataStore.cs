using BMIWebUI.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;

namespace BMIWebUI {
    public static class DataStore {
        private static List<Document> documentsRepository = null;
        public static List<Document> DocumentsRepository {
            get {
                if (documentsRepository == null) {
                    string appDataRoot = HostingEnvironment.MapPath("~/App_Data");
                    using (StreamReader reader = File.OpenText(appDataRoot + Path.DirectorySeparatorChar + "documents.json")) {
                        string json = reader.ReadToEnd();
                        documentsRepository = JsonConvert.DeserializeObject<List<Document>>(json);
                    }
                }
                return documentsRepository;
            }
        }
    }
}