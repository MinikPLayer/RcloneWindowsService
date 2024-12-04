# Rclone Windows Service
Windows service daemon for the rclone mount command.

## Requirements
You need the [rclone](https://rclone.org/downloads/) command visible from the **system** PATH variable.

## Usage
- From the [releases](https://github.com/MinikPLayer/RcloneWindowsService/releases/latest) page download RcloneWindowsService.exe and install.ps1.
- Put them into a permament directory of your choosing (For example **C:\\Program Files\\rclone**). Copy the directory path.
- Open powershell as administrator. Navigate to the target directory with the `cd {directory}` command.
- Run `./install.ps1 "Service Name" "remote:/directory L:`, replacing *Service Name* with your service name, *remote:/directory* with the name of your remote / directory and *L:* with the name of the letter for the remote to be mounted to.

## Uninstallation
- Open powershell as administrator in the target directory as instructed in *Usage*.
- Run `./install.ps1 "Service Name" "" -Uninstall`. Service will be uninstalled.

## Advanced
- You can add custom *rclone mount* [parameters](https://rclone.org/commands/rclone_mount/) to the parameters section (**remote:/directory L:** in Usage) of the install script invocation.
- Run `get-help ./install.ps1` to get more info about the install script and it's features.

## Debugging
- Errors will be logged to the Event Viewer under `Windows Logs/Application` directory.

## Planned
- GUI installer.
