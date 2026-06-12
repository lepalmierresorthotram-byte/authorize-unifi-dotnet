# 📋 Installation Guide - Authorize Unifi .NET Portal

Comprehensive step-by-step installation guide for Windows Server 2019/2022.

## ⏱️ Estimated Installation Time: 30-45 minutes

## Prerequisites Checklist

- [ ] Windows Server 2019 or 2022
- [ ] Administrative access
- [ ] .NET Framework 4.8 (or Internet access to download)
- [ ] Unifi Controller running (172.16.0.2)
- [ ] Network connectivity to Unifi Controller
- [ ] IIS can be installed

## Step 1: Install .NET Framework 4.8

### Option A: Online Installation
```powershell
# Run as Administrator
$url = "https://dotnet.microsoft.com/download/dotnet-framework/net48"
# Download installer and run
```

### Option B: Offline Installation
```powershell
# Download from Microsoft website
# Run installer as Administrator
.\ndp48-x86-x64-allos-enu.exe

# Verify installation
reg query "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" | find /i "Release"
# Should return: Release REG_DWORD 0x80150
```

## Step 2: Install and Configure IIS

### Install IIS
```powershell
# Run as Administrator
Install-WindowsFeature -Name Web-Server, Web-Asp-Net45 -IncludeManagementTools

# Verify installation
Get-WindowsFeature -Name Web-Server | Select DisplayName, Installed
```

### Enable Required Features
```powershell
# ASP.NET 4.8
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ASPNET48

# Management Service
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ManagementService

# Verify
Get-WindowsFeature -Name IIS-ASPNET48 | Select DisplayName, Installed
```

## Step 3: Clone and Build Application

### Clone Repository
```powershell
# Create application directory
New-Item -Path "C:\inetpub\wwwroot\AuthorizeUnifi" -ItemType Directory -Force

# Navigate to parent directory
cd C:\

# Clone repository
git clone https://github.com/lepalmierresorthotram-byte/authorize-unifi-dotnet.git AuthorizeUnifi-src
cd AuthorizeUnifi-src
```

### Build Application
```powershell
# Method 1: Using MSBuild (recommended)
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\msbuild.exe" `
    AuthorizeUnifi.csproj /p:Configuration=Release /p:OutputPath=.\bin\

# Method 2: Using Visual Studio
# Open AuthorizeUnifi.csproj in Visual Studio and Build > Build Solution

# Verify build
dir .\bin\Release\
```

## Step 4: Deploy to IIS

### Copy Application Files
```powershell
# Copy built files to IIS directory
$srcPath = "C:\AuthorizeUnifi-src\bin\Release\"
$dstPath = "C:\inetpub\wwwroot\AuthorizeUnifi\"

Copy-Item -Path "$srcPath*" -Destination $dstPath -Recurse -Force

# Verify files copied
ls $dstPath | Select Name
```

### Create Log Directory
```powershell
# Create logs directory
New-Item -Path "C:\Logs\AuthorizeUnifi" -ItemType Directory -Force

# Verify
ls "C:\Logs\AuthorizeUnifi"
```

## Step 5: Configure IIS (GUI Method)

### Open IIS Manager
```powershell
# Or manually: Server Manager > Tools > Internet Information Services (IIS) Manager
inetmgr
```

### Create Application Pool

1. **Right-click Application Pools** → Select "Add Application Pool..."
2. **Configure:**
   - Name: `AuthorizeUnifi`
   - .NET CLR version: `.NET CLR Version v4.0.30319`
   - Managed pipeline mode: `Integrated`
3. **Click OK**

### Create Application/Website

1. **Right-click Sites** → Select "Add Website..."
2. **Configure:**
   - Site name: `lepalmier.local`
   - Application pool: `AuthorizeUnifi`
   - Physical path: `C:\inetpub\wwwroot\AuthorizeUnifi`
   - Host name: `lepalmier.local`
   - Port: `80`
3. **Click OK**

### Configure Directory Permissions

1. **Right-click Application Folder** → Select "Edit Permissions"
2. **Security Tab** → **Edit**
3. **Add IIS_IUSRS:**
   - Object Types → Check "Service Accounts" → OK
   - Type: `IIS_IUSRS` → Check Names → OK
   - Permissions: Check "Full Control" → Apply → OK

## Step 6: Configure IIS (PowerShell Method - Alternative)

If you prefer command-line configuration:

```powershell
# Import IIS module
Import-Module WebAdministration

# Create Application Pool
New-WebAppPool -Name "AuthorizeUnifi" -Force
Set-ItemProperty "IIS:\AppPools\AuthorizeUnifi" -Name "managedRuntimeVersion" -Value "v4.0"

# Create Website
New-Website -Name "lepalmier.local" `
    -PhysicalPath "C:\inetpub\wwwroot\AuthorizeUnifi" `
    -HostHeader "lepalmier.local" `
    -Port 80 `
    -ApplicationPool "AuthorizeUnifi"

# Set permissions
$acl = Get-Acl "C:\inetpub\wwwroot\AuthorizeUnifi"
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    "IIS_IUSRS", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow"
)
$acl.AddAccessRule($rule)
Set-Acl "C:\inetpub\wwwroot\AuthorizeUnifi" $acl

# Set log directory permissions
$acl = Get-Acl "C:\Logs\AuthorizeUnifi"
$acl.AddAccessRule($rule)
Set-Acl "C:\Logs\AuthorizeUnifi" $acl
```

## Step 7: Configure Application Settings

### Edit Web.config
```powershell
# Edit with PowerShell
$configPath = "C:\inetpub\wwwroot\AuthorizeUnifi\Web.config"
[xml]$config = Get-Content $configPath

