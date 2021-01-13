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
		[Required(ErrorMessage = "Label moet een titel hebben")]
		public string Title { get; set; }
		public string MailChimpTagId { get; set; }
		public List<ContactTag> ContactTags { get; set; }
	}

}