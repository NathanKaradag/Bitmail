using Bitmail.Models;
using Bitmail.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitmail.Pages
{
    public partial class Labels
    {
        [Inject]
        private DatabaseService DatabaseService { get; set; }
        
        protected override void OnInitialized()
        {
            alleTags = DatabaseService.DB.Tags.ToList();
        }

        List<Tag> alleTags = new List<Tag>();
        string keyword = "";
        List<Tag> FilteredTags => alleTags.Where(i => (i.Title).ToLower().Contains(keyword.ToLower())).ToList();
    }
}
