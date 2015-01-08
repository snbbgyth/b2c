using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.Chinabank
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.Chinabank.Configure",
                 "Plugins/PaymentChinabank/Configure",
                 new { controller = "PaymentChinabank", action = "Configure" },
                 new[] { "Nop.Plugin.Payments.Chinabank.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.Chinabank.PaymentInfo",
                 "Plugins/PaymentChinabank/PaymentInfo",
                 new { controller = "PaymentChinabank", action = "PaymentInfo" },
                 new[] { "Nop.Plugin.Payments.Chinabank.Controllers" }
            );

            //Notify
            routes.MapRoute("Plugin.Payments.Chinabank.Notify",
                 "Plugins/PaymentChinabank/Notify",
                 new { controller = "PaymentChinabank", action = "Notify" },
                 new[] { "Nop.Plugin.Payments.Chinabank.Controllers" }
            );

            //Notify
            routes.MapRoute("Plugin.Payments.Chinabank.Return",
                 "Plugins/PaymentChinabank/Return",
                 new { controller = "PaymentChinabank", action = "Return" },
                 new[] { "Nop.Plugin.Payments.Chinabank.Controllers" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
