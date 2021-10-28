using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.ComponentModel.DataAnnotations;
using System.Windows.Data;

namespace Reporter.Controls.Base
{
    public class ReportComboBox : ComboBox
    {
        private Type _enumType;

        public ReportComboBox() : base()
        {
            
        }

        public void Init()
        {
            Items = GetEnumValues();
            ItemsSource = Items;
            DisplayMemberPath = "Value";
            SelectedValuePath = "Key";
        }

        public new List<KeyValuePair<object, string>> Items { get; set; }
        public Type EnumType
        {
            get {
                return _enumType;
            }
            set {
                _enumType = value;
                Init();
            }
        }

        public string Expression { get; set; }

        public string FieldName { get; set; }

        public bool IsRequired { get; set; }

        public string ErrorMessage { get; set; }

        private List<KeyValuePair<object, string>> GetEnumValues()
        {
            var values = _enumType.GetEnumValues();

            var list = new List<KeyValuePair<object, string>>();
            foreach (var value in values)
            {
                var member = _enumType.GetMembers().SingleOrDefault(x => x.Name == value.ToString());
                var descAttribute = member.GetCustomAttributes(typeof(DisplayAttribute), false)?.FirstOrDefault();
                var nameStr = ((DisplayAttribute)descAttribute).Description;
                list.Add(new KeyValuePair<object, string>(value, nameStr));
            }

            return list;
        }
    }
}
