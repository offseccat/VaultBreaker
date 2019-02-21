# VaultBreaker
A toolset designed for attacks against common password managers.

VaultBreaker is a Proof of Concept that demonstrates different attacks that can be performed against password managers. It supports 3 main functions. 
  - Clipboard Event Hooking
  - Memory Parsing 
  - Proxying

Please Note: This Proof of Concept is currently in Alpha and as such has limited support. I plan on increasing support as I find more ways to parse manager memory.

# Clipboard Event Hooking
Based on the code from SharpClipboard located at https://github.com/justinbui/SharpClipboard. This will spin up a listener and listen for clipboard events from specific windows, (e.g. BitWarden, LastPass, 1Pass, password, login, etc etc) and will output the clipboard content when triggered.

Usage:
```
VaultBreaker.exe -Monitor
```
You should see the following when a clipboard event is triggered:
```
[+] Starting Clipboard Monitor
[+] Clipboard Retrieved!
    Window Name: Bitwarden
    Text: Cammy

[+] Clipboard Retrieved!
    Window Name: Bitwarden
    Text: cAxr6BypHIhs
```

# Proxying
(In Development) This feature is still in testing, as I'm not 100% sure how useful it is. A few password managers are built as Electron applications. These Electron applications support specific Debug flags which allow you to proxy traffic through a host you control, while ignoring any SSL errors you may encounter. Specifically, VaultBreaker will launch an HTTP/HTTPS proxy in memory using FiddlerCore. It will then attempt to wait for the computer screen to be locked (This code most likely will need to be improved). It will then "bounce" the application and re-launch it with the following flags: --proxy-server=http://127.0.0.1:8888 --ignore-certificate-errors

Theoretically this will be useful in cases where sensitive information is transferred between the client and servers, currently it only checks for HTTP POST requests that contain a "password" string in it. However as more research into potential use cases is done, I expect to have more use out of this.

To launch execute the command
```
VaultBreaker.exe -Proxy -Manager "ManagerName"
```
Update 2/21/2019: Something seems to have broken, I'm looking into it now.

# Memory Parsing
The main portion of this application that I'm hoping to expand. In some of the common password managers, you can sometimes find strings that contain the cleartext password in memory. (Ex. 1Password will sometimes have a JSON string that contains the string master-password followed by the cleartext master password). In some cases, these strings persist even in the case of the vault becoming locked and can be parsed by anyone who is able to open a handle to the process. This code will attempt to parse for those strings and if found, will attempt to extract the password (or at least some of the memory where it thinks the password is). Please note: this isn't possible for all Password Managers due to the fact that some of them have the passwords stored in places where it's difficult to parse for it. I hope to find better ways to parse the memory and recover these strings.

To use this functionality execute the command:
```
VaultBreaker.exe -GetMaster -Manager "ManagerName"
```
You should see the following output:
```
[+] Checking for Bitwarden Executables.
[DEBUG] Number of Processes Found: 3
[DEBUG] Starting Memory Dump
[DEBUG] Opening Process
[DEBUG] OpenProcess Returned
[DEBUG] Process Handle isn't 0
[SUCCESS] Potential Bitwarden Password Location found! Sup3r S3cret P@ssw0rd
```


Currently Supported Password Managers
  - BitWarden
  - DashLane
  - 1Password

If you're looking to gather information from KeePass 2.X databases, you can download Harmj0ys KeeThief located at https://github.com/HarmJ0y/KeeThief


# Requirements
VaultBreaker currently has the following requirements. 
- FiddlerCore (https://www.telerik.com/fiddler/fiddlercore)
- PowerArgs (https://github.com/adamabdelhamed/PowerArgs)

I plan on releasing a self-contained binary that will be compatible with C2 frameworks that support .NET Assembly executions.
