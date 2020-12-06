using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bitmail.Models
{
    public class Organisation
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Organisatie moet een naam bezitten")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Organisatie moet een beschrijving bezitten")]
        public string Description { get; set; }
        public List<OrganisationContact> OrganisationContacts { get; set; }
    }
}