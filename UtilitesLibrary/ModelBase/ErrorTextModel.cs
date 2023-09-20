using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitesLibrary.ModelBase
{
    public class ErrorTextModel
    {
        public ErrorTextModel() { }
        public ErrorTextModel(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public string ErrorMessage { get; set; }
        public List<string> Errors { get; set; }
        public Exception Exception { get; set; } = null;
    }
}
