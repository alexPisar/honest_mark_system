using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Reporter.Enums
{
    public enum DocumentDiscrepancyTypeEnum
    {
        [Display(Description = "")]
        None,
        /// <summary>
        /// документ о приемке с расхождениями
        /// </summary>
        [Display(Description = "2 - документ о приемке с расхождениями")]
        DocumentWithDiscrepancy = 2,

        /// <summary>
        /// документ о расхождениях
        /// </summary>
        [Display(Description = "3 - документ о расхождениях")]
        DocumentAboutDiscrepancy
    }
}
