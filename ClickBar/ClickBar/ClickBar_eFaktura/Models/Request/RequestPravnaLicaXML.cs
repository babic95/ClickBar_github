using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_eFaktura.Models.Request
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://www.w3.org/2003/05/soap-envelope", IsNullable = false)]
    public partial class Envelope
    {

        private EnvelopeHeader headerField;

        private EnvelopeBody bodyField;

        /// <remarks/>
        public EnvelopeHeader Header
        {
            get
            {
                return headerField;
            }
            set
            {
                headerField = value;
            }
        }

        /// <remarks/>
        public EnvelopeBody Body
        {
            get
            {
                return bodyField;
            }
            set
            {
                bodyField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    public partial class EnvelopeHeader
    {

        private AuthenticationHeader authenticationHeaderField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://communicationoffice.nbs.rs")]
        public AuthenticationHeader AuthenticationHeader
        {
            get
            {
                return authenticationHeaderField;
            }
            set
            {
                authenticationHeaderField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://communicationoffice.nbs.rs")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://communicationoffice.nbs.rs", IsNullable = false)]
    public partial class AuthenticationHeader
    {

        private string userNameField;

        private string passwordField;

        private string licenceIDField;

        /// <remarks/>
        public string UserName
        {
            get
            {
                return userNameField;
            }
            set
            {
                userNameField = value;
            }
        }

        /// <remarks/>
        public string Password
        {
            get
            {
                return passwordField;
            }
            set
            {
                passwordField = value;
            }
        }

        /// <remarks/>
        public string LicenceID
        {
            get
            {
                return licenceIDField;
            }
            set
            {
                licenceIDField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    public partial class EnvelopeBody
    {

        private GetCompany getCompanyField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(Namespace = "http://communicationoffice.nbs.rs")]
        public GetCompany GetCompany
        {
            get
            {
                return getCompanyField;
            }
            set
            {
                getCompanyField = value;
            }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://communicationoffice.nbs.rs")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://communicationoffice.nbs.rs", IsNullable = false)]
    public partial class GetCompany
    {

        private string taxIdentificationNumberField;

        /// <remarks/>
        public string taxIdentificationNumber
        {
            get
            {
                return taxIdentificationNumberField;
            }
            set
            {
                taxIdentificationNumberField = value;
            }
        }
    }
}

