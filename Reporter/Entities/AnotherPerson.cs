using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Entities
{
    public class AnotherPerson
    {
        /// <summary>
        /// Представитель организации, которой доверено принятие товаров (груза)
        /// </summary>
        public OrganizationRepresentative OrganizationRepresentative { get; set; }

        /// <summary>
        /// Физическое лицо, которому доверено принятие товаров (груза)
        /// </summary>
        public TrustedIndividual TrustedIndividual { get; set; }
    }
}
