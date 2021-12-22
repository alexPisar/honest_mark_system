using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitesLibrary.ModelBase;
using DataContextManagementUnit.DataAccess.Contexts.Abt;

namespace HonestMarkSystem.Models
{
    public class RefGoodsModel: ListViewModel<RefGood>
    {
        public RefGoodsModel(IEnumerable<RefGood> refGoods)
        {
            ItemsList = new System.Collections.ObjectModel.ObservableCollection<RefGood>(refGoods);
        }
    }
}
