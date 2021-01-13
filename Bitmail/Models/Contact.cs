using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bitmail.Models
{
    public class Contact
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Contact moet een voornaam hebben")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Contact moet een achternaam hebben")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Contact moet een e-mailadres hebben")]
        public string Email { get; set; }
        public List<ContactTag> ContactTags { get; set; }
        public List<OrganisationContact> OrganisationContacts { get; set; }
    }
}