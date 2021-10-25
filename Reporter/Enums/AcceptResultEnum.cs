using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Reporter.Enums
{
    public enum AcceptResultEnum
    {
        /// <summary>
        /// товары (работы, услуги, права) приняты без расхождений (претензий)
        /// </summary>
        [Display(Description = "товары (работы, услуги, права) приняты без расхождений (претензий)")]
        GoodsAcceptedWithoutDiscrepancy = 1,

        /// <summary>
        /// товары (работы, услуги, права) приняты с расхождениями (претензией)
        /// </summary>
        [Display(Description = "товары (работы, услуги, права) приняты с расхождениями (претензией)")]
        GoodsAcceptedWithDiscrepancy,

        /// <summary>
        /// товары (работы, услуги, права) не приняты
        /// </summary>
        [Display(Description = "товары (работы, услуги, права) не приняты")]
        GoodsNotAccepted
    }
}
