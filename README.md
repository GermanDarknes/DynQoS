# DynQoS - Dynamic QoS Injector

## What is DynQoS?
The Dynamic QoS Injector is a tool that automatically adds Windows QoS policies, based on running processes. Designed primarily for gaming on Telekom MagentaZuhause Hybrid connections.

## How does it work?
DynQoS downloads Discord's detectable applications list and extracts game executables. It monitors running processes and creates per-executable QoS policies on demand when something is matching. Every start it cleans all rules it created, to keep the policy list clean.

## Configuration Files
All files go next to `DynQoS.exe`:

- **`qos_discord.txt`**
  - Auto-generated from Discord API
  - File name without extension, one per line

- **`qos_include.txt`**
  - Manually maintained include list
  - File name with or without extension, one per line

- **`qos_exclude.txt`**
  - Manually maintained exclude list
  - File name with or without extension, one per line

### Examples
See `Examples` folder for `qos_include.txt` and `qos_exclude.txt` samples.

## Preparation
Adjust registry values by running `dynqos_preparation.reg` from the `Scripts` folder.

## Usage
1. Place `DynQoS.exe` in a folder of your choice, e.g.: `C:\Tools\DynQoS\`
2. (Optional) Add `C:\Tools\DynQoS\qos_include.txt` & `C:\Tools\DynQoS\qos_exclude.txt`
3. Run `DynQoS.exe`

## Verifying QoS Rules - Powershell
Show all QoS-Policies:
```powershell
Get-NetQosPolicy | Select-Object Name, AppPathNameMatchCondition, DSCPAction
```

Show DynQoS added QoS-Policies only:
```powershell
Get-NetQosPolicy -Name "Dynamic QoS - *" | Select-Object Name, AppPathNameMatchCondition, DSCPAction
```

A watch script is available in the `Scripts` folder.

## Verifying QoS Markings - Wireshark
1. Use `ip.dsfield.dscp == 1` as filter
2. Optional: Add a column for `ip.dsfield.dscp`
3. See if theres any traffic

## Build

### Requirements
- .NET SDK 10.0

### Build
Single-File Publish
```bash
dotnet publish -p:PublishDir=.\Publish
```
