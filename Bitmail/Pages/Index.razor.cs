using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Bitmail.Pages
{
    public partial class Index
    {
        [CascadingParameter]
        private Task<AuthenticationState> _authState { get; set; }

        private AuthenticationState authState { get; set; }

        protected override async Task OnInitializedAsync()
        {
            authState = await _authState;

        }
    }
}
