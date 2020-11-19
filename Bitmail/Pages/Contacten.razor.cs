using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bitmail.Services;
using Bitmail.Models;
using Microsoft.EntityFrameworkCore;

namespace Bitmail.Pages
{
    public partial class Contacten
    {
        [Inject]
        private DatabaseService DatabaseService { get; set; }

        protected Contact CurrentContact { get; set; }
        protected List<Contact> AllContacts { get; set; }
        //protected List<Tag> AllTags { get; set; }
        protected List<Organisation> AllOrganisations { get; set; }
        protected bool IsNewContact { get; set; }
        protected List<int> SelectedOrganisations { get; set; }
        

        protected override async Task OnInitializedAsync()
        {
            AllContacts = DatabaseService.DB.Contacts.Include(t => t.OrganisationContacts).ToList();
            AllOrganisations = DatabaseService.DB.Organisations.ToList();
            //AllTags = DatabaseService.DB.Tags.ToList();

            CurrentContact = new Contact();
        }

        protected async Task SaveContact()
        {
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

            AllContacts = DatabaseService.DB.Contacts.ToList();
            CurrentContact = new Contact();
            StateHasChanged();

        }
        protected void OnContactClicked(Contact SelectedContact)
        {
            IsNewContact = false;
            CurrentContact = SelectedContact;
            SelectedOrganisations = new List<int>();
        }
        protected void OnNewContact()
        {
            IsNewContact = true;
            CurrentContact = new Contact();
            SelectedOrganisations = new List<int>();
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
        protected void RemoveContact()
        {
            DatabaseService.DB.Contacts.Remove(CurrentContact);
            DatabaseService.DB.SaveChanges();
            CurrentContact = new Contact();
            IsNewContact = true;
            StateHasChanged();

            AllContacts = DatabaseService.DB.Contacts.Include(t => t.OrganisationContacts).ToList();
            AllOrganisations = DatabaseService.DB.Organisations.ToList();
        }
    }
}
