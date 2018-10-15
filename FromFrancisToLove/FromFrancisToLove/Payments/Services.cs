using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Payments
{
    public class Finfit
    {
        public class Field
        {
            public string Name { get; set; }
            public int Type { get; set; }
            public int Length { get; set; }
            public int Class { get; set; }
            public object Value { get; set; }
            public bool Encrypt { get; set; }
            public string Checksum { get; set; }
        }

        public class ResponseService
        {
            public int AuthorizeCode { get; set; }
            public List<Field> Fields { get; set; }
            public string XML { get; set; } //OrException
            public bool Success { get; set; }
            public int ResponseCode { get; set; } //1000+ for exceptions
        }

        public class PaymentsService
        {
            private readonly string Url;
            private readonly string User;
            private readonly string Password;
            private readonly string EncryptedKey;
            private int Group { get; set; }
            private int Chain { get; set; }
            private int Merchant { get; set; }
            private int POS { get; set; }
            private int Cashier { get; set; }
            private bool IsConfigured { get; set; } = false;

            public PaymentsService(string Url, string User, string Password, string EncryptedKey = "")
            {
                this.Url = Url;
                this.User = User;
                this.Password = Password;
                this.EncryptedKey = EncryptedKey;
            }

            public void Config(int Group, int Chain, int Merchant, int POS, int Cashier = 1)
            {
                IsConfigured = true;
                this.Group = Group;
                this.Chain = Chain;
                this.Merchant = Merchant;
                this.POS = POS;
                this.Cashier = Cashier;
            }

            public List<Field> PaymentInfo(string SKU, string Reference)
            {
                if (!IsConfigured) throw new Exception();
                return new List<Field>();
            }


            public ResponseService Request(List<Field> Fields)
            {
                if (!IsConfigured) throw new Exception();
                return new ResponseService();
            }

            public ResponseService Check(List<Field> Fields)
            {
                if (!IsConfigured) throw new Exception();
                return new ResponseService();
            }

            public ResponseService Cancel(List<Field> Fields)
            {
                if (!IsConfigured) throw new Exception();
                return new ResponseService();
            }
        }
    }
}
