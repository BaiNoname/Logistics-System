; ============================================================================
;  Bộ cài TruckingSystem Desktop — Inno Setup 6 (https://jrsoftware.org/isdl.php)
;  Đặt file này ở:  Logistics-System-Desktop\installer\TruckingSystem.iss
;  Trước khi build: chạy publish.bat (ra thư mục ..\publish\Desktop)
;  Tùy chọn: đặt MicrosoftEdgeWebview2Setup.exe (tải ở https://go.microsoft.com/fwlink/p/?LinkId=2124703)
;            cạnh file này -> máy chưa có WebView2 sẽ được cài tự động.
; ============================================================================
#define AppName      "TruckingSystem"
#define AppVersion   "1.0.2"
#define AppPublisher "Nhóm Thiện Nhân – Hoàng Minh"
#define AppExe       "TruckingSystem.Desktop.exe"
#define PublishDir   "..\publish\Desktop"

[Setup]
AppId={{6D3F2A41-8C5B-4E7A-9B1D-2F4C8E7A1B53}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppVerName={#AppName} {#AppVersion}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir=..\publish\Installer
OutputBaseFilename=TruckingSystem_Setup_{#AppVersion}
; SetupIconFile=TruckingSystem.ico   (bo dau ; neu co file icon dat canh file nay)
UninstallDisplayIcon={app}\{#AppExe}
Compression=lzma2/ultra64
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
WizardStyle=modern
CloseApplications=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Tạo biểu tượng ngoài màn hình Desktop"; GroupDescription: "Biểu tượng:"

[Files]
; Toàn bộ thư mục publish (exe + các dll + thư mục runtimes), bỏ file .pdb
Source: "{#PublishDir}\*"; DestDir: "{app}"; Excludes: "*.pdb"; Flags: ignoreversion recursesubdirs createallsubdirs
; Bộ cài WebView2 (nếu có đặt cạnh file .iss)
Source: "MicrosoftEdgeWebview2Setup.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall skipifsourcedoesntexist; Check: NeedsWebView2

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{tmp}\MicrosoftEdgeWebview2Setup.exe"; Parameters: "/silent /install"; StatusMsg: "Đang cài Microsoft Edge WebView2..."; Check: NeedsWebView2 and WebView2SetupExists; Flags: waituntilterminated
Filename: "{app}\{#AppExe}"; Description: "Mở {#AppName} ngay"; Flags: nowait postinstall skipifsilent

[Code]
const
  WebView2Key = 'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}';
  WebView2KeyUser = 'Software\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}';

function NeedsWebView2: Boolean;
var v: String;
begin
  Result := not ((RegQueryStringValue(HKLM, WebView2Key, 'pv', v) and (v <> '') and (v <> '0.0.0.0')) or
                 (RegQueryStringValue(HKCU, WebView2KeyUser, 'pv', v) and (v <> '') and (v <> '0.0.0.0')));
end;

function WebView2SetupExists: Boolean;
begin
  Result := FileExists(ExpandConstant('{tmp}\MicrosoftEdgeWebview2Setup.exe'));
end;