# Update Unifi settings
$config.configuration.appSettings.add | Where-Object {$_.key -eq "UnifiControllerUrl"} | ForEach-Object {$_.value = "https://172.16.0.2:8443"}
$config.configuration.appSettings.add | Where-Object {$_.key -eq "UnifiUsername"} | ForEach-Object {$_.value = "admin"}
$config.configuration.appSettings.add | Where-Object {$_.key -eq "UnifiPassword"} | ForEach-Object {$_.value = "YOUR_UNIFI_PASSWORD"}

# Save
$config.Save($configPath)
```

Or manually using Notepad:
```powershell
notepad C:\inetpub\wwwroot\AuthorizeUnifi\Web.config
```

**Update these values:**
```xml
<appSettings>
    <!-- Change these values -->
    <add key="UnifiControllerUrl" value="https://172.16.0.2:8443" />
    <add key="UnifiUsername" value="admin" />
    <add key="UnifiPassword" value="YOUR_PASSWORD_HERE" />
    <add key="UnifiSite" value="default" />
    <add key="PortalUrl" value="http://lepalmier.local" />
    <add key="GuestNetworkGateway" value="172.16.40.1" />
    <add key="LogPath" value="C:\Logs\AuthorizeUnifi\" />
    <add key="EnableSSL" value="false" />
</appSettings>
```

## Step 8: Verify Installation

### Check IIS Status
```powershell
# Verify website is running
Get-WebsiteState -Name "lepalmier.local" | Select State

# Should output: Running
```

### Test Portal Access

**Local Test (on server):**
```powershell
# Test connectivity
Invoke-WebRequest -Uri "http://localhost/Default.aspx?action=portal" -UseBasicParsing

# Should return HTML content
```

**Remote Test (from guest network):**
```bash
# From guest device
curl http://lepalmier.local
# or
curl http://172.16.0.68

# Should receive HTML portal page
```

### Check Logs
```powershell
# View recent logs
Get-Content "C:\Logs\AuthorizeUnifi\*.txt" -Tail 20

# Should show: "[timestamp] [Info] Application started"
```

## Step 9: Network Configuration

### Add to DNS (if using DNS)
```powershell
# On DNS server
Add-DnsServerResourceRecordA -Name "lepalmier" -ZoneName "local" -IPv4Address "172.16.0.68"
```

### Configure Hosts File (alternative)
```powershell
# On each client that needs to access:
Add-Content -Path "C:\Windows\System32\drivers\etc\hosts" -Value "172.16.0.68 lepalmier.local"
```

## Step 10: Firewall Configuration (if needed)

```powershell
# Allow HTTP traffic
New-NetFirewallRule -DisplayName "Allow HTTP" -Direction Inbound -LocalPort 80 -Protocol TCP -Action Allow

# Allow HTTPS (if configured)
New-NetFirewallRule -DisplayName "Allow HTTPS" -Direction Inbound -LocalPort 443 -Protocol TCP -Action Allow

# Allow Unifi API communication
New-NetFirewallRule -DisplayName "Allow Unifi API" -Direction Outbound -RemoteAddress 172.16.0.2 -RemotePort 8443 -Protocol TCP -Action Allow
```

## ✅ Verification Checklist

- [ ] .NET Framework 4.8 installed
- [ ] IIS installed with ASP.NET support
- [ ] Application pool "AuthorizeUnifi" created and running
- [ ] Website "lepalmier.local" created and running
- [ ] Files deployed to `C:\inetpub\wwwroot\AuthorizeUnifi`
- [ ] Web.config configured with Unifi settings
- [ ] Logs directory created with proper permissions
- [ ] Portal accessible at `http://lepalmier.local`
- [ ] Portal connects to Unifi Controller successfully
- [ ] Guest authorization works

## 🚀 Starting the Service

```powershell
# Start Application Pool
Start-WebAppPool -Name "AuthorizeUnifi"

# Start Website
Start-Website -Name "lepalmier.local"

# Verify
Get-WebsiteState -Name "lepalmier.local"
```

## 🛑 Stopping the Service

```powershell
# Stop Website
Stop-Website -Name "lepalmier.local"

# Stop Application Pool
Stop-WebAppPool -Name "AuthorizeUnifi"
```

## 🔄 Restart Service

```powershell
# Quick restart
Restart-WebAppPool -Name "AuthorizeUnifi"
```

## Troubleshooting Installation

### Issue: ".NET CLR Version v4.0.30319 not available"
```
Solution: Verify .NET 4.8 installation:
reg query "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full"
```

### Issue: "Access Denied" when copying files
```
Solution: Run PowerShell as Administrator:
Right-click PowerShell → Run as Administrator
```

### Issue: "Website won't start"
```
Solution: Check IIS logs:
C:\inetpub\logs\LogFiles\W3SVC1\
```

### Issue: "Cannot connect to Unifi Controller"
```
Solution:
1. Verify Unifi IP is reachable: ping 172.16.0.2
2. Check credentials in Web.config
3. Verify port 8443 is open: Test-NetConnection -ComputerName 172.16.0.2 -Port 8443
```

## 📞 Support

If installation fails, check:
1. Event Viewer → Windows Logs → Application
2. Application logs: `C:\Logs\AuthorizeUnifi\`
3. IIS logs: `C:\inetpub\logs\LogFiles\`

---

**Installation Guide Version:** 1.0  
**Last Updated:** 2024-01-15
