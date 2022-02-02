using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using UtilitesLibrary.ModelBase;
using DataContextManagementUnit.DataAccess.Contexts.Abt;

namespace HonestMarkSystem.Models
{
    public class MarkedCodesModel : ListViewModel<DocGoodsDetailsLabels>
    {
        public List<DocGoodsDetailsLabels> SelectedItems { get; set; }

        public MarkedCodesModel(List<DocGoodsDetailsLabels> markedCodes)
        {
            ItemsList = new ObservableCollection<DocGoodsDetailsLabels>(markedCodes);
            SelectedItems = new List<DocGoodsDetailsLabels>();
        }
    }
}
