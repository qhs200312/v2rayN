#ifndef MyAppVersion
  #define MyAppVersion "7.23.7"
#endif

#ifndef SourceDir
  #define SourceDir "..\artifacts\installer\v7.23.7\package"
#endif

#ifndef OutputDir
  #define OutputDir "..\artifacts\installer\v7.23.7\output"
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

[CustomMessages]
english.LegacyInstallFound=An existing portable installation was found at:%n%n%s%n%nSetup will upgrade this directory. Existing profiles, settings, logs, and databases will be preserved.
chinesesimplified.LegacyInstallFound=检测到已有便携版目录：%n%n%s%n%n安装程序将覆盖升级此目录，现有节点、设置、日志和数据库都会保留。

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Excludes: "guiConfigs\*,guiLogs\*,binConfigs\*,*.db,*.db-shm,*.db-wal"; Flags: ignoreversion recursesubdirs createallsubdirs overwritereadonly

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent

[Code]
const
  UninstallKey = 'Software\Microsoft\Windows\CurrentVersion\Uninstall';
  CurrentAppKey = '{BC0ED823-5089-4F4D-9E42-4739BFE3D562}_is1';

function IsExistingInstallDirectory(const Directory: String): Boolean;
begin
  Result := (Directory <> '')
    and FileExists(AddBackslash(Directory) + '{#MyAppExeName}')
    and DirExists(AddBackslash(Directory) + 'guiConfigs');
end;

function FindRegisteredInstall(const RootKey: Integer): String;
var
  KeyNames: TArrayOfString;
  DisplayName: String;
  InstallLocation: String;
  Index: Integer;
begin
  Result := '';
  if not RegGetSubkeyNames(RootKey, UninstallKey, KeyNames) then
    Exit;

  for Index := 0 to GetArrayLength(KeyNames) - 1 do
  begin
    if RegQueryStringValue(RootKey, UninstallKey + '\' + KeyNames[Index], 'DisplayName', DisplayName)
      and (CompareText(Trim(DisplayName), '{#MyAppName}') = 0)
      and RegQueryStringValue(RootKey, UninstallKey + '\' + KeyNames[Index], 'InstallLocation', InstallLocation)
      and IsExistingInstallDirectory(InstallLocation) then
    begin
      Result := InstallLocation;
      Exit;
    end;
  end;
end;

function FindPortableInstall: String;
var
  Candidate: String;
  Drive: String;
  DriveCode: Integer;
  CurrentInstallLocation: String;
begin
  Result := '';

  Candidate := ExpandConstant('{param:LEGACYDIR|}');
  if IsExistingInstallDirectory(Candidate) then
  begin
    Result := Candidate;
    Exit;
  end;

  if RegQueryStringValue(HKCU, UninstallKey + '\' + CurrentAppKey, 'InstallLocation', CurrentInstallLocation)
    and IsExistingInstallDirectory(CurrentInstallLocation) then
  begin
    Result := CurrentInstallLocation;
    Exit;
  end;

  Candidate := FindRegisteredInstall(HKCU);
  if Candidate = '' then
    Candidate := FindRegisteredInstall(HKLM64);
  if Candidate = '' then
    Candidate := FindRegisteredInstall(HKLM32);
  if Candidate <> '' then
  begin
    Result := Candidate;
    Exit;
  end;

  Candidate := ExpandConstant('{src}');
  if IsExistingInstallDirectory(Candidate) then
  begin
    Result := Candidate;
    Exit;
  end;

  for DriveCode := Ord('C') to Ord('Z') do
  begin
    Drive := Chr(DriveCode) + ':\';
    if not DirExists(Drive) then
      Continue;

    Candidate := Drive + 'v2';
    if IsExistingInstallDirectory(Candidate) then
    begin
      Result := Candidate;
      Exit;
    end;

    Candidate := Drive + '{#MyAppName}';
    if IsExistingInstallDirectory(Candidate) then
    begin
      Result := Candidate;
      Exit;
    end;

    Candidate := Drive + 'Apps\{#MyAppName}';
    if IsExistingInstallDirectory(Candidate) then
    begin
      Result := Candidate;
      Exit;
    end;
  end;
end;

procedure InitializeWizard;
var
  LegacyDirectory: String;
begin
  { Respect an explicit /DIR used by administrators and automated deployments. }
  if ExpandConstant('{param:DIR|}') <> '' then
    Exit;

  LegacyDirectory := FindPortableInstall;
  if LegacyDirectory = '' then
    Exit;

  WizardForm.DirEdit.Text := LegacyDirectory;
  SuppressibleMsgBox(
    Format(CustomMessage('LegacyInstallFound'), [LegacyDirectory]),
    mbInformation,
    MB_OK,
    IDOK);
end;
