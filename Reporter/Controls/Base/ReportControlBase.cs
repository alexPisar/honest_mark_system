using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using UtilitesLibrary.Service;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Text.RegularExpressions;

namespace Reporter.Controls.Base
{
    public class ReportControlBase : UserControl
    {
        protected virtual string UrlXmlValidation { get; }

        private XmlValidation _validationXmlObj = new XmlValidation();

        public List<string> ErrorsValidationReport { get; set; }

        public List<string> ErrorsValidationXml => _validationXmlObj.Errors;
        public List<string> WarningsValidationXml => _validationXmlObj.Warnings;

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
        }

        private bool ValidationControl(object control, ScriptOptions options)
        {
            var panelControl = control as Panel;
            if(panelControl != null)
            {
                bool result = true;

                foreach (var p in panelControl.Children)
                    result = ValidationControl(p, options) && result;

                return result;
            }

            var contentControl = control as ContentControl;
            if(contentControl != null)
            {
                return ValidationControl(contentControl.Content, options);
            }

            if (control.GetType() == typeof(ReportComboBox))
            {
                var repComboBox = control as ReportComboBox;
                int? value = null;

                if (repComboBox.SelectedValue != null)
                    value = (int)repComboBox.SelectedValue;

                if (repComboBox.IsRequired && (value == null || value == 0))
                {
                    repComboBox.Background = System.Windows.Media.Brushes.Pink;
                    ErrorsValidationReport.Add($"Не заполнено поле '{repComboBox.FieldName}'");
                    return false;
                }

                if (!string.IsNullOrEmpty(repComboBox.Expression))
                {
                    bool result = CSharpScript.EvaluateAsync<bool>(repComboBox.Expression, options, Report).Result;

                    if (!result)
                        ErrorsValidationReport.Add(repComboBox.ErrorMessage);

                    return result;
                }

                return true;
            }

            if(control.GetType() == typeof(ReportSwitchTabControl))
            {
                var reportSwitchControl = control as ReportSwitchTabControl;
                return ValidationControl(reportSwitchControl.SelectedItem, options);
            }

            var itemsControl = control as ItemsControl;
            if(itemsControl != null)
            {
                bool result = true;

                foreach (var item in itemsControl.Items)
                    result = ValidationControl(item, options) && result;

                return result;
            }

            if(control.GetType() == typeof(ReportTextBox))
            {
                var repTextBox = control as ReportTextBox;

                if(repTextBox.IsRequired && string.IsNullOrEmpty(repTextBox.Text))
                {
                    repTextBox.Background = System.Windows.Media.Brushes.Pink;
                    ErrorsValidationReport.Add($"Не заполнено поле '{repTextBox.FieldName}'");
                    return false;
                }

                if (!string.IsNullOrEmpty(repTextBox.Mask))
                {
                    var result = Regex.IsMatch(repTextBox.Text ?? "", repTextBox.Mask);

                    if (!result)
                        ErrorsValidationReport.Add($"Значение поля '{repTextBox.FieldName}' не соответствует формату.");

                    return result;
                }

                if (!string.IsNullOrEmpty(repTextBox.Expression))
                {
                    bool result = CSharpScript.EvaluateAsync<bool>(repTextBox.Expression, options, Report).Result;

                    if (!result)
                        ErrorsValidationReport.Add(repTextBox.ErrorMessage);

                    return result;
                }

                return true;
            }

            return true;
        }

        public virtual IReport Report { get; }

        public virtual bool ValidateReport()
        {
            ErrorsValidationReport = new List<string>();
            var options = ScriptOptions.Default.WithImports(nameof(Reporter)).AddReferences(this.GetType().Assembly);
            options = options.AddImports("Reporter.Enums").AddImports("Reporter.Reports");

            var contentControl = this.Content;

            return ValidationControl(contentControl, options);
        }

        public bool ValidateXml()
        {
            var xml = Report.GetXmlContent();

            var xmlStream = new System.IO.StringReader(xml);
            return _validationXmlObj.ValidationXmlByXsd(xmlStream, UrlXmlValidation);
        }

        public virtual void SetDefaults()
        {

        }
    }
}
