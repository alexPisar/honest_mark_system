using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmsQrCodesMakerApp.ViewModels
{
    public class PowerOfAttorney
    {
        public string PersonSurname { get; set; }
        public string PersonName { get; set; }
        public string PersonPatronimycSurname { get; set; }
        public string PersonPosition { get; set; }
        public string PersonInn { get; set; }
        public string OrgInn { get; set; }
        public string OrgName { get; set; }
        public string EmchdId { get; set; }
        public virtual DateTime? EmchdBeginDate { get; set; }
        public virtual DateTime? EmchdEndDate { get; set; }
    }
}
