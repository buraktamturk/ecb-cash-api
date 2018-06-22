using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Europe.CentralBank.CashServer.Controllers;
using Microsoft.IdentityModel.Tokens;

namespace Europe.CentralBank.CashServer.Utils {
    public class CashValidator {
        private RsaSecurityKey key;
        
        public CashValidator(RsaSecurityKey key) {
            this.key = key;
        }

        public Cash CashFromString(string str) {
            var stuff = str.Split('.');

            var part = string.Join(".", stuff.Take(stuff.Length - 1));
            var signature = stuff.Last();

            key.Rsa.SignHash(System.Text.Encoding.ASCII.GetBytes(part), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            return new Cash() {
                id = Int32.Parse(stuff[0]),
                amount = Int32.Parse(stuff[1]),
                created_at = DateTimeOffset.Parse(stuff[2]),
                data = str
            };
        }

        public string CashToString(Cash cash) {
            var str = $"{cash.amount}.{cash.id}.{cash.created_at}";


            return str;
        }

    }
}
