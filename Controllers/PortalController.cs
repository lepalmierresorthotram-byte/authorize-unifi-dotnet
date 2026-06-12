using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using AuthorizeUnifi.Services;
using AuthorizeUnifi.Utils;
using Newtonsoft.Json;

namespace AuthorizeUnifi.Controllers
{
    /// <summary>
    /// Main portal controller handling HTTP requests
    /// </summary>
    public class PortalController
    {
        private readonly AuthorizationService _authService;

        public PortalController()
        {
            _authService = new AuthorizationService();
        }

        /// <summary>
        /// Display portal page
        /// </summary>
        public static void ShowPortalPage(HttpResponse response)
        {
            try
            {
                string html = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Lepalmier Resort WiFi - Guest Portal</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
        }

        .container {
            background: white;
            border-radius: 15px;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            max-width: 500px;
            width: 100%;
            padding: 40px;
            animation: slideIn 0.5s ease-out;
        }

        @keyframes slideIn {
            from {
                opacity: 0;
                transform: translateY(20px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        .header {
            text-align: center;
            margin-bottom: 30px;
        }

        .logo {
            width: 80px;
            height: 80px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 40px;
            margin: 0 auto 20px;
        }

        h1 {
            color: #333;
            font-size: 28px;
            margin-bottom: 10px;
        }

        .subtitle {
            color: #666;
            font-size: 14px;
        }

        .content {
            margin-bottom: 30px;
        }

        .ad-banner {
            background: #f5f5f5;
            border-left: 4px solid #667eea;
            padding: 15px;
            margin-bottom: 20px;
            border-radius: 5px;
        }

        .ad-banner h3 {
            color: #667eea;
            font-size: 14px;
            margin-bottom: 8px;
        }

        .ad-banner p {
            color: #666;
            font-size: 13px;
            line-height: 1.5;
        }

        .features {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 15px;
            margin-bottom: 20px;
        }

        .feature {
            background: #f9f9f9;
            padding: 15px;
            border-radius: 8px;
            text-align: center;
        }

        .feature-icon {
            font-size: 24px;
            margin-bottom: 8px;
        }

        .feature-title {
            color: #333;
            font-size: 13px;
            font-weight: 600;
        }

        .status-box {
            background: #f0f7ff;
            border: 1px solid #667eea;
            padding: 15px;
            border-radius: 8px;
            margin-bottom: 20px;
        }

        .status-label {
            color: #667eea;
            font-size: 12px;
            font-weight: 600;
            margin-bottom: 5px;
        }

        .status-value {
            color: #333;
            font-size: 14px;
        }

        .button-group {
            display: flex;
            gap: 10px;
            margin-bottom: 20px;
        }

        .btn {
            flex: 1;
            padding: 12px;
            border: none;
            border-radius: 8px;
            font-size: 14px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .btn-primary {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
        }

        .btn-primary:hover {
            transform: translateY(-2px);
            box-shadow: 0 10px 20px rgba(102, 126, 234, 0.3);
        }

        .btn-secondary {
            background: #f5f5f5;
            color: #333;
            border: 1px solid #ddd;
        }

        .btn-secondary:hover {
            background: #f0f0f0;
        }

        .btn:disabled {
            opacity: 0.5;
            cursor: not-allowed;
        }

        .loading {
            display: none;
            text-align: center;
            color: #667eea;
            margin: 20px 0;
        }

        .spinner {
            border: 3px solid #f3f3f3;
            border-top: 3px solid #667eea;
            border-radius: 50%;
            width: 30px;
            height: 30px;
            animation: spin 1s linear infinite;
            margin: 0 auto 10px;
        }

        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }

        .message {
            padding: 12px;
            border-radius: 8px;
            margin-bottom: 15px;
            display: none;
        }

        .message.success {
            background: #d4edda;
            color: #155724;
            border: 1px solid #c3e6cb;
            display: block;
        }

        .message.error {
            background: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
            display: block;
        }

        .message.info {
            background: #d1ecf1;
            color: #0c5460;
            border: 1px solid #bee5eb;
            display: block;
        }

        .footer {
            text-align: center;
            color: #999;
            font-size: 12px;
            margin-top: 20px;
            padding-top: 20px;
            border-top: 1px solid #eee;
        }

        @media (max-width: 480px) {
            .container {
                padding: 25px;
            }

            h1 {
                font-size: 22px;
            }

            .features {
                grid-template-columns: 1fr;
            }

            .button-group {
                flex-direction: column;
            }
        }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div class=""logo"">📶</div>
            <h1>Lepalmier Resort</h1>
            <p class=""subtitle"">Guest WiFi Portal</p>
        </div>

        <div id=""message"" class=""message""></div>

        <div class=""content"">
            <div class=""ad-banner"">
                <h3>🌴 Welcome to Our Resort</h3>
                <p>Enjoy high-speed WiFi throughout your stay. Complimentary access for all guests!</p>
            </div>

            <div class=""features"">
                <div class=""feature"">
                    <div class=""feature-icon"">⚡</div>
                    <div class=""feature-title"">High Speed</div>
                </div>
                <div class=""feature"">
                    <div class=""feature-icon"">🔒</div>
                    <div class=""feature-title"">Secure</div>
                </div>
                <div class=""feature"">
                    <div class=""feature-icon"">📱</div>
                    <div class=""feature-title"">Mobile Friendly</div>
                </div>
                <div class=""feature"">
                    <div class=""feature-icon"">✅</div>
                    <div class=""feature-title"">Free Access</div>
                </div>
            </div>

            <div class=""status-box"">
                <div class=""status-label"">📡 CONNECTION STATUS</div>
                <div class=""status-value"" id=""statusValue"">Checking...</div>
            </div>
        </div>

        <div class=""button-group"">
            <button class=""btn btn-primary"" id=""connectBtn"" onclick=""connectWifi()"">
                ✓ Connect WiFi
            </button>
            <button class=""btn btn-secondary"" id=""statusBtn"" onclick=""checkStatus()"">
                Refresh
            </button>
        </div>

        <div class=""loading"" id=""loading"">
            <div class=""spinner""></div>
            <p>Connecting...</p>
        </div>

        <div class=""footer"">
            <p>Portal v1.0 | Support available 24/7</p>
        </div>
    </div>

    <script>
        function showMessage(text, type = 'info') {
            const msgDiv = document.getElementById('message');
            msgDiv.textContent = text;
            msgDiv.className = 'message ' + type;
            setTimeout(() => { msgDiv.className = 'message'; }, 5000);
        }

        function checkStatus() {
            fetch('/api/status', { method: 'GET' })
                .then(r => r.json())
                .then(data => {
                    document.getElementById('statusValue').textContent = 
                        data.connected ? '✅ Connected' : '❌ Not Connected';
                })
                .catch(() => document.getElementById('statusValue').textContent = '⚠️ Unable to check');
        }

        function connectWifi() {
            const btn = document.getElementById('connectBtn');
            const loading = document.getElementById('loading');
            
            btn.disabled = true;
            loading.style.display = 'block';

            fetch('/api/authorize', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ duration: 60 })
            })
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    showMessage('✓ WiFi Connected! Enjoy your stay!', 'success');
                    setTimeout(() => checkStatus(), 1000);
                } else {
                    showMessage('❌ ' + (data.message || 'Connection failed'), 'error');
                }
            })
            .catch(err => showMessage('❌ Connection error', 'error'))
            .finally(() => {
                loading.style.display = 'none';
                btn.disabled = false;
            });
        }

        // Check status on page load
        window.onload = checkStatus;
    </script>
