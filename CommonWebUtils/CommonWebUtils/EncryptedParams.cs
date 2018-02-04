using System.Collections.Generic;
using System.Text;
using System.Web;

namespace CommonWebUtils {
    public class EncryptedParams : Dictionary<string, string> {
        public EncryptedParams() { }
        public EncryptedParams(string encryptedStringValue) {
            if (!string.IsNullOrEmpty(encryptedStringValue)) {
                string stringValue = HttpUtility.UrlDecode(encryptedStringValue).Decrypt();
                string[] pairs = stringValue.Split('&');
                foreach (string pair in pairs) {
                    string[] keyValueSet = pair.Split('=');
                    if (keyValueSet.Length == 2)
                        Add(keyValueSet[0], keyValueSet[1]);
                }
            }
        }
        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> kvp in this) {
                if (sb.Length != 0)
                    sb.Append("&");
                sb.AppendFormat("{0}={1}", kvp.Key, kvp.Value);
            }
            return HttpUtility.UrlEncode(sb.ToString().Encrypt());
        }
    }
}