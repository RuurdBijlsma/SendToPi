# SendToSFTP
Send a file to an server via SFTP and the Send to menu in Windows
## Usage
Setup connection.json at the same location as SendToSFTP.exe, should look like this:
```
{
    "Ip": "192.168.0.141",
    "User": "pi",
    "Password": "raspberry",
    "DestinationPath": "/home/pi/ftp/files/",
    "Multithreaded": false
}
```
The multithreaded option can be a bit unstable.

To add it to the Send To menu in Windows, press Win + R and type `shell:sendto`, this will open an explorer window. Paste a shortcut to the SendToSFTP.exe here. Renaming it will rename it in the Send to menu.

## Compile
First make sure you have the [dotnet core sdk](https://www.microsoft.com/net/download/windows) installed

In SendToSftp (the project folder) execute the following command to build
```
dotnet publish -c release --self-contained --runtime win10-x64
```
Replace `win10-x64` with anything from [here](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog)
