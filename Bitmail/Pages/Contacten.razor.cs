using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bitmail.Services;
using Bitmail.Models;
using Microsoft.EntityFrameworkCore;
using MailChimpWrapper.Models;
using Contact = Bitmail.Models.Contact;
using Tag = Bitmail.Models.Tag;
using MailChimpWrapper.Models.Exceptions;
using MailChimpWrapper.Models.Requests;
using MailChimpWrapper.Models.Responses;
using Microsoft.Extensions.Configuration;

namespace Bitmail.Pages
{
    public partial class Contacten
    {
        [Inject] private MailChimpService MailChimpService { get; set; }
        [Inject] private IConfiguration configuration { get; set; }
        [Inject] private DatabaseService DatabaseService { get; set; }
        protected Contact CurrentContact { get; set; }
        protected List<Contact> AllContacts { get; set; }
        protected List<Organisation> AllOrganisations { get; set; }
        protected List<Organisation> Organisations { get; set; }
        protected List<Tag> AllTags { get; set; }
        protected List<Tag> Tags { get; set; }
        protected bool IsNewContact { get; set; }
        protected List<int> SelectedOrganisations { get; set; }
        protected List<int> SelectedTags { get; set; }
        protected bool EditClicked { get; set; }
        protected string SearchTerm { get; set; } = "";
        protected List<Contact> FilteredContacts => AllContacts.Where(i => (i.FirstName + " " + i.LastName).ToLower().Contains(SearchTerm.ToLower())).ToList();

        protected override async Task OnInitializedAsync()
        {
            AllContacts = DatabaseService.DB.Contacts.Include(t => t.OrganisationContacts).Include(x => x.ContactTags).ToList();

            AllOrganisations = DatabaseService.DB.Organisations.ToList();
            Organisations = AllOrganisations;
            AllTags = DatabaseService.DB.Tags.ToList();
            Tags = AllTags;

            CurrentContact = new Contact();
        }

