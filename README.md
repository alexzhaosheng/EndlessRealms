# Endless Realms
The goal of this game is to leverage the powerful capabilities of AI to create a game world with maximum freedom and rich diversity.

## Usage
### Using the Released Package
- Download the release package corresponding to your platform (https://github.com/alexzhaosheng/EndlessRealms/releases)
- Extract the contents of the package.
- Run EndlessRealms.WebUi.exe
- Open your browser, access http://localhost:5000/
- Enjoy
### Build by yourself
- Download and install .NET 7 SDK (https://dotnet.microsoft.com/en-us/download)
- Download and extract the source code or git clone
- Run dotnet publish to build and publish.
  * Windows
  ```
  dotnet publish EndlessRealms.GUI\EndlessRealms.WebUi.csproj -c Release -o publish --no-self-contained --runtime win-x64 
  ```
  * Linux
  ```
  dotnet publish EndlessRealms.GUI/EndlessRealms.WebUi.csproj -c Release -o publish --no-self-contained --runtime linux-x64
  ```
  * MacOS X86
  ```
  dotnet publish EndlessRealms.GUI/EndlessRealms.WebUi.csproj -c Release -o publish --no-self-contained --runtime osx-x64
  ```  
  * MacOS ARM64
  ```
  dotnet publish EndlessRealms.GUI/EndlessRealms.WebUi.csproj -c Release -o publish --no-self-contained --runtime osx-arm64
  ```  
- Run EndlessRealms.WebUi.exe
- Open your browser, access http://localhost:5000/
- Enjoy
