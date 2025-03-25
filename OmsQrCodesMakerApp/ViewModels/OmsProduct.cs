using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmsQrCodesMakerApp.ViewModels
{
    public class OmsProduct
    {
        public string Gtin { get; set; }
        public WebSystems.Models.OMS.Product Product { get; set; }
        public WebSystems.Models.OMS.BufferInfo OrderStatus { get; set; }

        public string BufferStatus
        {
            get {
                switch (OrderStatus?.BufferStatus)
                {
                    case "PENDING":
                        return "Буфер КМ находится в ожидании";
                    case "ACTIVE":
                        return "Буфер создан";
                    case "EXHAUSTED":
                        return "Буфер и пулы РЭ не содержат больше кодов";
                    case "REJECTED":
                        return "Буфер более не доступен для работы ";
                    case "CLOSED":
                        return "Буфер закрыт";
                    default:
                        return "";
                }
            }
        }
    }
}
