# Roda os testes das regras do jogo (Assets/Scripts/Logica) SEM o Unity e sem
# o .NET SDK, usando o compilador C# que já vem no Windows (.NET Framework 4).
# Uso, na raiz do projeto:
#   powershell -ExecutionPolicy Bypass -File Ferramentas/TestesSemUnity/rodar_testes.ps1
# Os mesmos testes aparecem no Unity em Window > General > Test Runner (EditMode).

$ErrorActionPreference = "Stop"
$raiz = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$csc = Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) {
    $csc = Join-Path $env:WINDIR "Microsoft.NET\Framework\v4.0.30319\csc.exe"
}
if (-not (Test-Path $csc)) {
    Write-Error "Não encontrei o csc.exe do .NET Framework 4. Use 'dotnet test Ferramentas/TestesSemUnity' (precisa do .NET SDK)."
}

$saida = Join-Path $env:TEMP "CincoEstrelasTestes"
New-Item -ItemType Directory -Force $saida | Out-Null
$exe = Join-Path $saida "TestesLogica.exe"

$fontes = @()
$fontes += Get-ChildItem (Join-Path $raiz "Assets\Scripts\Logica") -Filter *.cs -Recurse | ForEach-Object { $_.FullName }
$fontes += Get-ChildItem (Join-Path $raiz "Assets\Tests\EditMode\Logica") -Filter *.cs -Recurse | ForEach-Object { $_.FullName }
$fontes += (Join-Path $PSScriptRoot "MiniNUnit.cs")

Write-Host "Compilando $($fontes.Count) arquivos..."
& $csc -nologo -codepage:65001 -langversion:5 -warn:4 -nowarn:1591 "-out:$exe" $fontes
if ($LASTEXITCODE -ne 0) {
    Write-Host "Falhou a compilação." -ForegroundColor Red
    exit $LASTEXITCODE
}

Push-Location $raiz
try {
    & $exe
    $codigo = $LASTEXITCODE
}
finally {
    Pop-Location
}
exit $codigo
