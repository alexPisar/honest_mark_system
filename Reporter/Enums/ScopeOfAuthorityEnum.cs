using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Enums
{
    public enum ScopeOfAuthorityEnum
    {
        /// <summary>
        /// лицо, совершившее сделку, операцию
        /// </summary>
        PersonWhoMadeOperation = 1,

        /// <summary>
        /// лицо, совершившее сделку, операцию и ответственное за ее оформление
        /// </summary>
        PersonWhoMadeOperationAndResponsibleForItsExecution,

        /// <summary>
        /// лицо, ответственное за оформление свершившегося события
        /// </summary>
        PersonWhoResponsibleForRegistrationExecution
    }
}
