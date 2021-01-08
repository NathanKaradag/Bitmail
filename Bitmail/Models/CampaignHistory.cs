using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitmail.Models
{
	public class CampaignHistory
	{
		public int Id { get; set; }
		public int CampaignId { get; set; }
		public Campaign Campaign { get; set; }
		public string HTML { get; set; }
	}
}