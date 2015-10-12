using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.EventReceivers;
using System;
using System.ServiceModel.Activation;

namespace SpSatWeb
{

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class EventReceiver : IRemoteEventService
    {

        public SPRemoteEventResult ProcessEvent(SPRemoteEventProperties properties)
        {
            try
            {
                using (ClientContext context = TokenHelper.CreateRemoteEventReceiverClientContext(properties))
                {
                    User user = context.Web.CurrentUser;
                    context.Load(user, u => u.LoginName);
                    context.ExecuteQuery();
                    SPRemoteEventResult result = new SPRemoteEventResult();
                    result.ErrorMessage = user.LoginName + " hit the event receiver.";
                    result.Status = SPRemoteEventServiceStatus.CancelWithError;
                    return result;
                }
            }
            catch (Exception ex)
            {
                SPRemoteEventResult result = new SPRemoteEventResult();
                result.ErrorMessage = "ex (ContentToken? " + ((properties.ContextToken.Length > 0) ? "yes" : "no") + ") - " + ex.Message;
                result.Status = SPRemoteEventServiceStatus.CancelWithError;
                return result;
            }
        }

        public void ProcessOneWayEvent(SPRemoteEventProperties properties)
        {
            throw new NotImplementedException();
        }

    }
}
