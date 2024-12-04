<#
.DESCRIPTION
    Installs the service
.PARAMETER Name
    Name of the service to be installed.
.PARAMETER Unattended
	Disables the PAUSE after installation.
.PARAMETER StartMode
	Service start mode. Possible values: boot, system, auto, demand, disabled, delayed-auto
.PARAMETER Params
	Additional parameters to be passed to the rclone mount command.
.PARAMETER Uninstall
	Uninstall instead of installing.
.EXAMPLE
	C:\PS> .\install.ps1 -Name "Google Drive" -Params "gdrive:/ G: --rc"
.EXAMPLE
    C:\PS> .\install.ps1 -Name "Google Drive"
	Installs the service as "Google Drive".
.EXAMPLE
    C:\PS> .\install.ps1 -Name "Something cloud" -Unattended
	Installs the service as "Something cloud" without a PAUSE prompt.
.EXAMPLE
    C:\PS> .\install.ps1 -Name "Something cloud" -Uninstall
	Uninstalls the service.
.NOTES
    Author: Michał Tomecki
    Date:   04.12.2024    
#>
param(
	[Parameter(mandatory=$true)]
	[string] $Name,

	[Parameter(mandatory=$true)]
	[string] $Params,

	[ValidateSet("boot", "system", "auto", "demand", "disabled", "delayed-auto")]
	[string] $StartMode = "auto",

	[switch] $Unattended = $false,
	[switch] $Uninstall = $false
)

$location = "$PSScriptRoot\RcloneWindowsService.exe"
if (!(Test-Path $location)) {
	Write-Host "The file $location does not exist. Perhaps project is not compiled?"
	Write-Host "Exiting"
	exit
}

$serviceName = "rclone-" + $Name.ToLower().Replace(' ', '-')
$displayName = "[rclone] $Name"
$formattedParams = ($Params -replace "'", '"') -replace '"', '\"'

if ($Uninstall) {
	Write-Host "Uninstalling service..."
	Write-Host "- Service Name: $serviceName"

	if (!$Unattended) {
		Read-Host -Prompt "Press any key to continue or CTRL+C to quit" | Out-Null
	}

	sc.exe stop $serviceName
	sc.exe delete $serviceName

	if (!$unattended) {
		PAUSE
	}
	exit  
}

echo "Installing service..."
echo "- Service Name: $serviceName"
echo "- Display Name: $displayName"
echo "- Full service command: rclone mount $formattedParams"
echo "- Location: $location"

if (!$Unattended) {
	Read-Host -Prompt "Press any key to continue or CTRL+C to quit" | Out-Null
}

sc.exe create $serviceName binpath= "$location $formattedParams" start= $StartMode DisplayName= $displayName
sc.exe start $serviceName

if (!$unattended) {
	PAUSE
}
