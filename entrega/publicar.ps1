<#
    Genera lo que hay que repartir. NO instala nada y NO toca la carpeta compartida:
    solo deja en entrega\salida\ el programa y el lanzador listos para el instalador.

    Publicar al laboratorio se hace DESDE EL PROGRAMA —«Ayuda | Acerca de | Publicar»—,
    que copia esta instalación a la carpeta compartida y escribe el marcador al final.
    Se hace desde dentro a propósito: se reparte lo que un equipo ya está usando y
    funciona, no lo que acaba de salir de una compilación que nadie ha abierto.

        .\entrega\publicar.ps1
        .\entrega\publicar.ps1 -Version 1.0.1

    Sin -Version se usa la del .csproj, que es la que manda.
#>

[CmdletBinding()]
param(
    [string] $Version,
    [string] $Salida = (Join-Path $PSScriptRoot "salida")
)

$ErrorActionPreference = "Stop"

$raiz = Split-Path $PSScriptRoot -Parent
$app = Join-Path $raiz "src\LumNotas.App\LumNotas.App.csproj"
$lanzador = Join-Path $raiz "src\LumNotas.Lanzador\LumNotas.Lanzador.csproj"

# Marco de trabajo aparte y no autocontenido: 1,5 MB en vez de 134. Cada equipo lleva el
# .NET 10 Desktop Runtime instalado una vez, y a cambio cada actualización pesa lo que un
# correo — que es lo que hace viable repartir por OneDrive a seis equipos.
$comunes = @("-c", "Release", "-r", "win-x64", "--self-contained", "false", "--nologo")
if ($Version) { $comunes += "-p:Version=$Version" }

if (Test-Path $Salida) { Remove-Item $Salida -Recurse -Force }

$programa = Join-Path $Salida "programa"
$lanzadorSalida = Join-Path $Salida "lanzador"

Write-Host "Publicando el programa..." -ForegroundColor Cyan
dotnet publish $app @comunes -o $programa
if ($LASTEXITCODE -ne 0) { throw "Falló la publicación del programa." }

Write-Host "Publicando el lanzador..." -ForegroundColor Cyan
dotnet publish $lanzador @comunes -o $lanzadorSalida
if ($LASTEXITCODE -ne 0) { throw "Falló la publicación del lanzador." }

# Los símbolos no se reparten: duplican lo que hay que sincronizar y no los usa el técnico.
Get-ChildItem $Salida -Recurse -Filter *.pdb | Remove-Item -Force

$exe = Join-Path $programa "LumenLab.exe"
$publicada = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($exe).ProductVersion
$peso = [math]::Round(((Get-ChildItem $programa -Recurse -File | Measure-Object Length -Sum).Sum / 1MB), 1)

Write-Host ""
Write-Host "Versión  : $publicada"           -ForegroundColor Green
Write-Host "Programa : $programa  ($peso MB)"
Write-Host "Lanzador : $lanzadorSalida"
Write-Host ""
Write-Host "Siguiente paso: compilar entrega\LumenLab.iss con Inno Setup." -ForegroundColor Yellow
