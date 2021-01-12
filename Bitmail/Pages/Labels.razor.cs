using Bitmail.Models;
using Bitmail.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailChimpWrapper.Models.Requests;



namespace Bitmail.Pages
{
    public partial class Labels
    {
        [Inject] private DatabaseService DatabaseService { get; set; }
        [Inject] private MailChimpService MailChimpService { get; set; }
        [Inject] private IConfiguration configuration { get; set; }
        protected Tag CurrentTag { get; set; }
        protected List<Tag> AllTags { get; set; }
        protected List<Tag> Tags { get; set; }
        protected List<Contact> AllContacts { get; set; }
        protected List<Contact> Contacts { get; set; }
        protected List<int> SelectedContacts { get; set; }
        protected bool IsNewTag { get; set; }
        protected bool EditClicked { get; set; }
        protected string keyword { get; set; } = "";
        protected string SearchTerm { get; set; } = "";
        protected List<Tag> FilteredTags => AllTags.Where(i => i.Title != null ? (i.Title).ToLower().Contains(keyword.ToLower()) : false).ToList();
        protected override async Task OnInitializedAsync()
        {
            AllContacts = DatabaseService.DB.Contacts.ToList();
            Contacts = AllContacts;
            AllTags = DatabaseService.DB.Tags.Include(x => x.ContactTags).ToList();
            Tags = AllTags;
            CurrentTag = new Tag();
            StateHasChanged();
        }
        protected async Task SaveTag()
        {
            List<Contact> realContacts = new List<Contact>();
            foreach (var selectedContact in SelectedContacts)
            {
                Contact existingContact = AllContacts.FirstOrDefault(c => c.Id == selectedContact);
                realContacts.Add(existingContact);
            }
            List<ContactTag> res2 = realContacts.Select(rc => new ContactTag() { Tag = CurrentTag, TagId = rc.Id, Contact = rc }).ToList();
            CurrentTag.ContactTags = res2;

            DatabaseService.DB.Tags.Add(CurrentTag);

            await DatabaseService.DB.SaveChangesAsync();
            await TagsInMailchimp("active");
            AllTags = DatabaseService.DB.Tags.ToList();
            Tags = AllTags;
            CurrentTag = new Tag();
            StateHasChanged();
        }
        protected void OnTagClicked(Tag selectedTag)
        {
            IsNewTag = false;
            EditClicked = false;
            CurrentTag = selectedTag;
            SelectedContacts = new List<int>();
        }
        protected void OnNewTag()
        {
            IsNewTag = true;
            CurrentTag = new Tag();
            SelectedContacts = new List<int>();
            StateHasChanged();
        }
        protected void OnContactItemSelected(int id)
        {
            if (SelectedContacts == null)
            {
                SelectedContacts = new List<int>();
            }

            if (!SelectedContacts.Contains(id))
            {
                SelectedContacts.Add(id);
            }
            else
            {
                SelectedContacts.Remove(id);
            }
            StateHasChanged();
        }
        protected async Task RemoveTag()
        {
            await TagsInMailchimp("inactive");

            DatabaseService.DB.Tags.Remove(CurrentTag);
            DatabaseService.DB.SaveChanges();
            CurrentTag = new Tag();
            IsNewTag = true;
            StateHasChanged();

            AllContacts = DatabaseService.DB.Contacts.ToList();
            Contacts = AllContacts;
            AllTags = DatabaseService.DB.Tags.Include(x => x.ContactTags).ToList();
        }
        protected async Task EditTag()
        {
            List<Contact> realContacts = new List<Contact>();
            foreach (var selectedContact in SelectedContacts)
            {
                Contact existingContact = AllContacts.FirstOrDefault(c => c.Id == selectedContact);
                realContacts.Add(existingContact);
            }
            List<ContactTag> res2 = realContacts.Select(rc => new ContactTag() { Tag = CurrentTag, TagId = rc.Id, Contact = rc }).ToList();
            CurrentTag.ContactTags = res2;

            await DatabaseService.DB.SaveChangesAsync();
            await TagsInMailchimp("active");
            EditClicked = false;
            StateHasChanged();
        }
        protected void OnEditClicked()
        {
            EditClicked = true;
        }
        protected void OnContactSearch(object value)
        {
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                Contacts = AllContacts.Where(c =>
                    (c.Email != null) ? c.Email.ToLower().Contains(value.ToString().ToLower()) : false).ToList();
            }
            else
            {
                Contacts = AllContacts;
            }

            StateHasChanged();
        }
        protected async Task TagsInMailchimp(string TagStatus)
        {
            var listId = configuration.GetValue<string>("MailChimpConstants:SubscriberListId");
            var TagHasContacts = new List<Contact>();

            foreach(var contacttag in CurrentTag.ContactTags)
            {
                TagHasContacts.Add(contacttag.Contact);
            }

            foreach(var contact in TagHasContacts)
            {
                await MailChimpService.Client.Request(new MemberTagsPostRequest(listId, contact.Email, new List<MailChimpWrapper.Models.Tag>() { new MailChimpWrapper.Models.Tag() { Name = CurrentTag.Title, Status = TagStatus } }));
            }
        }
    }
}
