#define MyAppName "Sleepy"
#define Version "1.0.0"
#define Publisher "Pixel"
#define AppExeName "UI.exe"
#define TargetFramework "net8.0-windows10.0.26100.0"

[Setup]
AppId={5DD15D58-4785-4E87-BC68-599FF19D2CE9}
AppName={#MyAppName}
AppVersion={#Version}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
Uninstallable=yes
OutputDir=Output
OutputBaseFilename=SleepyInstaller
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
DisableProgramGroupPage=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; GroupDescription: "Additional icons:"; Flags: unchecked

[Files]
Source: "UI\bin\Release\Publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; DestName: "Sleepy.exe"

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#AppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
