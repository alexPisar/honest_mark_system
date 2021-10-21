using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
internal partial class ФайлСвУчДокОбор
{

    private ФайлСвУчДокОборСвОЭДОтпр свОЭДОтпрField;

    private string идОтпрField;

    private string идПолField;

    /// <remarks/>
    public ФайлСвУчДокОборСвОЭДОтпр СвОЭДОтпр
    {
        get {
            return this.свОЭДОтпрField;
        }
        set {
            this.свОЭДОтпрField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ИдОтпр
    {
        get {
            return this.идОтпрField;
        }
        set {
            this.идОтпрField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ИдПол
    {
        get {
            return this.идПолField;
        }
        set {
            this.идПолField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
internal partial class ФайлСвУчДокОборСвОЭДОтпр
{

    private string наимОргField;

    private string иННЮЛField;

    private string идЭДОField;

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
    public string ИдЭДО
    {
        get {
            return this.идЭДОField;
        }
        set {
            this.идЭДОField = value;
        }
    }
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
internal partial class СвИПТип
{

    private ФИОТип фИОField;

    private string иННФЛField;

    private string свГосРегИПField;

    private string иныеСведField;

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

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string СвГосРегИП
    {
        get {
            return this.свГосРегИПField;
        }
        set {
            this.свГосРегИПField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ИныеСвед
    {
        get {
            return this.иныеСведField;
        }
        set {
            this.иныеСведField = value;
        }
    }
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
internal partial class ФИОТип
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

[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
internal partial class СвФЛТип
{

    private ФИОТип фИОField;

    private string иННФЛField;

    private string иныеСведField;

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

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ИныеСвед
    {
        get {
            return this.иныеСведField;
        }
        set {
            this.иныеСведField = value;
        }
    }
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
internal enum ФайлВерсФорм
{

    /// <remarks/>
    [System.Xml.Serialization.XmlEnumAttribute("5.01")]
    Item501,
}
