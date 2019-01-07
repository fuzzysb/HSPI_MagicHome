<#
.DESCRIPTION
	This script contains shared functions used by the other PowerShell scripts in this Visual Studio solution.
#>


<#
	================================================================================================
	================================================================================================
	Exported functions
	================================================================================================
	================================================================================================
#>

function Utility_AssertValid_Directory
{
	[CmdletBinding()]
	param(
		[parameter (Mandatory=$true)] 
		[ValidateNotNullOrEmpty()]
		[string]$directory
	)

	if( !( Test-Path -Path $directory ) ) {
		throw "cannot find directory: $directory"
	}
}

function Utility_AssertValid_FilePath
{
	[CmdletBinding()]
	param(
		[parameter (Mandatory=$true)] 
		[ValidateNotNullOrEmpty()]
		[string]$filePath
	)

	if( !( Test-Path -Path $filePath ) ) {
		throw "cannot find file: $filePath"
	}
}

function Utility_BackupDirectory
{
	[CmdletBinding()]
	param(
		[parameter (Mandatory=$true)] 
		[ValidateNotNullOrEmpty()]
		[string] $sourceFilePathPattern,

		[parameter (Mandatory=$true)] 
		[ValidateNotNullOrEmpty()]
		[string] $destinationDirectory
	)

	$sourceDirectory = Split-Path -Path $sourceFilePathPattern -Parent
	Utility_AssertValid_Directory $sourceDirectory

	if( !( Test-Path -Path $destinationDirectory ) ) {
		New-Item $destinationDirectory -ItemType directory 
		Utility_AssertValid_Directory $destinationDirectory
	}

	Utility_CopyFiles $sourceFilePathPattern $destinationDirectory
}

function Utility_BackupProjectOutputDirectory
{
	[CmdletBinding()]
	param(
		[parameter (Mandatory=$true)] 
		[ValidateNotNullOrEmpty()]
		[string] $projectAssemblyDirectory
	)

	Utility_AssertValid_Directory $projectAssemblyDirectory

	$sourceFilePathPattern = Join-Path -Path $projectAssemblyDirectory -ChildPath "*.*"
	$destinationDirectory = Join-Path -Path $projectAssemblyDirectory -ChildPath "NonObfuscatedAssemblyBackup"

	Utility_BackupDirectory $sourceFilePathPattern $destinationDirectory
}

function Utility_CopyFiles
{
	[CmdletBinding()]
	param(
		[parameter (Mandatory=$true)] 
		[ValidateNotNullOrEmpty()]
		[string] $sourceFilePathPattern,

		[parameter (Mandatory=$true)] 
		[ValidateNotNullOrEmpty()]
		[string] $destinationDirectory
	)

	$sourceDirectory = Split-Path -Path $sourceFilePathPattern -Parent
	Utility_AssertValid_Directory $sourceDirectory
	Utility_AssertValid_Directory $destinationDirectory

	Write-Verbose "Utility_CopyFiles: sourceFilePathPattern = $sourceFilePathPattern"
	Write-Verbose "Utility_CopyFiles: destinationDirectory = $destinationDirectory"
	Copy-Item $sourceFilePathPattern $destinationDirectory -Force
}

function Utility_GACAssembly
{
	[CmdletBinding()]
	param(
		[parameter (Mandatory=$true)] 
		[ValidateNotNullOrEmpty()]
		[string] $assemblyFilePath,

		[parameter (Mandatory=$true)] 
		[ValidateNotNullOrEmpty()]
		[string] $solutionPlatform,

		[parameter (Mandatory=$true)] 
		[boolean] $installAssemblyInGAC

	)

	$gacFilePath = _getGACProgramFilePath $solutionPlatform

	if( $true -eq $installAssemblyInGAC ) {
		Utility_AssertValid_FilePath $assemblyFilePath
		& $gacFilePath "-if" $assemblyFilePath
	}
	else {
		& $gacFilePath "-u" $assemblyFilePath 
	}
}

function Utility_ObfuscateProjectAssembly
{
	[CmdletBinding()]
	param(
		[parameter (Mandatory=$true)] 
		[ValidateNotNullOrEmpty()]
		[string] $NetReactorFilePath,

		[parameter (Mandatory=$true)] 
		[ValidateNotNullOrEmpty()]
		[string] $ProjectAssemblyFilePath,

		[parameter (Mandatory=$true)] 
		[ValidateNotNullOrEmpty()]
		[string] $obfuscatedAssemblyFilePath
	)

	Utility_AssertValid_FilePath $NetReactorFilePath

	if( Test-Path -Path $obfuscatedAssemblyFilePath ) {
		Remove-Item -Path $obfuscatedAssemblyFilePath
	}
	& $NetReactorFilePath -file $ProjectAssemblyFilePath -targetfile $obfuscatedAssemblyFilePath  -q -mono 1 -exception_handling 1 -compression 1 -showloadingscreen 0 -admin 0 -embed 0 -antitamp 1 -control_flow_obfuscation 0 -flow_level 1 -nativeexe 1 -necrobit 1 -necrobit_comp 1 -obfuscation 1 -exclude_enums 1 -obfuscate_public_types 0 -prejit 1 -resourceencryption 1 -antistrong 1 -stringencryption 0 -suppressildasm 1 -licensing_behaviour 1 -evaluationenable 0 -expirationdate_enable 0 -number_of_uses_enable 0 -number_of_uses_enable 0 -shownotfoundscreen 1 -number_of_instances_enable 0 -shownagscreen 0 -run_without_licensefile 1 -shutdown_process 1
    Start-Sleep -s 10

	Utility_AssertValid_FilePath $obfuscatedAssemblyFilePath 
}