        protected async Task SaveContact()
        {
            //await AddContactToMailchimp();
            DatabaseService.DB.Contacts.Add(CurrentContact);

            List<Organisation> realOrganisations = new List<Organisation>();
            foreach(var selectedOrganisation in SelectedOrganisations)
            {
                Organisation existingOrganisation = AllOrganisations.FirstOrDefault(c => c.Id == selectedOrganisation);
                realOrganisations.Add(existingOrganisation);
            }
            List<OrganisationContact> res = realOrganisations.Select(rc => new OrganisationContact() { Contact = CurrentContact, OrganisationId = rc.Id, Organisation = rc }).ToList();
            CurrentContact.OrganisationContacts = res;
            await DatabaseService.DB.SaveChangesAsync();


            List<Tag> realTags = new List<Tag>();
            foreach(var selectedTag in SelectedTags)
            {
                Tag existingTag = AllTags.FirstOrDefault(c => c.Id == selectedTag);
                realTags.Add(existingTag);
            }
            List<ContactTag> res2 = realTags.Select(rc => new ContactTag() { Contact = CurrentContact, TagId = rc.Id, Tag = rc }).ToList();
            CurrentContact.ContactTags = res2;
            await DatabaseService.DB.SaveChangesAsync();

            await ContactInMailchimp("subscribed");
            AllContacts = DatabaseService.DB.Contacts.ToList();
            CurrentContact = new Contact();
            StateHasChanged();

        }
        protected void OnContactClicked(Contact SelectedContact)
        {
            IsNewContact = false;
            EditClicked = false;
            CurrentContact = SelectedContact;
            SelectedOrganisations = new List<int>();
            SelectedTags = new List<int>();
        }
        protected void OnNewContact()
        {
            IsNewContact = true;
            CurrentContact = new Contact();
            SelectedOrganisations = new List<int>();
            SelectedTags = new List<int>();

        }
        protected void OnOrganisationItemSelected(int id)
        {
            if (SelectedOrganisations == null)
            {
                SelectedOrganisations = new List<int>();
            }

            if (!SelectedOrganisations.Contains(id))
            {
                SelectedOrganisations.Add(id);
            }
            else
            {
                SelectedOrganisations.Remove(id);
            }
            StateHasChanged();

        }
        protected void OnTagItemSelected(int id)
        {
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
        protected async Task RemoveContact()
        {
            await ContactInMailchimp("unsubscribed");

            DatabaseService.DB.Contacts.Remove(CurrentContact);
            DatabaseService.DB.SaveChanges();
            
            CurrentContact = new Contact();
            IsNewContact = true;

            StateHasChanged();
            
            AllContacts = DatabaseService.DB.Contacts.Include(t => t.OrganisationContacts).Include(x => x.ContactTags).ToList();
            AllOrganisations = DatabaseService.DB.Organisations.ToList();
            AllTags = DatabaseService.DB.Tags.ToList();
        }
        protected async Task EditContact()
        {
            List<Organisation> realOrganisations = new List<Organisation>();
            foreach (var selectedOrganisation in SelectedOrganisations)
            {
                Organisation existingOrganisation = AllOrganisations.FirstOrDefault(c => c.Id == selectedOrganisation);
                realOrganisations.Add(existingOrganisation);
            }
            List<OrganisationContact> res = realOrganisations.Select(rc => new OrganisationContact() { Contact = CurrentContact, OrganisationId = rc.Id, Organisation = rc }).ToList();
            CurrentContact.OrganisationContacts = res;
            await DatabaseService.DB.SaveChangesAsync();

            List<Tag> realTags = new List<Tag>();
            foreach (var selectedTag in SelectedTags)
            {
                Tag existingTag = AllTags.FirstOrDefault(c => c.Id == selectedTag);
                realTags.Add(existingTag);
            }
            List<ContactTag> res2 = realTags.Select(rc => new ContactTag() { Contact = CurrentContact, TagId = rc.Id, Tag = rc }).ToList();
            CurrentContact.ContactTags = res2;
            await DatabaseService.DB.SaveChangesAsync();

            await ContactInMailchimp("subscribed");
            StateHasChanged();
            EditClicked = false;
        }
        protected void OnEditClicked()
        {
            EditClicked = true;
        }
        public async Task ContactInMailchimp(string memberStatus)
        {
            try
            {
                List<Member> members = new List<Member>();
                var listId = configuration.GetValue<string>("MailChimpConstants:SubscriberListId");

                Member newMember = new Member();
                newMember.Status = memberStatus;
                newMember.MergeFields = new Dictionary<string, object>();
                newMember.EmailAddress = CurrentContact.Email;
                newMember.MergeFields = new Dictionary<string, object>(){
                {"FNAME",CurrentContact.FirstName},
                {"LNAME",CurrentContact.LastName},
                };
                members.Add(newMember);

                var response = await MailChimpService.Client.Request<BatchSubscribeUnsubscribeRequest, BatchSubscribeUnsubscribeResponse>(
                new BatchSubscribeUnsubscribeRequest(listId)
                {
                    Members = members,
                    UpdateExisting = true,
                });

                if (CurrentContact.ContactTags != null)
                {
                    foreach (var tag in CurrentContact.ContactTags)
                    {
                        await MailChimpService.Client.Request(new MemberTagsPostRequest(listId, CurrentContact.Email, new List<MailChimpWrapper.Models.Tag>() { new MailChimpWrapper.Models.Tag() { Name = tag.Tag.Title, Status = "active" } }));
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
        }
        protected void OnOrganisationSearch(object value)
        {
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                Organisations = AllOrganisations.Where(c =>
                    (c.Name != null) ? c.Name.ToLower().Contains(value.ToString().ToLower()) : false).ToList();
            }
            else
            {
                Organisations = AllOrganisations;
            }
            StateHasChanged();
        }
        protected void OnTagSearch(object value)
        {
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                Tags = Tags.Where(c =>
                    (c.Title != null) ? c.Title.ToLower().Contains(value.ToString().ToLower()) : false).ToList();
            }
            else
            {
                Tags = AllTags;
            }
            StateHasChanged();
        }
    }
}
