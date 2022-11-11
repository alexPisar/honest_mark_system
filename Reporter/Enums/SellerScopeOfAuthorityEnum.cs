using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Reporter.Enums
{
    public enum SellerScopeOfAuthorityEnum
    {
        /// <summary>
        /// лицо, ответственное за подписание счетов-фактур
        /// </summary>
        [Display(Description = "0 - лицо, ответственное за подписание счетов-фактур")]
        PersonWhoResponsibleForSigning = 0,

        /// <summary>
        /// лицо, совершившее сделку, операцию
        /// </summary>
        [Display(Description = "1 - лицо, совершившее сделку, операцию")]
        PersonWhoMadeOperation,

        /// <summary>
        /// лицо, совершившее сделку, операцию и ответственное за ее оформление
        /// </summary>
        [Display(Description = "2 - лицо, совершившее сделку, операцию и ответственное за ее оформление")]
        PersonWhoMadeOperationAndResponsibleForItsExecution,

        /// <summary>
        /// лицо, ответственное за оформление свершившегося события
        /// </summary>
        [Display(Description = "3 - лицо, ответственное за оформление свершившегося события")]
        PersonWhoResponsibleForRegistrationExecution,

        /// <summary>
        /// лицо, совершившее сделку, операцию и ответственное за подписание счетов-фактур
        /// </summary>
        [Display(Description = "4 - лицо, совершившее сделку, операцию и ответственное за подписание счетов-фактур")]
        PersonWhoMadeOperationAndResponsibleForSigning,

        /// <summary>
        /// лицо, совершившее сделку, операцию и ответственное за ее оформление и за подписание счетов-фактур
        /// </summary>
        [Display(Description = "5 - лицо, совершившее сделку, операцию и ответственное за ее оформление и за подписание счетов-фактур")]
        PersonWhoMadeOperationAndResponsibleForItsExecutionAndSigning,

        /// <summary>
        /// лицо, ответственное за оформление свершившегося события и за подписание счетов-фактур
        /// </summary>
        [Display(Description = "6 - лицо, ответственное за оформление свершившегося события и за подписание счетов-фактур")]
        PersonWhoResponsibleForRegistrationExecutionAndSigning
    }
}
