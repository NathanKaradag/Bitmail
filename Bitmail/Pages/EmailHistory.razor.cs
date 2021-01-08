using Bitmail.Models;
using Bitmail.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitmail.Pages
{
	public partial class EmailHistory
	{
		[Inject] private DatabaseService DatabaseService { get; set; }
		private Campaign CurrentCampaign { get; set; }
		private List<Campaign> Campaigns { get; set; }

		protected override async Task OnInitializedAsync()
		{
			Campaigns = await DatabaseService.DB.CampaignHistory.ToListAsync();
		}

		private async Task OnCampaignClicked(Campaign item)
		{
		}
	}
}