using Bitmail.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Bitmail.Logic;
using Bitmail.Models;
using MailChimpWrapper.Models;
using MailChimpWrapper.Models.Exceptions;
using MailChimpWrapper.Models.Requests;
using MailChimpWrapper.Models.Responses;
using Campaign = Bitmail.Models.Campaign;
using Tag = Bitmail.Models.Tag;
using Microsoft.Extensions.Configuration;

namespace Bitmail.Pages
{
	public partial class Emails
	{
		public enum SendingStatus
		{
			Done,
			Loading,
			Error
		}

		[Inject] private MailChimpService MailChimpService { get; set; }
		[Inject] private NavigationManager NavigationManager { get; set; }
		[Inject] private IConfiguration configuration { get; set; }
		[Parameter] public string Id { get; set; }
		private Dictionary<string, string> Sections { get; set; }
		[Inject] private DatabaseService DatabaseService { get; set; }
		private List<Template> AllTemplates { get; set; }
		protected List<Template> Templates { get; set; }
		private bool EditSelectedTemplate { get; set; }
		public string Message { get; set; }
		private TemplateContentResponse SelectedTemplateContent { get; set; }
		private List<Campaign> Campaigns { get; set; }
		private List<Campaign> AllCampaigns { get; set; }
		private Template SelectedTemplate { get; set; }
		private bool IsNewCampaign { get; set; }
		private bool SetupSending { get; set; }
		private bool TagsChanged { get; set; }
		private bool Editing { get; set; }
		private Campaign CurrentCampaign { get; set; }
		private List<Tag> AllTags { get; set; }
		private List<int> SelectedTags { get; set; }
		protected string SearchedValue { get; set; }
		protected bool SendingCampaign { get; set; }
		protected SendingStatus EmailStatus { get; set; }
		protected string SendMessage { get; set; }
		protected int? LastId { get; set; }

		protected async Task GetData()
		{
			await DatabaseService.DB.Contacts.ToListAsync();
			await DatabaseService.DB.ContactTags.ToListAsync();
			AllCampaigns = await DatabaseService.DB.Campaigns.Include(c => c.CampaignTags).OrderByDescending(c => c.Date).ToListAsync();
			Campaigns = AllCampaigns;
			AllTags = await DatabaseService.DB.Tags.ToListAsync();
		}

		protected override async Task OnInitializedAsync()
		{
			await GetData();
			if (NavigationManager.Uri.ToLower().Contains("send"))
			{
				SetupSending = true;
			}

			TemplatesResponse templatesResponse =
				await MailChimpService.Client.GetRequest<TemplatesResponse>(Endpoints.Templates);
			AllTemplates = templatesResponse.Templates;
			if (!string.IsNullOrEmpty(Id) && Id != "new")
			{
				var idInt = Convert.ToInt32(Id);
				var selectedCampaign = Campaigns.FirstOrDefault(c => c.Id == idInt);
				await OnCampaignClicked(selectedCampaign, fromUrl: true);
			}
		}

		private async Task OnCampaignClicked(Campaign item, bool fromUrl = false)
		{
			IsNewCampaign = false;
			SetupSending = false;
			Editing = true;
			TagsChanged = false;
			//reset the changes done to currentCampaign
			if (CurrentCampaign != null)
			{
				DatabaseService.DB.Entry(CurrentCampaign).Reload();
			}

			CurrentCampaign = item;
			Editing = true;
			SelectedTags = new List<int>();
			foreach (var tag in CurrentCampaign.CampaignTags)
			{
				SelectedTags.Add(tag.TagId);
			}

			if (CurrentCampaign.TemplateId != null)
			{
				var templateIdInt = Convert.ToInt32(CurrentCampaign.TemplateId);

				SelectedTemplate = AllTemplates.FirstOrDefault(t => t.Id == templateIdInt);
				if (SelectedTemplate != null)
				{
					await OnTemplateSelected(SelectedTemplate.Id);
				}
			}
			else
			{
			}
			if (!fromUrl)
			{
				NavigationManager.NavigateTo($"/emails/manage/{item.Id}");
			}
			StateHasChanged();
		}

		private async Task OnNewCampaign()
		{
			NavigationManager.NavigateTo($"/emails/manage/new");

			IsNewCampaign = true;
			TagsChanged = false;
			CurrentCampaign = new Campaign();
			SelectedTags = new List<int>();
			StateHasChanged();
		}

		private async Task OnCancel()
		{
			SetupSending = false;
			Editing = false;
			TagsChanged = false;
			//reset the changes done to currentCampaign
			if (CurrentCampaign != null)
			{
				await DatabaseService.DB.Entry(CurrentCampaign).ReloadAsync();
			}

			CurrentCampaign = null;
			SelectedTags = new List<int>();
			StateHasChanged();
		}

		private async Task SaveCampaignAndSend()
		{
			if (IsNewCampaign)
			{
				await SaveCampaign();
			}
			else
			{
				await EditCampaign();
			}
		}

