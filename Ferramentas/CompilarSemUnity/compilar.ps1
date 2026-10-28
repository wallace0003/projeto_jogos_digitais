# Compila TODO o código do jogo (runtime, Editor e testes) fora do Unity, contra
# uma versão simplificada da API do Unity 6 que fica nesta pasta: só as assinaturas
# que o jogo usa, sem implementação, separadas nas mesmas DLLs do Unity
# (UnityEngine, UnityEngine.UI, Unity.InputSystem, UnityEngine.TestRunner e UnityEditor).
# Cada assembly do jogo é montado a partir do seu .asmdef: os scripts da pasta, as
# referências, as plataformas e as defineConstraints. Assim, uma referência que falte
# num .asmdef também dá erro aqui, como daria no Unity.
# Compila duas vezes: como no Editor (UNITY_EDITOR, com os testes) e como numa build
# do jogo (sem UNITY_EDITOR e sem os assemblies só de Editor).
# Pega erros de C# sem abrir o Unity; a palavra final é do Unity 6000.5.10f1.
# Usa o compilador C# que já vem no Windows (.NET Framework 4, C# 5).
# Uso, na raiz do projeto:
#   powershell -ExecutionPolicy Bypass -File Ferramentas/CompilarSemUnity/compilar.ps1

$ErrorActionPreference = "Continue"
$raiz = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$aqui = $PSScriptRoot
$csc = Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) {
    $csc = Join-Path $env:WINDIR "Microsoft.NET\Framework\v4.0.30319\csc.exe"
}
if (-not (Test-Path $csc)) {
    Write-Host "Não encontrei o csc.exe do .NET Framework 4." -ForegroundColor Red
    exit 1
}
$saida = Join-Path $env:TEMP "CincoEstrelasCompilacao"
$base = @("-nologo", "-codepage:65001", "-langversion:5", "-warn:4", "-nowarn:1591,0414,0169", "-target:library")

# API simplificada: assembly do Unity -> arquivo desta pasta, seguido das referências dele
$api = [ordered]@{
    "UnityEngine"            = @("UnityEngine.cs")
    "UnityEngine.UI"         = @("UnityEngine.UI.cs", "UnityEngine")
    "Unity.InputSystem"      = @("Unity.InputSystem.cs", "UnityEngine", "UnityEngine.UI")
    "UnityEngine.TestRunner" = @("UnityEngine.TestRunner.cs", "UnityEngine")
    "UnityEditor"            = @("UnityEditor.cs", "UnityEngine")
}
# assemblies do Unity citados nos .asmdef cuja API o jogo não usa
$semApi = @("UnityEditor.TestRunner")

# assemblies do jogo: cada .asmdef fica com os .cs da pasta dele, menos as subpastas que têm outro .asmdef
$asmdefs = @{}
foreach ($arq in Get-ChildItem (Join-Path $raiz "Assets") -Filter *.asmdef -Recurse) {
    $asmdefs[$arq.DirectoryName] = Get-Content $arq.FullName -Raw | ConvertFrom-Json
}
$fontes = @{}
foreach ($cs in Get-ChildItem (Join-Path $raiz "Assets") -Filter *.cs -Recurse) {
    $pasta = $cs.DirectoryName
    while ($pasta -and -not $asmdefs.ContainsKey($pasta)) { $pasta = Split-Path $pasta -Parent }
    if (-not $pasta) {
        Write-Host "Script fora de qualquer .asmdef: $($cs.FullName)" -ForegroundColor Red
        exit 1
    }
    $nome = $asmdefs[$pasta].name
    if (-not $fontes.ContainsKey($nome)) { $fontes[$nome] = @() }
    $fontes[$nome] += $cs.FullName
}
$nomesDoJogo = @($asmdefs.Values | ForEach-Object { $_.name })

$falhou = $false
function Erro($texto) {
    Write-Host "   ERRO: $texto" -ForegroundColor Red
    $script:falhou = $true
}

function Compilar($titulo, $nome, $argumentos) {
    Write-Host "== $titulo"
    $dll = Join-Path $pastaSaida "$nome.dll"
    & $csc @base "-out:$dll" @argumentos | Out-Host
    if ($LASTEXITCODE -ne 0) {
        Erro "não compilou: $nome"
    } else {
        $dlls[$nome] = $dll
        Write-Host "   ok"
    }
}

