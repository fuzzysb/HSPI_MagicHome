<#
.SYNOPSIS
	Disable strong name verification for a delay signed assembly.
   
.DESCRIPTION
	This script instructs the CLR runtime to disable strong name verification for a delay signed assembly.
	
.PARAMETER $ProjectAssemblyFilePath
	Identifies the assembly for which strong name verification is to be disabled.
	The file path includes both the directory and the file name of the projects assembly. 
	
	Example: 
		E:\Users\Public\My Documents\Visual Studio 2013\Projects\Obfuscation\Solution\Projects\DelaySignedAssembly\bin\Debug\DelaySignedAssembly.dll

.PARAMETER $DisableStrongNameVerification
	If this switch is included on the command line then the strong name verification for an assembly is disabled,
	otherwise strong name verification is enabled.

.EXAMPLE
	set powerShellFilePath=%systemroot%\sysnative\windowspowershell\v1.0\powershell.exe
	set powerShellScriptPath=$(SolutionDir)PowerShellScripts\SetAssemblyStrongNameVerificationState\SetAssemblyStrongNameVerificationState.ps1
	"%powerShellFilePath%" -File "%powerShellScriptPath%" "$(TargetPath)" -DisableStrongNameVerification
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
	[string] $SolutionPlatform,

	[switch] $DisableStrongNameVerification

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

	Utility_AssertValid_FilePath $ProjectAssemblyFilePath
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
	Utility_SetAssemblyStrongNameVerificationState $ProjectAssemblyFilePath  $SolutionPlatform $DisableStrongNameVerification.IsPresent 
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