using System;
using System.Collections.Generic;
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
        public string Description { get; set; }
        public string SubjectLine { get; set; }
        public string PreviewText { get; set; }
        public string Title { get; set; }
        public string FromName { get; set; }
        public string ReplyTo { get; set; }
        public string ToName { get; set; }
        public string Status { get; set; }
        public DateTime Date{ get; set; }
        protected Dictionary<string, string> Sections { get; set; }

        public int CampaignTagsId { get; set; }
        public List<CampaignTag> CampaignTags { get; set; }
    }
}