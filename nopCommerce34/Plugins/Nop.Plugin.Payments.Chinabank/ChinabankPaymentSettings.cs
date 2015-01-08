using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Chinabank
{
    public class ChinabankPaymentSettings:ISettings
    {
        public string Vmid { get; set; }
        public string Key { get; set; }
        public decimal AdditionalFee { get; set; }
    }
}
