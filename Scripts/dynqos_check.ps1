while ($true) {
	Clear-Host
	Get-NetQosPolicy | Select-Object Name | Format-Table -AutoSize
	Start-Sleep 5
}