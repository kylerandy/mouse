; -- LittleBigMouse.iss --
;#define AppVer GetFileVersion('..\bin\x64\Release\LittleBigMouse.exe')
#define AppVer '1.0-beta3.1'

[Setup]
AppName=Little Big Mouse
AppVersion=AppVer
DefaultDirName={pf}\LittleBigMouse
DefaultGroupName=Little Big Mouse
UninstallDisplayIcon={app}\LittleBigMouse.exe
Compression=lzma2
SolidCompression=yes
OutputDir="."
ArchitecturesInstallIn64BitMode=x64
OutputBaseFilename=LittleBigMouse_{#AppVer}

[Files]
Source: "..\bin\x64\Release\LittleBigMouse.exe"; DestDir: "{app}"; Check: Is64BitInstallMode
Source: "..\bin\x64\Release\Microsoft.Win32.TaskScheduler.dll"; DestDir: "{app}"; Check: Is64BitInstallMode
Source: "..\bin\x64\Release\MouseKeyboardActivityMonitor.dll"; DestDir: "{app}"; Check: Is64BitInstallMode
Source: "..\bin\x86\Release\LittleBigMouse.exe"; DestDir: "{app}"; Check: not Is64BitInstallMode
Source: "..\bin\x86\Release\Microsoft.Win32.TaskScheduler.dll"; DestDir: "{app}"; Check: not Is64BitInstallMode
Source: "..\bin\x86\Release\MouseKeyboardActivityMonitor.dll"; DestDir: "{app}"; Check: not Is64BitInstallMode

[Icons]
Name: "{group}\Little Big Mouse"; Filename: "{app}\LittleBigMouse.exe"

[Run]
Filename: {app}\LittleBigMouse.exe; Description: Run Application; Flags: postinstall nowait skipifsilent runascurrentuser

[UninstallRun]
Filename: "{app}\LittleBigMouse.exe"; Parameters: "--exit"
Filename: "{app}\LittleBigMouse.exe"; Parameters: "--unschedule"