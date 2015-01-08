using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.Chinabank.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Payments.Chinabank.Vmid")]
        public string Vmid { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Chinabank.Key")]
        public string Key { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Chinabank.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
    }
}