		protected async Task SendCampaign()
		{
			LastId = null;
			EmailStatus = SendingStatus.Loading;
			await SyncContacts();
			NavigationManager.NavigateTo($"/emails/manage/{CurrentCampaign.Id}/send");
			SetupSending = true;
			SendMessage = null;
			SendingCampaign = true;
			StateHasChanged();
			await EditCampaign();
			CurrentCampaign.ListId = configuration.GetValue<string>("MailChimpConstants:SubscriberListId");
			var campPostReq = CampaignConversion.ConvertToMailChimpCampaignPost(CurrentCampaign);

			if (campPostReq.Recipients.SegmentOpts == null)
			{
				campPostReq.Recipients.SegmentOpts = new SegmentOpts();
			}
			campPostReq.Recipients.SegmentOpts.Match = "all";
			if (campPostReq.Recipients.SegmentOpts.Conditions == null)
			{
				campPostReq.Recipients.SegmentOpts.Conditions = new List<Condition>();
			}
			var response = await MailChimpService.Client.Request<ListSegmentsGetRequest, ListSegmentsResponse>(new ListSegmentsGetRequest(CurrentCampaign.ListId, "?count=1000"));
			var segments = response.Segments;
			foreach (var item in SelectedTags)
			{
				Segment seg = segments.FirstOrDefault(s => s.Name.ToLower() == AllTags.FirstOrDefault(at => at.Id == item).Title.ToLower());
				if (seg != null)
				{
					Condition condition = new Condition()
					{
						ConditionType = "StaticSegment",
						Field = "static_segment",
						Operator = "static_is",
						Value = seg.Id.ToString()
					};

					campPostReq.Recipients.SegmentOpts.Conditions.Add(condition);
				}
			}
			try
			{
				CampaignResponse res1 = await MailChimpService.Client.Request<CampaignNewPostRequest, CampaignResponse>(campPostReq);

				CampaignContentResponse test = await MailChimpService.Client.Request<CampaignContentEditRequest, CampaignContentResponse>(
				new CampaignContentEditRequest(res1.Id)
				{
					Template = new CampaignTemplate()
					{
						Id = Convert.ToInt32(SelectedTemplate.Id),
						Sections = Sections
					}
				});
				await MailChimpService.Client.Request(new CampaignSendRequest(res1.Id));
				var selectedtags = AllTags.Where(at => SelectedTags.Any(st => st == at.Id));
				string contacts = "";
				string tags = "";
				foreach (var item in selectedtags)
				{
					tags += $"{item.Title};";
					foreach (var ct in item.ContactTags)
					{
						if (ct != null && ct.Contact != null && !string.IsNullOrEmpty(ct.Contact.Email))
						{
							contacts += $"{ct.Contact.Email};";
						}
					}
				}

				var cmpgnHistory = new CampaignHistory() { MailChimpId = res1.Id, HTML = test.Html, Contacts = contacts, Tags = tags, Date = DateTime.UtcNow, Title = CurrentCampaign.Title, Description = CurrentCampaign.Description, SubjectLine = CurrentCampaign.SubjectLine };
				DatabaseService.DB.CampaignHistory.Add(cmpgnHistory);

				await SaveCampaign(update: true);
				LastId = cmpgnHistory.Id;
				SendMessage = "Email is verzonden";
				EmailStatus = SendingStatus.Done;
			}
			catch (ResponseException responseException)
			{
				var message = responseException.ErrorResponse.Detail;
				SendMessage = $"Email kon niet worden verzonden: {message}";
				EmailStatus = SendingStatus.Error;
			}
			catch (UnknownResponseException unknownException)
			{
				var responseError = unknownException.ResponseMessage;
				SendMessage = $"Email kon niet worden verzonden: {responseError.Content}";
				EmailStatus = SendingStatus.Error;
			}
			CurrentCampaign = null;
			IsNewCampaign = false;
			Editing = false;
			SendingCampaign = false;

			await GetData();
			StateHasChanged();
		}

		private async Task EditCampaign()
		{
			List<Tag> realTags = new List<Tag>();
			foreach (var selectedTag in SelectedTags)
			{
				if (CurrentCampaign.CampaignTags.All(ct => ct.TagId != selectedTag))
				{
					Tag exisitingTag = AllTags.FirstOrDefault(c => c.Id == selectedTag);

					realTags.Add(exisitingTag);
				}
			}

			List<CampaignTag> res = realTags.Select(rc => new CampaignTag()
			{ Tag = rc, CampaignId = rc.Id, Campaign = CurrentCampaign }).ToList();
			CurrentCampaign.CampaignTags.AddRange(res);
			await DatabaseService.DB.SaveChangesAsync();

			await GetData();
		}

