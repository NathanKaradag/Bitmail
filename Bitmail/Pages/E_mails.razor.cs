using Bitmail.Services;
using MailChimpWrapper;
using MailChimpWrapper.Models;
using MailChimpWrapper.Models.Exceptions;
using MailChimpWrapper.Models.Requests;
using MailChimpWrapper.Models.Responses;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Bitmail.Pages
{
    public class E_mailsModel : ComponentBase
    {
        [Inject]
        private MailChimpService _mailChimpService { get; set; }

        protected List<Template> Templates { get; set; }
        protected List<Campaign> Campaigns { get; set; }
        protected CampaignResponse CurrentCampaign { get; set; }
        protected Template SelectedTemplate { get; set; }
        protected TemplateContentResponse SelectedTemplateContent { get; set; }
        protected Dictionary<string, string> Sections { get; set; }
        protected List<SubscriberList> RecipientsLists { get; set; }
        protected bool EditSelectedTemplate { get; set; }
        protected string Message { get; set; }

        protected override async Task OnInitializedAsync()
        {
            TemplatesResponse templatesResponse = await _mailChimpService.Client.GetRequest<TemplatesResponse>(Endpoints.Templates);
            Templates = templatesResponse.Templates;
            CampaignsResponse campaignsResponse = await _mailChimpService.Client.GetRequest<CampaignsResponse>(Endpoints.Campaigns);
            Campaigns = campaignsResponse.Campaigns;
            ListsResponse listsres = await _mailChimpService.Client.GetRequest<ListsResponse>(Endpoints.Lists);
            RecipientsLists = listsres.Lists;
            CurrentCampaign = new CampaignResponse();
            SelectedTemplate = new Template();
        }

        public async Task OnTemplateSelected(object val)
        {
            if (Templates.FirstOrDefault(a => a.Id == Convert.ToInt32(val)) != null)
            {
                try
                {
                    SelectedTemplateContent = await _mailChimpService.Client.Request<TemplateContentGetRequest, TemplateContentResponse>(new TemplateContentGetRequest(Convert.ToInt32(val)));
                    SelectedTemplate.Id = Convert.ToInt32(val);
                }
                catch (Exception e)
                {
                }
            }
            StateHasChanged();
        }

        public void SectionChanged(string key, string value)
        {
            if (Sections == null)
            {
                if (SelectedTemplateContent != null && SelectedTemplateContent.Sections != null)
                {
                    Sections = SelectedTemplateContent.Sections;
                }
                else
                {
                    Sections = new Dictionary<string, string>();
                }
            }
            Sections[key] = value;
        }
        public async Task OnCampaignSelected(object val)
        {
            CurrentCampaign = await _mailChimpService.Client.Request<CampaignGetRequest, CampaignResponse>(new CampaignGetRequest(val.ToString()));
            if (CurrentCampaign.Settings == null)
            {
                CurrentCampaign.Settings = new Settings();
            }
            if (CurrentCampaign.Recipients == null)
            {
                CurrentCampaign.Recipients = new Recipients();
            }
            StateHasChanged();
        }

        public async Task OnEditRequest()
        {
            Message = null;
            try
            {
                CampaignEditRequest campaignEditRequest = new CampaignEditRequest(CurrentCampaign);
                CampaignResponse resp = await _mailChimpService.Client.Request<CampaignEditRequest, CampaignResponse>(campaignEditRequest);
                CampaignContentResponse test = await _mailChimpService.Client.Request<CampaignContentEditRequest, CampaignContentResponse>(
                   new CampaignContentEditRequest(CurrentCampaign.Id)
                   {
                       Template = new CampaignTemplate()
                       {
                           Id = Convert.ToInt32(SelectedTemplate.Id),
                           Sections = Sections
                       }
                   });
            }
            catch (ResponseException responseException)
            {
                Message = "ResponseException: " + responseException.ErrorResponse.Detail;
            }
            catch (UnknownResponseException unknownException)
            {
                Message = "UnknownResponseException: " + unknownException.ResponseMessage.Content.ToString();
            }
            catch (Exception e)
            {
                Message = e.Message;
            }
            StateHasChanged();
        }
        public async Task OnSend()
        {
            Message = null;
            try
            {
                await _mailChimpService.Client.Request(new CampaignSendRequest(CurrentCampaign.Id));
                Message = "The campaign has been send";
            }
            catch (ResponseException responseException)
            {
                Message = "ResponseException: " + responseException.ErrorResponse.Detail;
            }
            catch (UnknownResponseException unknownException)
            {
                Message = "UnknownResponseException: " + unknownException.ResponseMessage.Content.ToString();
            }
            catch (Exception e)
            {
                Message = e.Message;
            }
            StateHasChanged();
        }
    }
}