</body>
</html>";
                
                response.ContentType = "text/html; charset=utf-8";
                response.Write(html);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error rendering portal page", ex);
                response.Write("<h1>Error</h1><p>Failed to load portal page</p>");
            }
        }

        /// <summary>
        /// Handle authorization request
        /// </summary>
        public async Task HandleAuthorizeRequest(HttpContext context)
        {
            try
            {
                string clientIp = context.Request.ServerVariables["REMOTE_ADDR"];
                string duration = context.Request.Form["duration"] ?? "60";

                if (!int.TryParse(duration, out int minutes))
                    minutes = 60;

                Logger.LogInfo($"Authorization request from IP: {clientIp}");

                var result = await _authService.AuthorizeGuestAsync(clientIp, null, minutes);

                var response = new
                {
                    success = result.Success,
                    message = result.Message,
                    data = new
                    {
                        macAddress = result.MacAddress,
                        ipAddress = result.IpAddress,
                        authorizedTime = result.AuthorizedTime,
                        expirationTime = result.ExpirationTime
                    }
                };

                SendJsonResponse(context.Response, response);
            }
            catch (Exception ex)
            {
                Logger.LogError("Authorization request error", ex);
                SendJsonResponse(context.Response, new { success = false, message = "Server error" }, 500);
            }
        }

        /// <summary>
        /// Handle status check request
        /// </summary>
        public async Task HandleStatusRequest(HttpContext context)
        {
            try
            {
                string clientIp = context.Request.ServerVariables["REMOTE_ADDR"];
                var unifiService = new UnifiApiService();
                
                bool isConnected = await unifiService.CheckDeviceStatusAsync(clientIp);

                var response = new
                {
                    connected = isConnected,
                    clientIp = clientIp,
                    timestamp = DateTime.Now
                };

                SendJsonResponse(context.Response, response);
            }
            catch (Exception ex)
            {
                Logger.LogError("Status check error", ex);
                SendJsonResponse(context.Response, new { connected = false, error = "Status check failed" }, 500);
            }
        }

        /// <summary>
        /// Send JSON response
        /// </summary>
        private static void SendJsonResponse(HttpResponse response, object data, int statusCode = 200)
        {
            response.StatusCode = statusCode;
            response.ContentType = "application/json; charset=utf-8";
            response.Write(JsonConvert.SerializeObject(data));
        }
    }
}
