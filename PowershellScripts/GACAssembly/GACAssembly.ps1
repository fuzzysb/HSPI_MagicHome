<#
.SYNOPSIS
	Install or uninstall an assembly in the GAC.
   
.DESCRIPTION
	This script is used to install and uninstall an assembly in the global assembly cache (GAC).
	This script is called from a post-build event in a Visual Studio C# project.

.PARAMETER $ProjectAssemblyFilePath
	Identifies the assembly to be installed in the GAC.  
	The file path includes both the directory and the file name of the solution file. 
	
	Example: 
		E:\Users\Public\My Documents\Visual Studio 2013\Projects\Obfuscation\Solution\Projects\StrongNamedAssembly\bin\Debug\StrongNamedAssembly.dll

.PARAMETER $InstallAssemblyInGAC
	This switch specifies whether an assembly is installed or uninstalled from the GAC. 
	If this swith is added to the command line then the assembly is installed in the GAC, otherwise
	the assembly is uninstalled from the GAC.

.EXAMPLE
	set powerShellFilePath=%systemroot%\sysnative\windowspowershell\v1.0\powershell.exe
	set powerShellScriptPath=$(SolutionDir)PowerShellScripts\GACAssembly\GACAssembly.ps1
	"%powerShellFilePath%" -File "%powerShellScriptPath%" "$(TargetPath)" -InstallAssemblyInGAC
	if errorlevel 1 exit 1

#>

<#
	===================================================================================================
	===================================================================================================
	Script input arguments (i.e. command line variables)
	===================================================================================================
	===================================================================================================
#>

param(
	[parameter (Mandatory=$true)] 
	[ValidateNotNullOrEmpty()]
	[string] $ProjectAssemblyFilePath,

	[parameter (Mandatory=$true)] 
	[ValidateNotNullOrEmpty()]
	[string] $SolutionPlatform,

	[switch]$InstallAssemblyInGAC
)

<#
	===================================================================================================
	===================================================================================================
	Exported functions
	===================================================================================================
	===================================================================================================
#>

function ImportModulesForThisScript
{
	[CmdletBinding()]
	param()

	$scriptDirectory = $PSScriptRoot

	$module = Join-Path -path $scriptDirectory -ChildPath "..\Utility\Utility.psm1"
	Import-Module $module -Force 
}

function ValidateVariablesDefinedInThisScript
{
	[CmdletBinding()]
	param()

	if( $InstallAssemblyInGAC.IsPresent ) {
		Utility_AssertValid_FilePath $ProjectAssemblyFilePath
	}
}

<#
	===================================================================================================
	===================================================================================================
	Start of script
	===================================================================================================
	===================================================================================================
#>

Clear-Host
$ScriptDirectory = $PSScriptRoot
Set-Location $ScriptDirectory

[int] $exitCode = 0
$oldVerbose = $VerbosePreference
$VerbosePreference = "SilentlyContinue" # "SilentlyContinue" "continue" 

try {
	ImportModulesForThisScript
	ValidateVariablesDefinedInThisScript
	Utility_GACAssembly $ProjectAssemblyFilePath $SolutionPlatform $InstallAssemblyInGAC.IsPresent
}
catch {
	Write-Output $_
	$exitCode = 1
}
finally {
	Remove-Module *
	Remove-Variable * -Exclude "exitCode" -ErrorAction SilentlyContinue
	$error.Clear(); 
	$VerbosePreference = $oldVerbose
	exit $exitCode
}