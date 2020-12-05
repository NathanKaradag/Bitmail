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
    public partial class Organisaties
    {
        [Inject]
        private DatabaseService DatabaseService { get; set; }
        protected Organisation CurrentOrganisation { get; set; }
        protected List<Organisation> AllOrganisations { get; set; }
        protected List<Contact> AllContacts { get; set; }
        protected bool IsNewOrganisation { get; set; }
        protected bool EditClicked { get; set; }
        protected List<int> SelectedContacts { get; set; }
        protected override async Task OnInitializedAsync()
        {
            AllOrganisations = DatabaseService.DB.Organisations.Include(o=>o.OrganisationContacts).ToList();
            AllContacts = DatabaseService.DB.Contacts.ToList();
            CurrentOrganisation = new Organisation();
            StateHasChanged();
        }
        protected async Task SaveOrganisation()
        {
            DatabaseService.DB.Organisations.Add(CurrentOrganisation);
            
            List<Contact> realContacts = new List<Contact>();
            foreach (var selectedContact in SelectedContacts)
            {
                Contact existingContact = AllContacts.FirstOrDefault(c => c.Id == selectedContact);
                realContacts.Add(existingContact);
            }
            List<OrganisationContact> res = realContacts.Select(rc => new OrganisationContact() { Organisation = CurrentOrganisation, ContactId=rc.Id, Contact = rc}).ToList();
            CurrentOrganisation.OrganisationContacts = res;
            await DatabaseService.DB.SaveChangesAsync();
            AllOrganisations = DatabaseService.DB.Organisations.ToList();
            CurrentOrganisation = new Organisation();
            StateHasChanged();
        }

        protected void OnOrganisationClicked(Organisation selectedOrganisation)
        {
            IsNewOrganisation = false;
            EditClicked = false;
            CurrentOrganisation = selectedOrganisation;
            SelectedContacts = new List<int>();
        }
        protected void OnNewOrganisation()
        {
            IsNewOrganisation = true;
            CurrentOrganisation = new Organisation();
            SelectedContacts = new List<int>();
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
        protected void RemoveOrganisation()
        {
            DatabaseService.DB.Organisations.Remove(CurrentOrganisation);
            DatabaseService.DB.SaveChanges();
            CurrentOrganisation = new Organisation();
            IsNewOrganisation = true;
            StateHasChanged();
            AllOrganisations = DatabaseService.DB.Organisations.Include(o => o.OrganisationContacts).ToList();
            AllContacts = DatabaseService.DB.Contacts.ToList();
        }
        protected async Task EditOrganisation()
        {
            List<Contact> realContacts = new List<Contact>();
            foreach (var selectedContact in SelectedContacts)
            {
                Contact existingContact = AllContacts.FirstOrDefault(c => c.Id == selectedContact);
                realContacts.Add(existingContact);
            }
            List<OrganisationContact> res = realContacts.Select(rc => new OrganisationContact() { Organisation = CurrentOrganisation, ContactId = rc.Id, Contact = rc }).ToList();
            CurrentOrganisation.OrganisationContacts = res;
            await DatabaseService.DB.SaveChangesAsync();
            StateHasChanged();
            EditClicked = false;
        }
        protected void OnEditClicked()
        {
            EditClicked = true;
        }
    }
}
