using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Enums
{
    public enum DocumentDiscrepancyTypeEnum
    {
        None,
        /// <summary>
        /// документ о приемке с расхождениями
        /// </summary>
        DocumentWithDiscrepancy = 2,

        /// <summary>
        /// документ о расхождениях
        /// </summary>
        DocumentAboutDiscrepancy
    }
}
