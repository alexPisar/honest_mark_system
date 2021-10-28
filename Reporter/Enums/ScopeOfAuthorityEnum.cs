using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Reporter.Enums
{
    public enum ScopeOfAuthorityEnum
    {
        /// <summary>
        /// лицо, совершившее сделку, операцию
        /// </summary>
        [Display(Description = "1 - лицо, совершившее сделку, операцию")]
        PersonWhoMadeOperation = 1,

        /// <summary>
        /// лицо, совершившее сделку, операцию и ответственное за ее оформление
        /// </summary>
        [Display(Description = "2 - лицо, совершившее сделку, операцию и ответственное за ее оформление")]
        PersonWhoMadeOperationAndResponsibleForItsExecution,

        /// <summary>
        /// лицо, ответственное за оформление свершившегося события
        /// </summary>
        [Display(Description = "3 - лицо, ответственное за оформление свершившегося события")]
        PersonWhoResponsibleForRegistrationExecution
    }
}
