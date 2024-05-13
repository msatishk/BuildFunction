using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ToIVUMultipleFromOracle.Models
{
    [XmlRoot(ElementName = "address")]
    public class Address
    {
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "street")]
        public string Street { get; set; }
        [XmlElement(ElementName = "co")]
        public string Co { get; set; }
        [XmlElement(ElementName = "postalCode")]
        public string PostalCode { get; set; }
        [XmlElement(ElementName = "town")]
        public string Town { get; set; }
        [XmlElement(ElementName = "district")]
        public string District { get; set; }
        [XmlElement(ElementName = "country")]
        public string Country { get; set; }
    }

    [XmlRoot(ElementName = "telephoneNumber")]
    public class TelephoneNumber
    {
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "number")]
        public string Number { get; set; }
    }

    [XmlRoot(ElementName = "emailAddress")]
    public class EmailAddress
    {
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "address")]
        public string Address { get; set; }
    }

    [XmlRoot(ElementName = "personnelData")]
    public class PersonnelData
    {
        [XmlElement(ElementName = "personnelNumber")]
        public string PersonnelNumber { get; set; }
        [XmlElement(ElementName = "firstName")]
        public string FirstName { get; set; }
        [XmlElement(ElementName = "lastName")]
        public string LastName { get; set; }
        [XmlElement(ElementName = "dateOfBirth")]
        public DateTime DateOfBirth { get; set; }
        [XmlElement(ElementName = "gender")]
        public string Gender { get; set; }
        [XmlElement(ElementName = "address")]
        public List<Address> Address { get; set; }
        [XmlElement(ElementName = "telephoneNumber")]
        public List<TelephoneNumber> TelephoneNumber { get; set; }
        [XmlElement(ElementName = "emailAddress")]
        public List<EmailAddress> EmailAddress { get; set; }
        [XmlElement(ElementName = "comment")]
        public string Comment { get; set; }
        [XmlElement(ElementName = "cardNumber")]
        public long CardNumber { get; set; }
        [XmlElement(ElementName = "ignoreExistingComment")]
        public bool IgnoreExistingComment { get; set; }
        [XmlElement(ElementName = "title")]
        public string Title { get; set; }
        [XmlElement(ElementName = "formOfAddress")]
        public string FormOfAddress { get; set; }

        [XmlElement(ElementName = "cardNumberSpecified")]
        public bool CardNumberSpecified { get; set; }
        [XmlElement(ElementName = "ignoreExistingCommentSpecified")]
        public bool IgnoreExistingCommentSpecified { get; set; }
    }

    [XmlRoot(ElementName = "importPersonnelDataRequest")]
    public class ImportPersonnelDataRequest 
    {
        [XmlElement(ElementName = "personnelData")]
        public List<PersonnelData> PersonnelData { get; set; }
    }


}
