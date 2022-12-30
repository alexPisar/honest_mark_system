using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HonestMarkSystem.Implementations
{
    public class LoadActionData
    {
        public List<object> InputData { get; set; }
        public string ErrorMessage { get; set; }
        public string TitleErrorText { get; set; }
        public List<string> Errors { get; set; }
    }
}
