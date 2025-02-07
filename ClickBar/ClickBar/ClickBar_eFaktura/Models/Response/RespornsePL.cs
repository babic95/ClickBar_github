using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickBar_eFaktura.Models.Response
{
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class CompanyDataSet
    {

        private CompanyDataSetCompany companyField;

        /// <remarks/>
        public CompanyDataSetCompany Company
        {
            get
            {
                return this.companyField;
            }
            set
            {
                this.companyField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class CompanyDataSetCompany
    {

        private string companyIDField;

        private int nationalIdentificationNumberField;

        private int taxIdentificationNumberField;

        private string nameField;

        private object shortNameField;

        private string addressField;

        private string cityField;

        private string municipalityField;

        private string regionField;

        private string postalCodeField;

        private string activityNameField;

        private System.DateTime registrationDateField;

        private System.DateTime foundingDateField;

        private byte companyTypeIDField;

        private byte companyStatusIDField;

        private System.DateTime nBSDateField;

        private byte resourceField;

        /// <remarks/>
        public string CompanyID
        {
            get
            {
                return this.companyIDField;
            }
            set
            {
                this.companyIDField = value;
            }
        }

        /// <remarks/>
        public int NationalIdentificationNumber
        {
            get
            {
                return this.nationalIdentificationNumberField;
            }
            set
            {
                this.nationalIdentificationNumberField = value;
            }
        }

        /// <remarks/>
        public int TaxIdentificationNumber
        {
            get
            {
                return this.taxIdentificationNumberField;
            }
            set
            {
                this.taxIdentificationNumberField = value;
            }
        }

        /// <remarks/>
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public object ShortName
        {
            get
            {
                return this.shortNameField;
            }
            set
            {
                this.shortNameField = value;
            }
        }

        /// <remarks/>
        public string Address
        {
            get
            {
                return this.addressField;
            }
            set
            {
                this.addressField = value;
            }
        }

        /// <remarks/>
        public string City
        {
            get
            {
                return this.cityField;
            }
            set
            {
                this.cityField = value;
            }
        }

        /// <remarks/>
        public string Municipality
        {
            get
            {
                return this.municipalityField;
            }
            set
            {
                this.municipalityField = value;
            }
        }

        /// <remarks/>
        public string Region
        {
            get
            {
                return this.regionField;
            }
            set
            {
                this.regionField = value;
            }
        }

        /// <remarks/>
        public string PostalCode
        {
            get
            {
                return this.postalCodeField;
            }
            set
            {
                this.postalCodeField = value;
            }
        }

        /// <remarks/>
        public string ActivityName
        {
            get
            {
                return this.activityNameField;
            }
            set
            {
                this.activityNameField = value;
            }
        }

        /// <remarks/>
        public System.DateTime RegistrationDate
        {
            get
            {
                return this.registrationDateField;
            }
            set
            {
                this.registrationDateField = value;
            }
        }

        /// <remarks/>
        public System.DateTime FoundingDate
        {
            get
            {
                return this.foundingDateField;
            }
            set
            {
                this.foundingDateField = value;
            }
        }

        /// <remarks/>
        public byte CompanyTypeID
        {
            get
            {
                return this.companyTypeIDField;
            }
            set
            {
                this.companyTypeIDField = value;
            }
        }

        /// <remarks/>
        public byte CompanyStatusID
        {
            get
            {
                return this.companyStatusIDField;
            }
            set
            {
                this.companyStatusIDField = value;
            }
        }

        /// <remarks/>
        public System.DateTime NBSDate
        {
            get
            {
                return this.nBSDateField;
            }
            set
            {
                this.nBSDateField = value;
            }
        }

        /// <remarks/>
        public byte Resource
        {
            get
            {
                return this.resourceField;
            }
            set
            {
                this.resourceField = value;
            }
        }
    }
}
