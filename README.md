# AIZ

AIZ is a Windows task planner built with WinUI 3 and .NET 10. Tasks and settings are stored locally under `%LOCALAPPDATA%\ToDoListApp`.

## Windows x64

Download the complete `AIZ-win-x64.zip` release asset, extract it, and run `AIZ.exe`. Keep the other files next to the executable.

To build from source with the .NET 10 SDK:

```powershell
dotnet publish .\ToDoListApp.csproj -c Release -r win-x64 -p:Platform=x64 -p:PublishReadyToRun=false -p:PublishTrimmed=false
```

## Sign-in and data

Guest mode stores tasks locally. The email/password form is a local prototype: it does not verify credentials with a server and must not be treated as authentication. Google sign-in uses OAuth with PKCE and does not include a client secret in the application.

Tasks do not sync between devices. The Google sign-in flow has not been verified on a clean Windows account. A formerly embedded OAuth client secret should be revoked or rotated before public distribution.
