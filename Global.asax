<%@ Application Language="C#" %>
<script runat="server">
    void Application_Start(object sender, EventArgs e)
    {
        // Initialize application
        AuthorizeUnifi.Utils.Logger.LogInfo("Application started");
    }

    void Application_End(object sender, EventArgs e)
    {
        // Clean up
        AuthorizeUnifi.Utils.Logger.LogInfo("Application ended");
    }

    void Application_Error(object sender, EventArgs e)
    {
        Exception ex = Server.GetLastError();
        if (ex != null)
        {
            AuthorizeUnifi.Utils.Logger.LogError("Unhandled application error", ex);
        }
    }

    void Session_Start(object sender, EventArgs e)
    {
        AuthorizeUnifi.Utils.Logger.LogDebug("New session started");
    }

    void Session_End(object sender, EventArgs e)
    {
        AuthorizeUnifi.Utils.Logger.LogDebug("Session ended");
    }
</script>
