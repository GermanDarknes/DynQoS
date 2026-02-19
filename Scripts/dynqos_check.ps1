while ($true) {
	Clear-Host
	Get-NetQosPolicy -Name "Dynamic QoS - *" | Select-Object Name, AppPathNameMatchCondition, DSCPAction | Format-Table -AutoSize
	Start-Sleep 5
}