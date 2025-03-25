using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmsQrCodesMakerApp.ViewModels
{
    public class OmsOrder
    {
        public OmsOrder()
        {
            OrderStatuses = new List<WebSystems.Models.OMS.BufferInfo>();
            Products = new List<OmsProduct>();
        }

        public WebSystems.Models.OMS.OrderInfo OrderInfo { get; set; }
        public List<WebSystems.Models.OMS.BufferInfo> OrderStatuses { get; set; }
        public List<OmsProduct> Products { get; set; }

        public string OrderId => OrderInfo?.OrderId;
        public string OrderStatus
        {
            get {
                switch (OrderInfo?.OrderStatus)
                {
                    case "CREATED":
                        return "Заказ создан";
                    case "PENDING":
                        return "Заказ ожидает подтверждения ГИС МТ";
                    case "DECLINED":
                        return "Заказ не подтверждён в ГИС МТ";
                    case "APPROVED":
                        return "Заказ подтверждён в ГИС МТ";
                    case "READY":
                        return "Заказ готов";
                    case "CLOSED":
                        return "Заказ закрыт";
                    default:
                        return "";
                }

            }
        }

        public int? TotalCodes => OrderStatuses?.Sum(s => s.TotalCodes);
        public int? ProductCount => Products.Count;

        public DateTime? DateCreate
        {
            get {
                if (OrderInfo?.CreatedTimestamp == null)
                    return null;
                
                return new DateTime(OrderInfo.CreatedTimestamp.Value * 10000 + 621355968000000000);
            }
        }
    }
}
