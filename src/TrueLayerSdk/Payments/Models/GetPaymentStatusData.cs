using System;

namespace TrueLayerSdk.Payments.Models
{
    public class GetPaymentStatusData
    {
        public string simp_id { get; set; }
        public DateTime created_at { get; set; }
        public int amount { get; set; }
        public string currency { get; set; }
        public string beneficiary_reference { get; set; }
        public string beneficiary_name { get; set; }
        public string beneficiary_sort_code { get; set; }
        public string beneficiary_account_number { get; set; }
        public string remitter_reference { get; set; }
        public string redirect_uri { get; set; }
        public string remitter_provider_id { get; set; }
        public string status { get; set; }
    }
}
