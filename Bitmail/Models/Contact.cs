using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitmail.Models
{
    public class Contact
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public List<ContactTag> ContactTags { get; set; }
        public List<OrganisationContact> OrganisationContacts { get; set; }
    }
}