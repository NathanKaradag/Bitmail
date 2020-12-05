using MailChimpWrapper.Models;
using MailChimpWrapper.Models.Requests;

namespace Bitmail.Logic
{
    public static class CampaignConversion
    {
        /// <summary>
        /// Will convert a bitmailCampaign to a MailChimp Campaign
        /// </summary>
        /// <param name="bitmailCampaign"></param>
        /// <returns></returns>
        public static CampaignNewPostRequest ConvertToMailChimpCampaignPost(Models.Campaign bitmailCampaign)
        {
            CampaignNewPostRequest mailChimpCampaign = new CampaignNewPostRequest
            {
                Id = bitmailCampaign.Id.ToString(),
                Settings = new Settings
                {
                    Title = bitmailCampaign.Title,
                    FromName = bitmailCampaign.FromName,
                    ReplyTo = bitmailCampaign.ReplyTo,
                    PreviewText = bitmailCampaign.PreviewText,
                    SubjectLine = bitmailCampaign.SubjectLine
                },
                Recipients = new Recipients() {ListId = bitmailCampaign.ListId}
            };

            return mailChimpCampaign;
        }
    }
}