		protected void OnCancelEdit()
		{
			Editing = false;
			//reset the changes done to currentCampaign
			DatabaseService.DB.Entry(CurrentCampaign).Reload();
			SelectedTags = new List<int>();
			StateHasChanged();
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

		private async Task SaveCampaign(bool update = false)
		{
			if (!update)
			{
				DatabaseService.DB.Campaigns.Add(CurrentCampaign);
			}
			List<Tag> realTags = new List<Tag>();
			foreach (var selectedTag in SelectedTags)
			{
				Tag exisitingTag = AllTags.FirstOrDefault(c => c.Id == selectedTag);
				realTags.Add(exisitingTag);
			}

			List<CampaignTag> res = realTags.Select(rc => new CampaignTag()
			{ Tag = rc, CampaignId = rc.Id, Campaign = CurrentCampaign }).ToList();
			CurrentCampaign.CampaignTags = res;
			CurrentCampaign.Date = DateTime.UtcNow;
			await DatabaseService.DB.SaveChangesAsync();

			await GetData();
			CurrentCampaign = null;
			IsNewCampaign = false;

			StateHasChanged();
		}

		private void OnTagItemSelected(int id)
		{
			TagsChanged = true;
			if (SelectedTags == null)
			{
				SelectedTags = new List<int>();
			}

			if (!SelectedTags.Contains(id))
			{
				SelectedTags.Add(id);
			}
			else
			{
				SelectedTags.Remove(id);
			}

			StateHasChanged();
		}

		private void RemoveTag()
		{
			DatabaseService.DB.Campaigns.Remove(CurrentCampaign);
			DatabaseService.DB.SaveChanges();
			CurrentCampaign = new Campaign();
			IsNewCampaign = true;
			_ = GetData();
			StateHasChanged();
		}

		public async Task OnTemplateSelected(object val)
		{
			if (val.ToString() != "None")
			{
				if (AllTemplates.FirstOrDefault(a => a.Id == Convert.ToInt32(val)) != null)
				{
					try
					{
						SelectedTemplateContent =
							await MailChimpService.Client.Request<TemplateContentGetRequest, TemplateContentResponse>(
								new TemplateContentGetRequest(Convert.ToInt32(val)));
						var templateIdInt = Convert.ToInt32(val);

						SelectedTemplate = AllTemplates.FirstOrDefault(t => t.Id == templateIdInt);

						if (CurrentCampaign != null)
						{
							if (SelectedTemplate != null)
								CurrentCampaign.TemplateId = SelectedTemplate.Id.ToString();
						}
					}
					catch (Exception e)
					{
					}
				}
			}
			else
			{
				if (CurrentCampaign != null)
				{
					CurrentCampaign.TemplateId = null;
					SelectedTemplateContent = null;
				}
			}

			StateHasChanged();
		}

		public void SectionChanged(string key, string value)
		{
			// ??= means that if Sections is null the result of everything after ??= will be set in Sections
			Sections ??= SelectedTemplateContent?.Sections ?? new Dictionary<string, string>();
			Sections[key] = value;
		}

		public async Task SyncContacts()
		{
			try
			{
				var listId = configuration.GetValue<string>("MailChimpConstants:SubscriberListId");
				var AllContacts = DatabaseService.DB.Contacts.Include(c => c.ContactTags).ToList();
				List<Member> ConvertedContacts = new List<Member>();
				List<(Member, List<MailChimpWrapper.Models.Tag>)> tags = new List<(Member, List<MailChimpWrapper.Models.Tag>)>();
				foreach (var item in AllContacts)
				{
					Member newMember = new Member();
					newMember.Status = "subscribed";
					newMember.MergeFields = new Dictionary<string, object>();
					newMember.EmailAddress = item.Email;
					newMember.MergeFields = new Dictionary<string, object>()
					{
						{"FNAME",item.FirstName},
						{"LNAME",item.LastName},
					};
					List<MailChimpWrapper.Models.Tag> newtags = new List<MailChimpWrapper.Models.Tag>();
					foreach (var t in item.ContactTags)
					{
						newtags.Add(new MailChimpWrapper.Models.Tag() { Name = t.Tag.Title, Status = "active" });
					}
					tags.Add((newMember, newtags));
					ConvertedContacts.Add(newMember);
				}
				var res = await MailChimpService.Client.Request<BatchSubscribeUnsubscribeRequest, BatchSubscribeUnsubscribeResponse>(
					new BatchSubscribeUnsubscribeRequest(listId)
					{
						Members = ConvertedContacts,
						UpdateExisting = true,
					});
				if (res.NewMembers.Length + res.UpdatedMembers.Length == ConvertedContacts.Count)
				{
					foreach (var ct in tags)
					{
						await MailChimpService.Client.Request(new MemberTagsPostRequest(listId, ct.Item1.EmailAddress, ct.Item2));
					}
				}
			}
			catch (ResponseException responseException)
			{
				var message = responseException.ErrorResponse.Detail;
			}
			catch (UnknownResponseException unknownException)
			{
				var response = unknownException.ResponseMessage;
			}
			catch (Exception e)
			{
			}
		}
	}
}