function Utility_SetAssemblyStrongNameVerificationState
{
	[CmdletBinding()]
	param(
		[parameter (Mandatory=$true)] 
		[ValidateNotNullOrEmpty()]
		[string] $assemblyFilePath,

		[parameter (Mandatory=$true)] 
		[ValidateNotNullOrEmpty()]
		[string] $solutionPlatform,

		[parameter (Mandatory=$true)] 
		[boolean] $disableStrongNameVerification

	)
	
	Utility_AssertValid_FilePath $assemblyFilePath
	$strongNamefilePath = _getSNProgramFilePath $solutionPlatform

	if( $true -eq $disableStrongNameVerification ) {
		# register for verification skipping
		& $strongNamefilePath "-Vr" $assemblyFilePath
		Write-Verbose "Utility_SetAssemblyStrongNameVerificationState: $strongNamefilePath -Vr $assemblyFilePath"
	}
	else {
		# un-register for verification skipping
		& $strongNamefilePath "-Vu" $assemblyFilePath 
		Write-Verbose "Utility_SetAssemblyStrongNameVerificationState: $strongNamefilePath -Vu $assemblyFilePath"
	}
}

function Utility_SignAssembly
{
	[CmdletBinding()]
	param(
		[parameter (Mandatory=$true)] 
		[ValidateNotNullOrEmpty()]
		[string] $assemblyFilePath,

		[parameter (Mandatory=$true)] 
		[ValidateNotNullOrEmpty()]
		[string] $strongNameKeyFilePath,

		[parameter (Mandatory=$true)] 
		[ValidateSet( "AnyCPU", "Any CPU", "x86", "x64", "Win32" )]
		[string] $solutionPlatform
	)

	Utility_AssertValid_FilePath $assemblyFilePath
	Utility_AssertValid_FilePath $strongNameKeyFilePath
	
	$filePath = _getSNProgramFilePath $solutionPlatform
	& $filePath "-R" $assemblyFilePath $strongNameKeyFilePath 
}

<#
	================================================================================================
	================================================================================================
	local variables
	================================================================================================
	================================================================================================
#>
[string] $_windowsSdkDirectory = "C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6 Tools\"

<#
	================================================================================================
	================================================================================================
	local functions
	================================================================================================
	================================================================================================
#>

function _enableConfuserExProjectRuleProtections
{
	[CmdletBinding()]
	param(
		[xml]$fileContents,
		[boolean] $enableConfuserExRefProxyProtection,
		[boolean] $enableConfuserExRefResourceProtection
	)

	$rule = $fileContents.project.rule
	if( !$rule.HasChildNodes ) {
		return
	}
	
	<#
		The ConfuserEx template project file is configured so a rule protection
		is enabled by removing the rule protection from the template file.
	#> 
	$nodes = $rule.ChildNodes
	for( $x = 0;  $x -lt $nodes.Count;  $x++ ) {
		if( $enableConfuserExRefProxyProtection ) {
			if( $nodes[$x].id -contains "ref proxy" ) {
				$rule.RemoveChild( $nodes[$x] )
			}
		}

		if( $enableConfuserExRefResourceProtection ) {
			if( $nodes[$x].id -contains "resources" ) {
				$rule.RemoveChild( $nodes[$x] )
			}
		}
	}
}

function _getGACProgramFilePath
{
	[CmdletBinding()]
	param(
		[parameter (Mandatory=$true)] 
		[ValidateSet( "AnyCPU", "Any CPU", "x86", "x64", "Win32" )]
		[string] $solutionPlatform
	)
	
	$filePathX86 = Join-Path -Path $_windowsSdkDirectory -ChildPath "gacutil.exe"
	$filePathX64 = Join-Path -Path $_windowsSdkDirectory -ChildPath "x64\gacutil.exe"

	[string] $filePath
	switch( $solutionPlatform ) 
	{
		"AnyCPU" { $filePath = $filePathX64 }
		"Any CPU" { $filePath = $filePathX64 }
		"x64" {  $filePath = $filePathX64 }
		default { $filePath = $filePathX86 }
	}
	
	Utility_AssertValid_FilePath $filePath
	Write-Verbose "GAC program file path: $filePath" 
	$filePath
}

function _getSNProgramFilePath
{
	[CmdletBinding()]
	param(
		[parameter (Mandatory=$true)] 
		[ValidateSet( "AnyCPU", "Any CPU", "x86", "x64", "Win32" )]
		[string] $solutionPlatform
	)
	$filePathX86 = Join-Path -Path $_windowsSdkDirectory -ChildPath "sn.exe"
	$filePathX64 = Join-Path -Path $_windowsSdkDirectory -ChildPath "x64\sn.exe"

	[string] $filePath
	switch( $solutionPlatform ) 
	{
		"AnyCPU" { $filePath = $filePathX64 }
		"Any CPU" { $filePath = $filePathX64 }
		"x64" {  $filePath = $filePathX64 }
		default { $filePath = $filePathX86 }
	}
	Utility_AssertValid_FilePath $filePath
	Write-Verbose "strong name program file path: $filePath" 
	$filePath
}


