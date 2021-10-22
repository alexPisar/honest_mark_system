using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Enums
{
    public enum AcceptResultEnum
    {
        /// <summary>
        /// товары (работы, услуги, права) приняты без расхождений (претензий)
        /// </summary>
        GoodsAcceptedWithoutDiscrepancy = 1,

        /// <summary>
        /// товары (работы, услуги, права) приняты с расхождениями (претензией)
        /// </summary>
        GoodsAcceptedWithDiscrepancy,

        /// <summary>
        /// товары (работы, услуги, права) не приняты
        /// </summary>
        GoodsNotAccepted
    }
}
