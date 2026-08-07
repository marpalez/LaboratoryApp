; Instalador de LumenLab. Se compila con Inno Setup 6 sobre entrega\salida\, que la
; genera publicar.ps1.
;
; DECISIONES QUE NO SON DE GUSTO:
;
;   · Instalación POR USUARIO, en %LOCALAPPDATA%. En Archivos de programa haría falta
;     administrador, y entonces el equipo no podría actualizarse solo — que es justo lo
;     que se ha montado. Un pinchazo de admin al principio (el runtime) y ninguno más.
;
;   · El acceso directo apunta al LANZADOR, no al programa. El lanzador mira si el
;     laboratorio ha publicado una versión más nueva, se la trae y arranca. Si apuntara
;     al programa, habría que volver a los seis equipos con cada entrega.
;
;   · La extensión .lmnlab también la abre el lanzador, por lo mismo. El programa
;     reenvía el fichero a la ventana que ya esté abierta (UnaSolaInstancia).
;
;   · El instalador NO trae la carpeta compartida configurada. La pregunta el programa
;     la primera vez que arranca, que es donde el técnico ya la elige hoy.

#define Nombre        "LumenLab"
#define Autor         "David Martínez Palomares (DMP)"
#define Web           "https://davidmarpalez.com"
#define ExeLanzador   "LumenLab.Lanzador.exe"
#define Salida        "salida"

; Sale del propio ejecutable, para no tener el número escrito en dos sitios.
#define Version GetVersionNumbersString(Salida + "\programa\LumenLab.exe")

[Setup]
AppId={{8E1D4C7A-3F52-4B9E-9D26-4C7A1F0B5E33}
AppName={#Nombre}
AppVersion={#Version}
AppPublisher={#Autor}
AppPublisherURL={#Web}
AppSupportURL={#Web}

; Por usuario: sin UAC, y así la actualización automática puede escribir.
PrivilegesRequired=lowest
DefaultDirName={localappdata}\{#Nombre}
DefaultGroupName={#Nombre}
DisableProgramGroupPage=yes
DisableDirPage=yes

OutputDir=.
OutputBaseFilename=LumenLab-{#Version}-instalador
SetupIconFile=..\src\LumNotas.App\LumenLab.ico
UninstallDisplayIcon={app}\{#ExeLanzador}
UninstallDisplayName={#Nombre} {#Version}
WizardStyle=modern
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "es"; MessagesFile: "compiler:Languages\Spanish.isl"

[Files]
; El lanzador vive en la carpeta de instalación y casi nunca cambia.
Source: "{#Salida}\lanzador\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

; La primera versión del programa, para que el equipo funcione antes de haber hablado
; con la carpeta compartida. A partir de ahí las versiones las trae el lanzador solo.
Source: "{#Salida}\programa\*"; DestDir: "{localappdata}\{#Nombre}\versiones\{#Version}"; \
    Flags: ignoreversion recursesubdirs

[Icons]
Name: "{userprograms}\{#Nombre}"; Filename: "{app}\{#ExeLanzador}"
Name: "{userdesktop}\{#Nombre}"; Filename: "{app}\{#ExeLanzador}"; Tasks: escritorio

[Tasks]
Name: "escritorio"; Description: "Crear un acceso directo en el escritorio"; \
    GroupDescription: "Accesos directos:"

[Registry]
; Doble clic sobre una toma de notas. Va en HKCU porque la instalación es por usuario.
Root: HKCU; Subkey: "Software\Classes\.lmnlab"; ValueType: string; \
    ValueName: ""; ValueData: "LumenLab.TomaDeNotas"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\LumenLab.TomaDeNotas"; ValueType: string; \
    ValueName: ""; ValueData: "Toma de notas de ensayo"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\LumenLab.TomaDeNotas\DefaultIcon"; ValueType: string; \
    ValueName: ""; ValueData: "{app}\{#ExeLanzador},0"
Root: HKCU; Subkey: "Software\Classes\LumenLab.TomaDeNotas\shell\open\command"; ValueType: string; \
    ValueName: ""; ValueData: """{app}\{#ExeLanzador}"" ""%1"""

[Run]
Filename: "{app}\{#ExeLanzador}"; Description: "Abrir {#Nombre}"; \
    Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Las versiones que haya ido trayendo el lanzador. Los AJUSTES Y LA CACHÉ NO SE BORRAN:
; están en %APPDATA%\LumNotas y guardan las carpetas del laboratorio, que cuesta más
; volver a elegir que reinstalar. Desinstalar no es empezar de cero.
Type: filesandordirs; Name: "{localappdata}\{#Nombre}\versiones"

[Code]
{
  Sin el .NET 10 Desktop Runtime el programa no arranca, y el error que da Windows no
  dice qué falta. Se comprueba antes de instalar y se manda a descargarlo.

  No se instala solo a propósito: el runtime pide administrador y este instalador no lo
  tiene. Pedirlo convertiría cada actualización en una llamada a informática.
}
{
  Cualquier 10.x vale, así que se busca por patrón y no por el número exacto: el runtime
  se actualiza solo por Windows Update y hoy es el 10.0.10, no el 10.0.0.
}
function HayRuntime: Boolean;
var
  carpeta: String;
  encontrado: TFindRec;
begin
  Result := False;
  carpeta := ExpandConstant('{commonpf64}\dotnet\shared\Microsoft.WindowsDesktop.App');

  if not FindFirst(carpeta + '\10.*', encontrado) then Exit;

  try
    repeat
      if (encontrado.Attributes and FILE_ATTRIBUTE_DIRECTORY) <> 0 then
      begin
        Result := True;
        Exit;
      end;
    until not FindNext(encontrado);
  finally
    FindClose(encontrado);
  end;
end;

function InitializeSetup: Boolean;
var
  respuesta: Integer;
  error: Integer;
begin
  Result := True;
  if HayRuntime then Exit;

  respuesta := MsgBox(
    'LumenLab necesita el .NET 10 Desktop Runtime (x64) y este equipo no lo tiene.' + #13#10#13#10 +
    'Se instala una sola vez y hace falta un administrador.' + #13#10#13#10 +
    '¿Abrir la página de descarga?',
    mbConfirmation, MB_YESNO);

  if respuesta = IDYES then
    ShellExec('open', 'https://dotnet.microsoft.com/es-es/download/dotnet/10.0',
              '', '', SW_SHOW, ewNoWait, error);

  Result := False;
end;
