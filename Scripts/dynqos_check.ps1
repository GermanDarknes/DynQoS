while ($true) {
	Clear-Host
	Get-NetQosPolicy -Name "Dynamic QoS - *" | Select-Object Name, AppPathNameMatchCondition, DSCPAction
	Start-Sleep 5
}