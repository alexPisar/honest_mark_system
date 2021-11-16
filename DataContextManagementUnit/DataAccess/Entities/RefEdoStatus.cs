using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContextManagementUnit.DataAccess.Contexts.Abt
{
    public partial class RefEdoStatus
    {
        public RefEdoStatus()
        {
            OnCreated();
        }

        #region Properties
        public virtual int Id { get; set; }

        public virtual string Name { get; set; }
        #endregion

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }
}
