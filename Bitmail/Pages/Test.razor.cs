using Bitmail.Models;
using Bitmail.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitmail.Pages
{
	public partial class Test
	{
		[Inject]
		private DatabaseService DatabaseService { get; set; }

		protected Tag CurrentTag { get; set; }
		protected List<Tag> AllTags { get; set; }
		protected List<Contact> AllContacts { get; set; }
		protected bool IsNewTag { get; set; }

		protected List<int> SelectedContacts { get; set; }

		protected override async Task OnInitializedAsync()
		{
			AllTags = await DatabaseService.DB.Tags.Include(t => t.ContactTags).ToListAsync();
			AllContacts = DatabaseService.DB.Contacts.ToList();
			CurrentTag = new Tag();
		}

		protected async Task SaveTag()
		{
			DatabaseService.DB.Tags.Add(CurrentTag);
			List<Contact> realContacts = new List<Contact>();
			foreach (var selectedContact in SelectedContacts)
			{
				Contact exisitingContact = AllContacts.FirstOrDefault(c => c.Id == selectedContact);
				realContacts.Add(exisitingContact);
			}

			List<ContactTag> res = realContacts.Select(rc => new ContactTag() { Tag = CurrentTag, ContactId = rc.Id, Contact = rc }).ToList();
			CurrentTag.ContactTags = res;
			await DatabaseService.DB.SaveChangesAsync();

			AllTags = DatabaseService.DB.Tags.ToList();
			CurrentTag = new Tag();
			StateHasChanged();
		}

		protected void OnTagClicked(Tag selectedTag)
		{
			IsNewTag = false;
			CurrentTag = selectedTag;
			SelectedContacts = new List<int>();
		}

		protected void OnNewTag()
		{
			IsNewTag = true;
			CurrentTag = new Tag();
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

		protected void RemoveTag()
		{
			DatabaseService.DB.Tags.Remove(CurrentTag);
			DatabaseService.DB.SaveChanges();
			CurrentTag = new Tag();
			IsNewTag = true;
			AllTags = DatabaseService.DB.Tags.Include(t => t.ContactTags).ToList();
			AllContacts = DatabaseService.DB.Contacts.ToList();
		}
	}
}