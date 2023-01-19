using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContextManagementUnit.DataAccess
{
    public enum DocJournalActStatus
    {
        New = 0,
        Reserve,
        WareHouse,
        Selection,
        Selected,
        Exported,
        Confirmed
    }
}
