using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Enums
{
    public enum SignerStatusEnum
    {
        /// <summary>
        /// работник иной уполномоченной организации 
        /// </summary>
        EmployeeOfAnotherAuthorizedOrganization = 3,

        /// <summary>
        /// уполномоченное физическое лицо, в том числе индивидуальный предприниматель
        /// </summary>
        Individual,

        /// <summary>
        /// работник организации - покупателя
        /// </summary>
        EmployeeOfBuyerOrganization,

        /// <summary>
        /// работник организации – составителя файла обмена информации покупателя, если составитель файла обмена информации покупателя не является покупателем
        /// </summary>
        EmployeeOfOrganizationCompilerInformationExchangeFile
    }
}
