using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bitmail.Models
{
	public class Tag
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string MailChimpTagId { get; set; }
		public List<ContactTag> ContactTags { get; set; }
	}

}