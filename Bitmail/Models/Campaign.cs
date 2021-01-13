using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bitmail.Models
{
	public class Campaign
	{
		public int Id { get; set; }
		public string ListId { get; set; }
		public string MailChimpId { get; set; }
		public string TemplateId { get; set; }
		[Required(ErrorMessage = "Mail moet een beschrijving bezitten")]
		public string Description { get; set; }
		[Required(ErrorMessage = "Mail moet een onderwerp bezitten")]
		public string SubjectLine { get; set; }
		[Required(ErrorMessage = "Mail moet een voorbeeldtext bezitten")]
		public string PreviewText { get; set; }
		[Required(ErrorMessage = "Mail moet een titel bezitten")]
		public string Title { get; set; }
		[Required(ErrorMessage = "Mail moet een afzender bezitten")]
		public string FromName { get; set; }
		[Required(ErrorMessage = "Mail moet een antwoord adres bezitten")]
		public string ReplyTo { get; set; }
		public string ToName { get; set; }
		public string HTML { get; set; }
		public string Status { get; set; }
		public DateTime Date { get; set; }
		protected Dictionary<string, string> Sections { get; set; }

		public int CampaignTagsId { get; set; }
		public List<CampaignTag> CampaignTags { get; set; }
	}
}