foreach ($variante in @("editor", "build")) {
    $editor = $variante -eq "editor"
    $defines = if ($editor) { @("UNITY_EDITOR", "UNITY_INCLUDE_TESTS", "ENABLE_INPUT_SYSTEM") } else { @("ENABLE_INPUT_SYSTEM") }
    $define = "-define:" + ($defines -join ";")
    $pastaSaida = Join-Path $saida $variante
    New-Item -ItemType Directory -Force $pastaSaida | Out-Null
    $dlls = @{}

    foreach ($nome in $api.Keys) {
        if ($nome -eq "UnityEditor" -and -not $editor) { continue }
        $info = $api[$nome]
        $refs = @()
        for ($i = 1; $i -lt $info.Count; $i++) { $refs += "-r:" + $dlls[$info[$i]] }
        Compilar "[$variante] API do Unity: $nome" $nome (@($define) + $refs + @(Join-Path $aqui $info[0]))
    }
    if ($editor) {
        Compilar "[$variante] NUnit (MiniNUnit)" "nunit.framework" @($define, (Join-Path $raiz "Ferramentas\TestesSemUnity\MiniNUnit.cs"))
    }

    # assemblies do jogo, na ordem das dependências
    $pendentes = @($asmdefs.Values)
    $foraDaVariante = @()
    $naoCompilados = @()
    while ($pendentes.Count -gt 0) {
        $restantes = @()
        foreach ($a in $pendentes) {
            $fora = (-not $editor) -and (@($a.includePlatforms) -contains "Editor")
            foreach ($c in @($a.defineConstraints)) {
                if ($c -and ($defines -notcontains $c)) { $fora = $true }
            }
            if ($fora) {
                $foraDaVariante += $a.name
                continue
            }
            $refs = @()
            $esperando = $false
            $dependeDeFalha = $false
            foreach ($r in @($a.references)) {
                if (-not $r) { continue }
                if ($dlls.ContainsKey($r)) { $refs += "-r:" + $dlls[$r] }
                elseif ($naoCompilados -contains $r) { $dependeDeFalha = $true }
                elseif ($foraDaVariante -contains $r -or $api.Contains($r)) { Erro "$($a.name) referencia $r, que não existe na variante $variante" }
                elseif ($nomesDoJogo -contains $r) { $esperando = $true }
                elseif ($semApi -notcontains $r) { Erro "$($a.name) referencia '$r', que não existe" }
            }
            if ($esperando) {
                $restantes += $a
                continue
            }
            if ($dependeDeFalha) {
                Write-Host "== [$variante] $($a.name): não compilado (depende de um assembly que falhou)" -ForegroundColor Red
                $naoCompilados += $a.name
                continue
            }
            if (-not $a.noEngineReferences) {
                $refs += "-r:" + $dlls["UnityEngine"]
                if ($editor) { $refs += "-r:" + $dlls["UnityEditor"] }
            }
            if ($a.overrideReferences) {
                foreach ($p in @($a.precompiledReferences)) {
                    if (-not $p) { continue }
                    if ($p -eq "nunit.framework.dll" -and $dlls.ContainsKey("nunit.framework")) { $refs += "-r:" + $dlls["nunit.framework"] }
                    else { Erro "$($a.name): referência pré-compilada desconhecida '$p'" }
                }
            }
            if (-not $fontes.ContainsKey($a.name)) {
                Write-Host "== [$variante] $($a.name): sem scripts"
                continue
            }
            Compilar "[$variante] $($a.name)" $a.name (@($define) + $refs + $fontes[$a.name])
            if (-not $dlls.ContainsKey($a.name)) { $naoCompilados += $a.name }
        }
        if ($restantes.Count -eq $pendentes.Count) {
            Erro ("referências que nunca se resolvem entre: " + (($restantes | ForEach-Object { $_.name }) -join ", "))
            break
        }
        $pendentes = $restantes
    }
}

if ($falhou) {
    Write-Host "Falhou a compilação." -ForegroundColor Red
    exit 1
}
Write-Host "Tudo compilou (Editor e build)."
exit 0
