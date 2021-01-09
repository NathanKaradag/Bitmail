using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitmail.Models
{
	public class CampaignHistory
	{
		public int Id { get; set; }
		public string MailChimpId { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string SubjectLine { get; set; }
		public DateTime Date { get; set; }
		public string HTML { get; set; }
		public string Contacts { get; set; }
		public string Tags { get; set; }
	}
}