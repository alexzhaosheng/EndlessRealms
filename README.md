# Endless Realms
The goal of this game is to leverage the powerful capabilities of AI to create a game world with maximum freedom and rich diversity.

## Usage
### Using the Released Package
- Download and install .NET 7 (https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- Download the release package corresponding to your platform (https://github.com/alexzhaosheng/EndlessRealms/releases)
- Extract the contents of the package.
- Obtain a ChatGPT API key from here: https://platform.openai.com/account/api-keys
- Insert the API key into the appSettings.json file under ChatGpt:ApiKey.
- Run EndlessRealms.Gui.exe
- Enjoy
### Build by yourself
- Download and install .NET 7 SDK (https://dotnet.microsoft.com/en-us/download)
- Download and extract the source code or git clone
- Run dotnet publish to build and publish.
  * Windows
  ```
  dotnet publish EndlessRealms.GUI\EndlessRealms.Gui.csproj -c Release -o publish --no-self-contained --runtime win-x64 
  ```
  * Linux
  ```
  dotnet publish EndlessRealms.GUI/EndlessRealms.Gui.csproj -c Release -o publish --no-self-contained --runtime linux-x64
  ```
  * MacOS X86
  ```
  dotnet publish EndlessRealms.GUI/EndlessRealms.Gui.csproj -c Release -o publish --no-self-contained --runtime osx-x64
  ```  
  * MacOS X86
  ```
  dotnet publish EndlessRealms.GUI/EndlessRealms.Gui.csproj -c Release -o publish --no-self-contained --runtime osx-arm64
  ```  
- Obtain a ChatGPT API key from here: https://platform.openai.com/account/api-keys
- Insert the API key into the appSettings.json file under ChatGpt:ApiKey.
- Run EndlessRealms.Gui.exe
- Enjoy
