using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Europe.CentralBank.CashServer.Controllers;

namespace Europe.CentralBank.CashServer.Utils {
    public class CashValidator {
        private RSACryptoServiceProvider key;
        
        public CashValidator(RSACryptoServiceProvider key) {
            this.key = key;
        }

        public Cash CashFromString(string str) {
            var stuff = str.Split('.');

            var part = string.Join(".", stuff.Take(stuff.Length - 1));
            var signature = stuff.Last();

            if (!key.VerifyData(Encoding.ASCII.GetBytes(part), Convert.FromBase64String(signature),
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1)) {
                throw new Exception("no signature");
            }

            return new Cash() {
                id = Int32.Parse(stuff[1]),
                amount = Int32.Parse(stuff[0]),
                created_at = DateTimeOffset.Parse(stuff[2]),
                data = str
            };
        }

        public string CashToString(Cash cash) {
            var str = $"{cash.amount}.{cash.id}.{cash.created_at}";

            var a = key.SignData(Encoding.ASCII.GetBytes(str), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            str += "." + Convert.ToBase64String(a);

            return str;
        }
    }
}
