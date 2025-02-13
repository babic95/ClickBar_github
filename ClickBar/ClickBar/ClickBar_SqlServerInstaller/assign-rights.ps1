param (
    [string]$username
)

$rights = @(
    "SeBackupPrivilege",
    "SeSecurityPrivilege",
    "SeDebugPrivilege"
)

$tempFilePath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "secedit.inf")
$seceditContent = @"
[Unicode]
Unicode=yes
[Version]
signature="\$CHICAGO\$"
Revision=1
[Privilege Rights]
SeBackupPrivilege = *S-1-5-32-544,*S-1-5-21-0-0-0-1000
SeSecurityPrivilege = *S-1-5-32-544,*S-1-5-21-0-0-0-1000
SeDebugPrivilege = *S-1-5-32-544,*S-1-5-21-0-0-0-1000
"@

$seceditContent = $seceditContent -replace "\*S-1-5-21-0-0-0-1000", "$username"

Set-Content -Path $tempFilePath -Value $seceditContent

try {
    Invoke-Expression "secedit /configure /db secedit.sdb /cfg $tempFilePath /areas USER_RIGHTS"
    Write-Output "Successfully assigned privileges to $username"
} catch {
    Write-Output "Failed to assign privileges to $username"
    throw $_
} finally {
    Remove-Item -Path $tempFilePath -Force
    Remove-Item -Path (Join-Path ([System.IO.Path]::GetTempPath()) "secedit.sdb") -Force
}