using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitmail.Models
{
    public class OrganisationContact
    {
        public int Id { get; set; }
        public int OrganisationId { get; set; }
        public Organisation Organisation { get; set; }
        public int ContactId { get; set; }
        public Contact Contact { get; set; }
    }
}