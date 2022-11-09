using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HonestMarkSystem.Interfaces
{
    public interface ITreeListView
    {
        void SetItems(DevExpress.Xpf.Grid.TreeListNodeCollection nodeCollection);
        void ProcessingSelectedItems(IEnumerable<DevExpress.Xpf.Grid.TreeListNode> selectedNodes);
    }
}
