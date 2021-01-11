using Bitmail.Models;
using Bitmail.Services;
using MailChimpWrapper.Models;
using MailChimpWrapper.Models.Exceptions;
using MailChimpWrapper.Models.Requests;
using MailChimpWrapper.Models.Responses;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitmail.Pages
{
	public partial class EmailStatistics
	{
		[Inject] private DatabaseService DatabaseService { get; set; }
		[Inject] private MailChimpService MailChimpService { get; set; }
		private Dictionary<string, ReportData> ReportsPerCampaign { get; set; } = new Dictionary<string, ReportData>();
		private List<CampaignHistory> Campaigns { get; set; }

		protected override async Task OnInitializedAsync()
		{
			Campaigns = await DatabaseService.DB.CampaignHistory.OrderByDescending(ch => ch.Date).Take(10).ToListAsync();

			//Discard the call so it disconnects from the main thread and runs asynchronously
			_ = GetReports(Campaigns);
		}

		private async Task GetReports(List<CampaignHistory> campaigns)
		{
			foreach (var item in campaigns)
			{
				if (!string.IsNullOrEmpty(item.MailChimpId))
				{
					try
					{
						CampaignReportResponse resp = await MailChimpService.Client.Request<CampaignReportGetRequest, CampaignReportResponse>(new CampaignReportGetRequest(item.MailChimpId));
						if (resp != null)
						{
							ReportsPerCampaign[item.MailChimpId] = new ReportData() { Clicks = resp.Clicks, Opens = resp.Opens, Bounces = resp.Bounces, Forwards = resp.Forwards, Title = resp.CampaignTitle, EmailsSent = resp.EmailsSent, SentTime = resp.SendTime };
							StateHasChanged();
						}
					}
					catch (ResponseException responseException)
					{
						var message = responseException.ErrorResponse.Detail;
						ReportsPerCampaign[item.MailChimpId] = new ReportData() { Error = true };
					}
					catch (UnknownResponseException unknownException)
					{
						var response = unknownException.ResponseMessage;
						ReportsPerCampaign[item.MailChimpId] = new ReportData() { Error = true };
					}
					catch (Exception e)
					{
						ReportsPerCampaign[item.MailChimpId] = new ReportData() { Error = true };
					}
					StateHasChanged();
				}
			}
		}

		private class ReportData
		{
			public string Title { get; set; }
			public Clicks Clicks { get; set; }
			public Bounces Bounces { get; set; }
			public Forwards Forwards { get; set; }
			public Opens Opens { get; set; }
			public int EmailsSent { get; set; }
			public string SentTime { get; set; }
			public bool Error { get; set; }
		}
	}
}