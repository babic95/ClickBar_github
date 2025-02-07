
namespace ClickBar_eFaktura.Models.Response
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.w3.org/2003/05/soap-envelope", IsNullable = false)]
    public partial class Envelope
    {

        private EnvelopeBody bodyField;

        /// <remarks/>
        public EnvelopeBody Body
        {
            get
            {
                return this.bodyField;
            }
            set
            {
                this.bodyField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    public partial class EnvelopeBody
    {

        private GetCompanyResponse getCompanyResponseField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://communicationoffice.nbs.rs")]
        public GetCompanyResponse GetCompanyResponse
        {
            get
            {
                return this.getCompanyResponseField;
            }
            set
            {
                this.getCompanyResponseField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://communicationoffice.nbs.rs")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://communicationoffice.nbs.rs", IsNullable = false)]
    public partial class GetCompanyResponse
    {

        private string getCompanyResultField;

        /// <remarks/>
        public string GetCompanyResult
        {
            get
            {
                return this.getCompanyResultField;
            }
            set
            {
                this.getCompanyResultField = value;
            }
        }
    }
}
