using System.Collections.Generic;
using System.Linq;
using DataContextManagementUnit.DataAccess.Contexts.Abt;

namespace HonestMarkSystem.Models
{
    public class DocJournalForLoading
    {
        private object _docEdoReturnPurchasing;

        public DocJournal Item { get; set; }
        public object DocEdoReturnPurchasing
        {
            set {
                _docEdoReturnPurchasing = value;
            }
            get {
                if (_docEdoReturnPurchasing != null)
                {
                    if (_docEdoReturnPurchasing as IQueryable<DocEdoReturnPurchasing> != null)
                    {
                        var query = _docEdoReturnPurchasing as IQueryable<DocEdoReturnPurchasing>;

                        if (query.Any(s => s.DocStatus == 2))
                            _docEdoReturnPurchasing = query.FirstOrDefault(s => s.DocStatus == 2);
                        else if (query.Any(s => s.DocStatus == 1))
                            _docEdoReturnPurchasing = query.FirstOrDefault(s => s.DocStatus == 1);
                        else
                            _docEdoReturnPurchasing = query.FirstOrDefault();
                    }
                    else if (_docEdoReturnPurchasing as IEnumerable<DocEdoReturnPurchasing> != null)
                    {
                        var collection = _docEdoReturnPurchasing as IEnumerable<DocEdoReturnPurchasing>;

                        if (collection.Any(s => s.DocStatus == 2))
                            _docEdoReturnPurchasing = collection.FirstOrDefault(s => s.DocStatus == 2);
                        else if (collection.Any(s => s.DocStatus == 1))
                            _docEdoReturnPurchasing = collection.FirstOrDefault(s => s.DocStatus == 1);
                        else
                            _docEdoReturnPurchasing = collection.FirstOrDefault();
                    }
                    else if (_docEdoReturnPurchasing as DocEdoReturnPurchasing == null)
                        return null;
                }

                return _docEdoReturnPurchasing;
            }
        }

        public int ReturnStatus
        {
            get {
                if(DocEdoReturnPurchasing != null)
                {
                    return (_docEdoReturnPurchasing as DocEdoReturnPurchasing)?.DocStatus ?? 0;
                }

                return 0;
            }
        }

        public string ResultStatusName
        {
            get {
                if (ReturnStatus == 0)
                    return "-";
                else
                    return (_docEdoReturnPurchasing as DocEdoReturnPurchasing)?.Status?.Name;
            }
        }
    }
}
