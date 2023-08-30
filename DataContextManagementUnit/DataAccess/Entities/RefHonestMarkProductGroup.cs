using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContextManagementUnit.DataAccess.Contexts.Abt
{
    public partial class RefHonestMarkProductGroup
    {
        public RefHonestMarkProductGroup()
        {
            OnCreated();
        }

        #region Properties
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
        #endregion

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }
}
