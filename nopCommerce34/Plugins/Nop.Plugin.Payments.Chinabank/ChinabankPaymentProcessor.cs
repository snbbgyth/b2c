using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.Chinabank.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework;

namespace Nop.Plugin.Payments.Chinabank
{
    /// <summary>
    /// Chinabank payment processor
    /// </summary>
    public class ChinabankPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly ChinabankPaymentSettings _chinabankPayPaymentSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public ChinabankPaymentProcessor(ChinabankPaymentSettings chinabankPayPaymentSettings,
            StoreInformationSettings storeInformationSettings,
            ISettingService settingService, IWebHelper webHelper)
        {
            this._chinabankPayPaymentSettings = chinabankPayPaymentSettings;
            this._storeInformationSettings = storeInformationSettings;
            this._settingService = settingService;
            this._webHelper = webHelper;
        }

        #endregion

        #region Utilities

        
        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            RemotePost post = new RemotePost();
            post.FormName = "chinabanksubmit";
            post.Url = "https://pay3.chinabank.com.cn/PayGate";
            post.Method = "POST";

            //��Ҫ�Ľ�����Ϣ
            string v_amount;       // �������
            string v_moneytype;    // ����
            string v_md5info;      // ��ƴ�մ�MD5˽Կ���ܺ��ֵ
            string v_mid;		 // �̻���
            string v_url;		 // ����ҳ��ַ
            string v_oid;		 // �Ƽ������Ź��ɸ�ʽΪ ������-�̻���-Сʱ������
            //�ջ���Ϣ
            string v_rcvname;      // �ջ���
            string v_rcvaddr;      // �ջ���ַ
            string v_rcvtel;       // �ջ��˵绰
            string v_rcvpost;      // �ջ����ʱ�
            string v_rcvemail;     // �ջ����ʼ�
            string v_rcvmobile;    // �ջ����ֻ���
            //��������Ϣ
            string v_ordername;    // ����������
            string v_orderaddr;    // �����˵�ַ
            string v_ordertel;     // �����˵绰
            string v_orderpost;    // �������ʱ�
            string v_orderemail;   // �������ʼ�
            string v_ordermobile;  // �������ֻ���

            //������ע
            string remark1;
            string remark2;

            v_mid = _chinabankPayPaymentSettings.Vmid;
            v_url = _webHelper.GetStoreLocation(false) + "Plugins/PaymentChinabank/Notify";
            string key = _chinabankPayPaymentSettings.Key;
            v_oid = postProcessPaymentRequest.Order.Id.ToString();

            if (v_oid == null || v_oid.Equals(""))
            {
                DateTime dt = DateTime.Now;
                string v_ymd = dt.ToString("yyyyMMdd"); // yyyyMMdd
                string timeStr = dt.ToString("HHmmss"); // HHmmss
                v_oid = v_ymd + v_mid + timeStr;
            }

            v_amount = postProcessPaymentRequest.Order.OrderTotal.ToString("0.00", CultureInfo.InvariantCulture);
            v_moneytype = "CNY";
            string text = v_amount + v_moneytype + v_oid + v_mid + v_url + key; // ƴ�ռ��ܴ�
            v_md5info = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(text, "md5").ToUpper();

            //�ջ���Ϣ
            v_rcvname = String.Empty;
            v_rcvaddr = postProcessPaymentRequest.Order.BillingAddress.Address1;
            v_rcvtel = postProcessPaymentRequest.Order.BillingAddress.PhoneNumber;
            v_rcvpost = postProcessPaymentRequest.Order.BillingAddress.ZipPostalCode;
            v_rcvemail = postProcessPaymentRequest.Order.BillingAddress.Email;
            v_rcvmobile = postProcessPaymentRequest.Order.BillingAddress.PhoneNumber;
            //��������Ϣ
            v_ordername = String.Empty;
            v_orderaddr = postProcessPaymentRequest.Order.ShippingAddress.Address1;
            v_ordertel = postProcessPaymentRequest.Order.ShippingAddress.PhoneNumber;
            v_orderpost = postProcessPaymentRequest.Order.ShippingAddress.ZipPostalCode;
            v_orderemail = postProcessPaymentRequest.Order.ShippingAddress.Email;
            v_ordermobile = postProcessPaymentRequest.Order.ShippingAddress.PhoneNumber;
            //������Ϣ
            remark1 = "";
            remark2 = "";

            post.Add("v_md5info", v_md5info);
            post.Add("v_mid", v_mid);
            post.Add("v_oid", v_oid);
            post.Add("v_amount", v_amount);
            post.Add("v_moneytype", v_moneytype);
            post.Add("v_url", v_url);
            post.Add("v_rcvname", v_rcvname);
            post.Add("v_rcvaddr", v_rcvaddr);
            post.Add("v_rcvtel", v_rcvtel);
            post.Add("v_rcvpost", v_rcvpost);
            post.Add("v_rcvemail", v_rcvemail);
            post.Add("v_rcvmobile", v_rcvmobile);
            post.Add("v_ordername", v_ordername);
            post.Add("v_orderaddr", v_orderaddr);
            post.Add("v_ordertel", v_ordertel);
            post.Add("v_orderpost", v_orderpost);
            post.Add("v_orderemail", v_orderemail);
            post.Add("v_ordermobile", v_ordermobile);
            post.Add("remark1", remark1);
            post.Add("remark2", remark2);

            post.Post();
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee()
        {
            return _chinabankPayPaymentSettings.AdditionalFee;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //AliPay is the redirection payment method
            //It also validates whether order is also paid (after redirection) so customers will not be able to pay twice
            
            //payment status should be Pending
            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;

            //let's ensure that at least 1 minute passed after order is placed
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes < 1)
                return false;

            return true;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentChinabank";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Chinabank.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentChinabank";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Chinabank.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentChinabankController);
        }

        public override void Install()
        {
            //settings
            var settings = new ChinabankPaymentSettings()
            {
                Vmid = "",
                Key = "",
                AdditionalFee = 0,
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Chinabank.RedirectionTip", "You will be redirected to Chinabank site to complete the order.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Chinabank.Vmid", "Vmid");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Chinabank.Vmid.Hint", "Enter vmid.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Chinabank.Key", "Key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Chinabank.Key.Hint", "Enter key.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Chinabank.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Chinabank.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            
            base.Install();
        }


        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.Chinabank.RedirectionTip");
            this.DeletePluginLocaleResource("Plugins.Payments.Chinabank.Vmid");
            this.DeletePluginLocaleResource("Plugins.Payments.Chinabank.Vmid.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Chinabank.Key");
            this.DeletePluginLocaleResource("Plugins.Payments.Chinabank.Key.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Chinabank.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.Chinabank.AdditionalFee.Hint");
            
            base.Uninstall();
        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            return false;
        }

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {

            return _chinabankPayPaymentSettings.AdditionalFee;
        }
        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        #endregion





    }
}
