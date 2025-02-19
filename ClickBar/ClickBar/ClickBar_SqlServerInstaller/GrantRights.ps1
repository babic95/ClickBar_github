# Pronađi trenutno prijavljenog korisnika
$currentUser = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name

function Grant-UserRights {
    param (
        [string]$user,
        [string]$right
    )

    try {
        # Dobij SID za korisnika
        $sid = (New-Object System.Security.Principal.NTAccount($user)).Translate([System.Security.Principal.SecurityIdentifier]).Value
        # Dobij trenutna prava iz registra
        $rights = (Get-ItemProperty "HKLM:\SYSTEM\CurrentControlSet\Control\Lsa" -Name $right).$right

        if ($rights -notcontains $sid) {
            Write-Output "Granting $right to $user"
            $rights += ",$sid"
            Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\Lsa" -Name $right -Value $rights
        } else {
            Write-Output "$user already has $right"
        }
    } catch {
        Write-Error "Failed to grant $right to $user: $_"
    }
}

# Dodaj prava trenutnom korisniku
Grant-UserRights -user $currentUser -right "SeBackupPrivilege"    # The right to back up files and directories
Grant-UserRights -user $currentUser -right "SeSecurityPrivilege"  # The right to manage auditing and the security log
Grant-UserRights -user $currentUser -right "SeDebugPrivilege"     # The right to debug programs