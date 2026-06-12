using System;
using System.Web;
using AuthorizeUnifi.Controllers;

namespace AuthorizeUnifi
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string action = Request.QueryString["action"] ?? "portal";

                var controller = new PortalController();

                switch (action.ToLower())
                {
                    case "authorize":
                        controller.HandleAuthorizeRequest(Context).Wait();
                        break;

                    case "status":
                        controller.HandleStatusRequest(Context).Wait();
                        break;

                    case "portal":
                    default:
                        PortalController.ShowPortalPage(Response);
                        break;
                }
            }
            catch (Exception ex)
            {
                AuthorizeUnifi.Utils.Logger.LogError("Page load error", ex);
                Response.Write("<h1>Error</h1><p>An error occurred while processing your request.</p>");
            }
        }
    }
}
