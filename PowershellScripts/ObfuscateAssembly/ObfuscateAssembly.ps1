<#
.SYNOPSIS
	Obfuscate an assembly using .NetReactor
   
.DESCRIPTION
	This script is used to obfuscate an assembly and is to be called from a post-build event 
	in a Visual Studio project.

	An assembly is obfuscated using .Net Reactor
	
.PARAMETER $SolutionFilePath
	Identifies the Visual Studio solution file that reference this PowerShell script. 
	The file path includes both the directory and the file name of the solution file. 

	Example:
		E:\Users\Public\My Documents\Visual Studio 2013\Projects\Obfuscation\Solution\Obfuscation.sln

.PARAMETER $ProjectAssemblyFilePath
	Identifies the assembly to be obfuscated.  
	The file path includes both the directory and the file name of the projects assembly. 
	
	Example: 
		E:\Users\Public\My Documents\Visual Studio 2013\Projects\Obfuscation\Solution\Projects\PrivateAssembly\bin\Debug\PrivateAssembly.dll


.EXAMPLE
	The following is an example of a post build event that can be added to a Visual Studio C# project:

		set powerShellFilePath=%systemroot%\sysnative\windowspowershell\v1.0\powershell.exe

		if /I $(ConfigurationName) neq ObfuscatedRelease goto lDoNotObfuscate

		rem obfuscate the assembly
		set powerShellScriptPath=$(SolutionDir)PowerShellScripts\ObfuscateAssembly\ObfuscateAssembly.ps1
		"%powerShellFilePath%" -File "%powerShellScriptPath%" "$(SolutionPath)" "$(TargetPath)" 
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
	[string] $SolutionFilePath,

	[parameter (Mandatory=$true)] 
	[ValidateNotNullOrEmpty()]
	[string] $ProjectAssemblyFilePath
)

<#
	===================================================================================================
	===================================================================================================
	Exported functions
	===================================================================================================
	===================================================================================================
#>

function BackupProjectOutputDirectory
{
	[CmdletBinding()]
	param()

	Utility_BackupProjectOutputDirectory $ProjectAssemblyDirectory
}

function CopyObfuscatedAssemblyToProjectOutputDirectory
{
	[CmdletBinding()]
	param()

	$obfuscatedFilePathPattern = Join-Path -Path $ObfuscatedAssemblyDirectory -ChildPath $ProjectFileNamePattern
	Utility_CopyFiles $obfuscatedFilePathPattern $projectAssemblyDirectory
}


function ImportModulesForThisScript
{
	[CmdletBinding()]
	param()

	$scriptDirectory = $PSScriptRoot

	$module = Join-Path -path $scriptDirectory -ChildPath "..\Utility\Utility.psm1"
	Import-Module $module -Force 
}

function ObfuscateProjectAssembly
{
	[CmdletBinding()]
	param()

	Utility_ObfuscateProjectAssembly $NetReactorFilePath `
        $ProjectAssemblyFilePath `
		$ObfuscatedAssemblyFilePath 
}

function ValidateVariablesDefinedInThisScript
{
	[CmdletBinding()]
	param()

	Utility_AssertValid_Directory $SolutionDirectory
	Utility_AssertValid_FilePath $ProjectAssemblyFilePath
	Utility_AssertValid_FilePath $NetReactorFilePath
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
	$SolutionDirectory = Split-Path -Path $SolutionFilePath -Parent
	$NetReactorFilePath = "E:\Program Files (x86)\Eziriz\.NET Reactor\dotNET_Reactor.exe"
	$projectAssemblyDirectory = Split-Path -Path $ProjectAssemblyFilePath -Parent
	$projectAssemblyFileName = Split-Path -Path $ProjectAssemblyFilePath -Leaf
	$ProjectFileNamePattern = [System.IO.Path]::GetFileNameWithoutExtension( $ProjectAssemblyFilePath ) + ".*"
	$ProjectAssemblyFilePathPattern = Join-Path -Path $projectAssemblyDirectory -ChildPath $ProjectFileNamePattern
	$ObfuscatedAssemblyDirectory = Join-Path -Path $projectAssemblyDirectory -ChildPath "Confused"
	$ObfuscatedAssemblyFilePath = Join-Path -Path $ObfuscatedAssemblyDirectory -ChildPath $projectAssemblyFileName

	ImportModulesForThisScript
	ValidateVariablesDefinedInThisScript
	ObfuscateProjectAssembly
	BackupProjectOutputDirectory
	CopyObfuscatedAssemblyToProjectOutputDirectory
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