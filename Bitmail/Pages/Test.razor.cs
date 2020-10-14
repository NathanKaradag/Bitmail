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
    public class TestModel : ComponentBase
    {
        [Inject]
        private DatabaseService DatabaseService { get; set; }

        public List<Contact> CurrentContacts { get; set; }
        public Contact NewContact { get; set; }

        protected override async Task OnInitializedAsync()
        {
            CurrentContacts = await DatabaseService.DB.Contacts.Include(c => c.ContactTags).Include(c => c.OrganisationContacts).ToListAsync();
            NewContact = new Contact();
            StateHasChanged();
        }

        protected async Task ValidSubmit()
        {
            DatabaseService.DB.Contacts.Add(new Contact() { Email = NewContact.Email });
            await DatabaseService.DB.SaveChangesAsync();
            NewContact = new Contact();
            StateHasChanged();
        }
    }
}