﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// Этот исходный код был создан с помощью xsd, версия=4.6.1055.0.
// 
namespace Reporter.XsdClasses.DpUvutoch
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Файл
    {

        private ФайлДокумент документField;

        private string идФайлField;

        private string версПрогField;

        private ФайлВерсФорм версФормField;

        /// <remarks/>
        public ФайлДокумент Документ
        {
            get {
                return this.документField;
            }
            set {
                this.документField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ИдФайл
        {
            get {
                return this.идФайлField;
            }
            set {
                this.идФайлField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ВерсПрог
        {
            get {
                return this.версПрогField;
            }
            set {
                this.версПрогField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ФайлВерсФорм ВерсФорм
        {
            get {
                return this.версФормField;
            }
            set {
                this.версФормField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ФайлДокумент
    {

        private УчастЭДОТип участЭДОField;

        private ФайлДокументСвУведУточ свУведУточField;

        private УчастЭДОТип отпрДокField;

        private ПодписантТип подписантField;

        private ФайлДокументКНД кНДField;

        /// <remarks/>
        public УчастЭДОТип УчастЭДО
        {
            get {
                return this.участЭДОField;
            }
            set {
                this.участЭДОField = value;
            }
        }

        /// <remarks/>
        public ФайлДокументСвУведУточ СвУведУточ
        {
            get {
                return this.свУведУточField;
            }
            set {
                this.свУведУточField = value;
            }
        }

        /// <remarks/>
        public УчастЭДОТип ОтпрДок
        {
            get {
                return this.отпрДокField;
            }
            set {
                this.отпрДокField = value;
            }
        }

        /// <remarks/>
        public ПодписантТип Подписант
        {
            get {
                return this.подписантField;
            }
            set {
                this.подписантField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ФайлДокументКНД КНД
        {
            get {
                return this.кНДField;
            }
            set {
                this.кНДField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class УчастЭДОТип
    {

        private object itemField;

        private string идУчастЭДОField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ИП", typeof(ФЛТип))]
        [System.Xml.Serialization.XmlElementAttribute("ЮЛ", typeof(ЮЛТип))]
        public object Item
        {
            get {
                return this.itemField;
            }
            set {
                this.itemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ИдУчастЭДО
        {
            get {
                return this.идУчастЭДОField;
            }
            set {
                this.идУчастЭДОField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ФЛТип
    {

        private ФИОТип фИОField;

        private string иННФЛField;

        /// <remarks/>
        public ФИОТип ФИО
        {
            get {
                return this.фИОField;
            }
            set {
                this.фИОField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ИННФЛ
        {
            get {
                return this.иННФЛField;
            }
            set {
                this.иННФЛField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ФИОТип
    {

        private string фамилияField;

        private string имяField;

        private string отчествоField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Фамилия
        {
            get {
                return this.фамилияField;
            }
            set {
                this.фамилияField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Имя
        {
            get {
                return this.имяField;
            }
            set {
                this.имяField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Отчество
        {
            get {
                return this.отчествоField;
            }
            set {
                this.отчествоField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ПодписантТип
    {

        private ФИОТип фИОField;

        private string должностьField;

        /// <remarks/>
        public ФИОТип ФИО
        {
            get {
                return this.фИОField;
            }
            set {
                this.фИОField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Должность
        {
            get {
                return this.должностьField;
            }
            set {
                this.должностьField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ДанПолучДокТип
    {

        private string наимДокField;

        private string номСФField;

        private string датаСФField;

        private string номИспрСФField;

        private string датаИспрСФField;

        private string номКСФField;

        private string датаКСФField;

        private string номИспрКСФField;

        private string датаИспрКСФField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string НаимДок
        {
            get {
                return this.наимДокField;
            }
            set {
                this.наимДокField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string НомСФ
        {
            get {
                return this.номСФField;
            }
            set {
                this.номСФField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ДатаСФ
        {
            get {
                return this.датаСФField;
            }
            set {
                this.датаСФField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string НомИспрСФ
        {
            get {
                return this.номИспрСФField;
            }
            set {
                this.номИспрСФField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ДатаИспрСФ
        {
            get {
                return this.датаИспрСФField;
            }
            set {
                this.датаИспрСФField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string НомКСФ
        {
            get {
                return this.номКСФField;
            }
            set {
                this.номКСФField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ДатаКСФ
        {
            get {
                return this.датаКСФField;
            }
            set {
                this.датаКСФField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string НомИспрКСФ
        {
            get {
                return this.номИспрКСФField;
            }
            set {
                this.номИспрКСФField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ДатаИспрКСФ
        {
            get {
                return this.датаИспрКСФField;
            }
            set {
                this.датаИспрКСФField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ЮЛТип
    {

        private string наимОргField;

        private string иННЮЛField;

        private string кППField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string НаимОрг
        {
            get {
                return this.наимОргField;
            }
            set {
                this.наимОргField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ИННЮЛ
        {
            get {
                return this.иННЮЛField;
            }
            set {
                this.иННЮЛField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string КПП
        {
            get {
                return this.кППField;
            }
            set {
                this.кППField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ФайлДокументСвУведУточ
    {

        private ФайлДокументСвУведУточСведПолФайл сведПолФайлField;

        private string текстУведУточField;

        private ДанПолучДокТип данПолучДокField;

        private string датаПолField;

        private string времяПолField;

        /// <remarks/>
        public ФайлДокументСвУведУточСведПолФайл СведПолФайл
        {
            get {
                return this.сведПолФайлField;
            }
            set {
                this.сведПолФайлField = value;
            }
        }

        /// <remarks/>
        public string ТекстУведУточ
        {
            get {
                return this.текстУведУточField;
            }
            set {
                this.текстУведУточField = value;
            }
        }

        /// <remarks/>
        public ДанПолучДокТип ДанПолучДок
        {
            get {
                return this.данПолучДокField;
            }
            set {
                this.данПолучДокField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ДатаПол
        {
            get {
                return this.датаПолField;
            }
            set {
                this.датаПолField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ВремяПол
        {
            get {
                return this.времяПолField;
            }
            set {
                this.времяПолField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ФайлДокументСвУведУточСведПолФайл
    {

        private string эЦППолФайлField;

        private string имяПостФайлаField;

        /// <remarks/>
        public string ЭЦППолФайл
        {
            get {
                return this.эЦППолФайлField;
            }
            set {
                this.эЦППолФайлField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ИмяПостФайла
        {
            get {
                return this.имяПостФайлаField;
            }
            set {
                this.имяПостФайлаField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public enum ФайлДокументКНД
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1115113")]
        Item1115113,
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public enum ФайлВерсФорм
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1.01")]
        Item101,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("1.02")]
        Item102,
    }
}