using Bitmail.Models;
using MailChimpWrapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitmail.Services
{
    /// <summary>
    /// A service which can be injected into pages so we are able to use the MailChimp api
    /// </summary>
    public class MailChimpService
    {
        public MailChimpClient Client { get; }

        public MailChimpService(IConfiguration configuration)
        {
            try
            {
                string key = configuration.GetValue<string>("ApiKeys:MailChimp");
                string server = key.Substring(key.LastIndexOf('-') + 1);
                Client = new MailChimpClient(key, server);
            }
            catch
            {
                throw new Exception("MailChimp Apikeys not configured");
            }
        }
    }
}