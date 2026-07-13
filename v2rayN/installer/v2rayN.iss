#ifndef MyAppVersion
  #define MyAppVersion "7.23.6"
#endif

#ifndef SourceDir
  #define SourceDir "..\artifacts\installer\v7.23.6\package"
#endif

#ifndef OutputDir
  #define OutputDir "..\artifacts\installer\v7.23.6\output"
#endif

#define MyAppName "v2rayN"
#define MyAppExeName "v2rayN.exe"
#define MyAppPublisher "qhs200312"
#define MyAppUrl "https://github.com/qhs200312/v2rayN"

[Setup]
AppId={{BC0ED823-5089-4F4D-9E42-4739BFE3D562}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppUrl}
AppSupportURL={#MyAppUrl}/issues
AppUpdatesURL={#MyAppUrl}/releases
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} installer
VersionInfoProductName={#MyAppName}
DefaultDirName={localappdata}\Programs\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.19041
OutputDir={#OutputDir}
OutputBaseFilename=v2rayN-windows-64-setup
SetupIconFile=..\v2rayN.WinUI\Assets\v2rayN.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
CloseApplications=force
CloseApplicationsFilter={#MyAppExeName},AmazTool.exe
RestartApplications=no
UsePreviousAppDir=yes
SetupLogging=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Excludes: "guiConfigs\*,guiLogs\*,binConfigs\*,*.db,*.db-shm,*.db-wal"; Flags: ignoreversion recursesubdirs createallsubdirs overwritereadonly

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent
