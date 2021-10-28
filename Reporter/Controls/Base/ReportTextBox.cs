using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpf.Editors;

namespace Reporter.Controls.Base
{
    public class ReportTextBox : TextEdit
    {
        public string Expression { get; set; }

        public string FieldName { get; set; }

        public bool IsRequired { get; set; }

        public string ErrorMessage { get; set; }
    }
}
