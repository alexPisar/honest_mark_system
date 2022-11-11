using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Reporter.Enums
{
    public enum SellerSignerStatusEnum
    {
        /// <summary>
        /// работник организации продавца товаров (работ, услуг, имущественных прав)
        /// </summary>
        [Display(Description = "1 - работник организации продавца товаров (работ, услуг, имущественных прав)")]
        EmployeeOfSellerOrganization = 1,

        /// <summary>
        /// работник организации - составителя файла обмена информации продавца, если составитель файла обмена информации не является продавцом
        /// </summary>
        [Display(Description = "2 - работник организации - составителя файла обмена информации продавца, если составитель файла обмена информации не является продавцом")]
        EmployeeOfMakerSellerDocumentOrganization,

        /// <summary>
        /// работник иной уполномоченной организации 
        /// </summary>
        [Display(Description = "3 - работник иной уполномоченной организации")]
        EmployeeOfAnotherAuthorizedOrganization,

        /// <summary>
        /// уполномоченное физическое лицо, в том числе индивидуальный предприниматель
        /// </summary>
        [Display(Description = "4 - уполномоченное физическое лицо, в том числе индивидуальный предприниматель")]
        Individual
    }
}
