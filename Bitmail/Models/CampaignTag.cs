using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitmail.Models
{
    public class CampaignTag
    {
        public int TagId { get; set; }
        public Tag Tag { get; set; }
        public int CampaignId { get; set; }
        public Campaign Campaign { get; set; }
    }
}