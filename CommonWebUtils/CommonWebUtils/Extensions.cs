using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using System.Xml;

namespace CommonWebUtils {
    public static class Extensions {
        private static readonly string MachineKeyProtectionPurpose = "GENERAL_STRING_ENCRYPTION";

        public static string GetHash(this string input) {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();
            byte[] byteValue = Encoding.UTF8.GetBytes(input);
            byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);
            return Convert.ToBase64String(byteHash);
        }

        public static string Encrypt(this string stringToEncrypt) {
            byte[] stringToEncryptBytes = Encoding.UTF8.GetBytes(stringToEncrypt);
            byte[] protectedBytes = MachineKey.Protect(stringToEncryptBytes, MachineKeyProtectionPurpose);
            return Convert.ToBase64String(protectedBytes);
        }

        public static string Decrypt(this string stringToDecrypt) {
            byte[] stringToDecryptBytes = Convert.FromBase64String(stringToDecrypt);
            byte[] unprotectedBytes = MachineKey.Unprotect(stringToDecryptBytes, MachineKeyProtectionPurpose);
            return Encoding.UTF8.GetString(unprotectedBytes);
        }
        public static bool IsXml(this string stringToCheck) {
            bool bIsXml = true;
            try { stringToCheck.ToXml(); } catch { bIsXml = false; }
            return bIsXml;
        }

        public static bool IsJson(this string stringToCheck) {
            bool bIsJson = true;
            try { stringToCheck.ToJson(); } catch { bIsJson = false; }
            return bIsJson;
        }

        public static object ToJson(this string jsonString) {
            return JsonConvert.DeserializeObject(jsonString);
        }

        public static XmlDocument ToXml(this string xmlString) {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(xmlString);
            return xd;
        }
    }
}
