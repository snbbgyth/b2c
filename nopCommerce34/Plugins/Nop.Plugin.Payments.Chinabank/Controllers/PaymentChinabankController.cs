using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.Chinabank.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.Chinabank.Controllers
{
    public class PaymentChinabankController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly ChinabankPaymentSettings _chinabankPaymentSettings;
        private readonly PaymentSettings _paymentSettings;

        public PaymentChinabankController(ISettingService settingService, 
            IPaymentService paymentService, IOrderService orderService, 
            IOrderProcessingService orderProcessingService, 
            ILogger logger, IWebHelper webHelper,
            ChinabankPaymentSettings chinabankPaymentSettings,
            PaymentSettings paymentSettings)
        {
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._logger = logger;
            this._webHelper = webHelper;
            this._chinabankPaymentSettings = chinabankPaymentSettings;
            this._paymentSettings = paymentSettings;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.Vmid = _chinabankPaymentSettings.Vmid;
            model.Key = _chinabankPaymentSettings.Key;
            model.AdditionalFee = _chinabankPaymentSettings.AdditionalFee;
            return View("~/Plugins/Payments.Chinabank/Views/PaymentChinabank/Configure.cshtml", model);
            //return View("Nop.Plugin.Payments.Chinabank.Views.PaymentChinabank.Configure", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _chinabankPaymentSettings.Vmid = model.Vmid;
            _chinabankPaymentSettings.Key = model.Key;
            _chinabankPaymentSettings.AdditionalFee = model.AdditionalFee;
            _settingService.SaveSetting(_chinabankPaymentSettings);
            return View("~/Plugins/Payments.Chinabank/Views/PaymentChinabank/Configure.cshtml", model);
            //return View("Nop.Plugin.Payments.Chinabank.Views.PaymentChinabank.Configure", model);
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();
            return View("~/Plugins/Payments.Chinabank/Views/PaymentChinabank/PaymentInfo.cshtml", model);
            //return View("Nop.Plugin.Payments.Chinabank.Views.PaymentChinabank.PaymentInfo", model);
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        [ValidateInput(false)]
        public ActionResult Notify(FormCollection form)
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.Chinabank") as ChinabankPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("Chinabank module cannot be loaded");

            string vmid = _chinabankPaymentSettings.Vmid;
            if (string.IsNullOrEmpty(vmid))
                throw new Exception("Vmid is not set");
            string key = _chinabankPaymentSettings.Key;
            if (string.IsNullOrEmpty(key))
                throw new Exception("Key is not set");

            string v_oid = Request["v_oid"];
            string v_pstatus = Request["v_pstatus"];
            string v_pstring = Request["v_pstring"];
            string v_pmode = Request["v_pmode"];
            string v_md5str = Request["v_md5str"];
            string v_amount = Request["v_amount"];
            string v_moneytype = Request["v_moneytype"];
            string remark1 = Request["remark1"];
            string remark2 = Request["remark2"];

            string mysign = v_oid + v_pstatus + v_amount + v_moneytype + key;
            mysign = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(mysign, "md5").ToUpper();

            if (mysign == v_md5str)
            {
                if (v_pstatus.Equals("20"))
                {
                    int orderId = 0;
                    if (Int32.TryParse(v_oid, out orderId))
                    {
                        var order = _orderService.GetOrderById(orderId);
                        if (order != null && _orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            _orderProcessingService.MarkOrderAsPaid(order);
                        }
                    }
                }

                Response.Write("success");
            }
            else
            {
                Response.Write("fail");
                string logStr = "MD5:mysign=" + mysign + ",sign=" + mysign + ",responseTxt=校验失败,数据可疑";
                _logger.Error(logStr);
            }

            return Content("");
        }

        [ValidateInput(false)]
        public ActionResult Return()
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.Chinabank") as ChinabankPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("Chinabank module cannot be loaded");

            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}