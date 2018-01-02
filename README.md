# SendToPi
Send a file to an server via SFTP and the Send to menu in Windows
## Usage
Setup connection.json at the same location as SendToPi.exe, should look like this:
```
{
	"Ip": "192.168.0.141",
	"User": "pi",
	"Password": "raspberry",
	"DestinationPath": "/home/pi/ftp/files/"
}
```
To add it to the `Send To` menu in Windows, press Win + R and type `shell:sendto`, this will open an explorer window. Paste a shortcut to the SendToPi.exe here. Renaming it will rename it in the Send to menu.
