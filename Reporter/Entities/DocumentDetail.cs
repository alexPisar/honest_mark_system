using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter.Entities
{
    public class DocumentDetail : Base.IReportEntity<DocumentDetail>
    {
        /// <summary>
        /// Наименование документа - основания
        /// </summary>
        public string DocName { get; set; }

        /// <summary>
        /// Номер документа - основания
        /// </summary>
        public string DocNumber { get; set; }

        /// <summary>
        /// Дата документа - основания
        /// </summary>
        public DateTime? DocDate { get; set; }

        /// <summary>
        /// Дополнительные сведения
        /// </summary>
        public string OtherInfo { get; set; }

        /// <summary>
        /// Идентификатор файла основания
        /// </summary>
        public string FileId { get; set; }
    }
}
