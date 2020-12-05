using Bitmail.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bitmail.Logic;
using Bitmail.Models;
using MailChimpWrapper.Models;
using MailChimpWrapper.Models.Exceptions;
using MailChimpWrapper.Models.Requests;
using MailChimpWrapper.Models.Responses;
using Campaign = Bitmail.Models.Campaign;
using Tag = Bitmail.Models.Tag;

namespace Bitmail.Pages
{
    public partial class Emails
    {
        [Inject] private MailChimpService MailChimpService { get; set; }
        [Inject] private NavigationManager NavigationManager { get; set; }

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

        protected override async Task OnInitializedAsync()
        {
            AllCampaigns = await DatabaseService.DB.Campaigns.Include(c => c.CampaignTags).ToListAsync();
            Campaigns = AllCampaigns;
            AllTags = await DatabaseService.DB.Tags.ToListAsync();
            if (NavigationManager.Uri.ToLower().Contains("send"))
            {
                SetupSending = true;
            }

            if (Id != null && Id != "new")
            {
                var idInt = Convert.ToInt32(Id);
                CurrentCampaign = Campaigns.FirstOrDefault(c => c.Id == idInt);

                IsNewCampaign = false;
            }
            TemplatesResponse templatesResponse =
                await MailChimpService.Client.GetRequest<TemplatesResponse>(Endpoints.Templates);
            AllTemplates = templatesResponse.Templates;
        }

        private async Task OnCampaignClicked(Campaign item)
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
                
                SelectedTemplate = AllTemplates.FirstOrDefault(t=>t.Id==templateIdInt);
            }
            else
            {
                
            }
            NavigationManager.NavigateTo($"/emails/manage/{item.Id}");

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

        private async Task BeginSend()
        {
            if (CurrentCampaign != null)
            {
                NavigationManager.NavigateTo($"/emails/manage/{CurrentCampaign.Id}/send");
                SetupSending = true;
                if (CurrentCampaign != null&&CurrentCampaign.TemplateId!=null)
                {
                    await OnTemplateSelected(CurrentCampaign.TemplateId);
                }
            }
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
            else if (SetupSending)
            {
                await EditCampaign();
                var campPostReq = CampaignConversion.ConvertToMailChimpCampaignPost(CurrentCampaign);
                
                try
                {
                    CampaignResponse res1 = await MailChimpService.Client.Request<CampaignNewPostRequest, CampaignResponse>(campPostReq);
                    
                }
                catch (ResponseException responseException)
                {
                    var message = responseException.ErrorResponse.Detail;
                }
                catch (UnknownResponseException unknownException)
                {
                    var response = unknownException.ResponseMessage;
                }
                CurrentCampaign = null;
                IsNewCampaign = false;
                Editing = false;
            }
            else
            {
                await EditCampaign();
            }
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
                {Tag = rc, CampaignId = rc.Id, Campaign = CurrentCampaign}).ToList();
            CurrentCampaign.CampaignTags.AddRange(res);
            await DatabaseService.DB.SaveChangesAsync();

            AllCampaigns = await DatabaseService.DB.Campaigns.Include(c => c.CampaignTags).ToListAsync();
            Campaigns = AllCampaigns;
            AllTags = await DatabaseService.DB.Tags.ToListAsync();
            
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

        private async Task SaveCampaign()
        {
            DatabaseService.DB.Campaigns.Add(CurrentCampaign);
            List<Tag> realTags = new List<Tag>();
            foreach (var selectedTag in SelectedTags)
            {
                Tag exisitingTag = AllTags.FirstOrDefault(c => c.Id == selectedTag);
                realTags.Add(exisitingTag);
            }

            List<CampaignTag> res = realTags.Select(rc => new CampaignTag()
                {Tag = rc, CampaignId = rc.Id, Campaign = CurrentCampaign}).ToList();
            CurrentCampaign.CampaignTags = res;
            await DatabaseService.DB.SaveChangesAsync();

            AllCampaigns = await DatabaseService.DB.Campaigns.Include(c => c.CampaignTags).ToListAsync();
            Campaigns = AllCampaigns;
            AllTags = await DatabaseService.DB.Tags.ToListAsync();
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
            AllCampaigns = DatabaseService.DB.Campaigns.Include(c => c.CampaignTags).ToList();
            Campaigns = AllCampaigns;
            AllTags = DatabaseService.DB.Tags.ToList();
            StateHasChanged();
        }

        public async Task OnTemplateSelected(object val)
        {
            if (AllTemplates.FirstOrDefault(a => a.Id == Convert.ToInt32(val)) != null)
            {
                try
                {
                    SelectedTemplateContent = await MailChimpService.Client.Request<TemplateContentGetRequest, TemplateContentResponse>(new TemplateContentGetRequest(Convert.ToInt32(val)));
                    var templateIdInt = Convert.ToInt32(val);

                    if (SelectedTemplate == null)
                    {
                        SelectedTemplate = AllTemplates.FirstOrDefault(t => t.Id == templateIdInt);
                    }
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
            StateHasChanged();
        }

        public void SectionChanged(string key, string value)
        {
            // ??= means that if Sections is null the result of everything after ??= will be set in Sections
            Sections ??= SelectedTemplateContent?.Sections ?? new Dictionary<string, string>();
            Sections[key] = value;
        }
    }
}