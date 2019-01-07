<#
.SYNOPSIS
	Sign an assembly with the private key from a strong name key file.
   
.DESCRIPTION
	This script is used to sign a strong named assembly with the private key from a strong name key file
	and is called from a post-build event in a Visual Studio project.
	
.PARAMETER $SolutionFilePath
	Identifies the Visual Studio solution file that reference this PowerShell script. 
	The file path includes both the directory and the file name of the solution file. 

	Example:
		E:\Users\Public\My Documents\Visual Studio 2013\Projects\Obfuscation\Solution\Obfuscation.sln

.PARAMETER $ProjectAssemblyFilePath
	Identifies the assembly to be obfuscated.  
	The file path includes both the directory and the file name of the projects assembly. 
	
	Example: 
		E:\Users\Public\My Documents\Visual Studio 2013\Projects\Obfuscation\Solution\Projects\StrongNamedAssembly\bin\Debug\StrongNamedAssembly.dll

.PARAMETER $StrongNamePrivateKeyFilePath
	Identifies the strong name key file that contains the private key.
	
	Example: 
		E:\Users\Public\My Documents\Visual Studio 2013\Projects\Obfuscation\Solution\StrongNameKeyFiles\PublicPrivateKeys.snk

.EXAMPLE
	The following is an example of a post build event that can be added to a Visual Studio C# project:

		set powerShellFilePath=%systemroot%\sysnative\windowspowershell\v1.0\powershell.exe

		if /I $(ConfigurationName) neq ObfuscatedRelease goto lDoNotObfuscate

		rem obfuscate the assembly
		set powerShellScriptPath=$(SolutionDir)PowerShellScripts\ObfuscateAssembly\ObfuscateAssembly.ps1
		"%powerShellFilePath%" -File "%powerShellScriptPath%" "$(SolutionPath)" "$(TargetPath)" 
		if errorlevel 1 exit 1

		rem re-sign the obfuscated assembly
		set powerShellScriptPath=$(SolutionDir)PowerShellScripts\SignAssembly\SignAssembly.ps1
		set keyFilePath=$(ProjectDir)PublicPrivateKeys.snk
		"%powerShellFilePath%" -File "%powerShellScriptPath%" "$(TargetPath)" "%keyFilePath%"
		if errorlevel 1 exit 1

		:lDoNotObfuscate
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
	[string] $StrongNamePrivateKeyFilePath,

	[parameter (Mandatory=$true)] 
	[string] $SolutionPlatform
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

function SignAssemblyInProjectOutputDirectory
{
	[CmdletBinding()]
	param()

	Utility_SignAssembly $ProjectAssemblyFilePath $StrongNamePrivateKeyFilePath $SolutionPlatform
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
	SignAssemblyInProjectOutputDirectory
}
catch {
	Write-Output $_
	$exitCode = 1
}
finally {
	Remove-Variable * -Exclude "exitCode" -ErrorAction SilentlyContinue
	Remove-Module *
	$error.Clear(); 
	$VerbosePreference = $oldVerbose
	exit $exitCode
}