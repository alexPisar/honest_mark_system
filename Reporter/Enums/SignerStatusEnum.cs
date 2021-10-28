using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Reporter.Enums
{
    public enum SignerStatusEnum
    {
        /// <summary>
        /// работник иной уполномоченной организации 
        /// </summary>
        [Display(Description = "3 - работник иной уполномоченной организации")]
        EmployeeOfAnotherAuthorizedOrganization = 3,

        /// <summary>
        /// уполномоченное физическое лицо, в том числе индивидуальный предприниматель
        /// </summary>
        [Display(Description = "4 - уполномоченное физическое лицо, в том числе индивидуальный предприниматель")]
        Individual,

        /// <summary>
        /// работник организации - покупателя
        /// </summary>
        [Display(Description = "5 - работник организации - покупателя")]
        EmployeeOfBuyerOrganization,

        /// <summary>
        /// работник организации – составителя файла обмена информации покупателя, если составитель файла обмена информации покупателя не является покупателем
        /// </summary>
        [Display(Description = "6 - работник организации – составителя файла обмена информации покупателя,\n если составитель файла обмена информации покупателя не является покупателем")]
        EmployeeOfOrganizationCompilerInformationExchangeFile
    }
}
