using Bitmail.Models;
using Bitmail.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bitmail.Pages
{
	public partial class EmailHistory
	{
		[Inject] private DatabaseService DatabaseService { get; set; }
		private CampaignHistory CurrentCampaign { get; set; }
		private List<CampaignHistory> Campaigns { get; set; }
		private List<CampaignHistory> AllCampaigns { get; set; }
		[Parameter] public string Id { get; set; }
		protected override async Task OnInitializedAsync()
		{
			await DatabaseService.DB.Campaigns.ToListAsync();
			await DatabaseService.DB.CampaignTags.Include(ct => ct.Tag).ToListAsync();
			await DatabaseService.DB.ContactTags.ToListAsync();
			await DatabaseService.DB.Contacts.ToListAsync();

			AllCampaigns = await DatabaseService.DB.CampaignHistory.OrderByDescending(ch=>ch.Date).ToListAsync();
			Campaigns = AllCampaigns;
			if (!string.IsNullOrEmpty(Id))
			{
				var idInt = Convert.ToInt32(Id);
				var selectedCampaign = AllCampaigns.FirstOrDefault(c => c.Id == idInt);
				await OnCampaignClicked(selectedCampaign);
			}
		}
		protected void OnSearch(object value)
		{
			if (value != null && !string.IsNullOrEmpty(value.ToString()))
			{
				Campaigns = AllCampaigns.Where(c =>
					(c.Title != null) ? c.Title.ToLower().Contains(value.ToString().ToLower()) : false).ToList();
			}
			else
			{
				Campaigns = AllCampaigns;
			}

			StateHasChanged();
		}
		private async Task OnCampaignClicked(CampaignHistory item)
		{
			Regex rRemScript = new Regex(@"<script[^>]*>[\s\S]*?</script>");
			var output = rRemScript.Replace(item.HTML, "");
			item.HTML = output;
			CurrentCampaign = item;

			StateHasChanged();
		}

		protected async Task RemoveHistory(CampaignHistory campaignHistory)
		{
			DatabaseService.DB.Remove(campaignHistory);
			await DatabaseService.DB.SaveChangesAsync();
			await DatabaseService.DB.Campaigns.ToListAsync();
			await DatabaseService.DB.CampaignTags.Include(ct => ct.Tag).ToListAsync();
			await DatabaseService.DB.ContactTags.ToListAsync();
			await DatabaseService.DB.Contacts.ToListAsync();

			AllCampaigns = await DatabaseService.DB.CampaignHistory.ToListAsync();
			Campaigns = AllCampaigns;
			CurrentCampaign = null;
			StateHasChanged();
		}
	}
}