using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using UtilitesLibrary.Service;
using System.ComponentModel.DataAnnotations;

namespace Reporter.Controls.Base
{
    public class ReportControlBase : UserControl
    {
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
        }

        public virtual IReport Report { get; }

        protected virtual Dictionary<TEnum, string> GetEnumValues<TEnum>()
        {
            var names = typeof(TEnum).GetEnumValues();

            var dictionary = new Dictionary<TEnum, string>();
            foreach (var name in names)
            {
                var member = typeof(TEnum).GetMembers().SingleOrDefault(x => x.Name == name.ToString());
                var descAttribute = member.GetCustomAttributes(typeof(DisplayAttribute), false)?.FirstOrDefault();
                var nameStr = ((DisplayAttribute)descAttribute).Description;
                dictionary.Add((TEnum)name, nameStr);
            }

            return dictionary;
        }
    }
}
