# -*- coding: utf-8 -*-
"""
Gera as 3 cenas do jogo (Menu, Suecia1958 e FimDeJogo) e a lista de cenas da
Build (ProjectSettings/EditorBuildSettings.asset).

Cada cena tem só o essencial: a câmera ortográfica (com o componente do URP),
a Global Light 2D e um objeto com o script que monta o resto em tempo de
execução (MenuPrincipal, ConstrutorFase ou TelaFinal). Os scripts são
referenciados pelo GUID do .meta deles (veja gerar_metas.py).

Uso (na raiz do projeto, depois de gerar_metas.py):  py Ferramentas/gerar_cenas.py
"""
import hashlib
import io
import os
import re

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# GUIDs de componentes e materiais do pacote URP (iguais em todo projeto Unity 6)
GUID_CAMERA_URP = "a79441f348de89743a2939f4d699eac1"   # UniversalAdditionalCameraData
GUID_LUZ_2D = "073797afb82c5a1438f328866b10b3f0"       # Light2D
GUID_SPRITE_LIT = "a97c105638bdf8b4a8650670310a4cd3"   # Sprite-Lit-Default
GUID_SPRITE_UNLIT = "9dfc825aed78fcd4ba02077103263b40"  # Sprite-Unlit-Default
GUID_ACOES_INPUT = "2bcd2660ca9b64942af0de543d8d7100"   # Assets/Settings/InputSystem_Actions.inputactions

CENAS = [
    # (nome, script, nome do objeto, campos extras, cor de fundo da câmera)
    ("Menu", "Assets/Scripts/Interface/MenuPrincipal.cs", "Menu Principal",
     ["creditos: Protótipo da disciplina de Jogos Digitais · arte, música e código originais"],
     (0.42, 0.62, 0.86)),
    ("Suecia1958", "Assets/Scripts/Fase/ConstrutorFase.cs", "Fase (Suécia 1958)",
     ["arquivoFase: Suecia1958"],
     (0.42, 0.62, 0.86)),
    ("FimDeJogo", "Assets/Scripts/Interface/TelaFinal.cs", "Tela Final",
     [],
     (0.12, 0.16, 0.3)),
]


def guid_do_meta(caminho_rel):
    meta = os.path.join(RAIZ, caminho_rel.replace("/", os.sep) + ".meta")
    with io.open(meta, encoding="utf-8") as f:
        m = re.search(r"^guid: ([0-9a-f]{32})", f.read(), re.M)
    if not m:
        raise SystemExit("GUID não encontrado em " + meta)
    return m.group(1)


def guid_de(caminho_relativo):
    return hashlib.md5(("cincoestrelas:" + caminho_relativo).encode("utf-8")).hexdigest()


CABECALHO = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!29 &1
OcclusionCullingSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_OcclusionBakeSettings:
    smallestOccluder: 5
    smallestHole: 0.25
    backfaceThreshold: 100
  m_SceneGUID: 00000000000000000000000000000000
  m_OcclusionCullingData: {fileID: 0}
--- !u!104 &2
RenderSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 9
  m_Fog: 0
  m_FogColor: {r: 0.5, g: 0.5, b: 0.5, a: 1}
  m_FogMode: 3
  m_FogDensity: 0.01
  m_LinearFogStart: 0
  m_LinearFogEnd: 300
  m_AmbientSkyColor: {r: 0.212, g: 0.227, b: 0.259, a: 1}
  m_AmbientEquatorColor: {r: 0.114, g: 0.125, b: 0.133, a: 1}
  m_AmbientGroundColor: {r: 0.047, g: 0.043, b: 0.035, a: 1}
  m_AmbientIntensity: 1
  m_AmbientMode: 3
  m_SubtractiveShadowColor: {r: 0.42, g: 0.478, b: 0.627, a: 1}
  m_SkyboxMaterial: {fileID: 0}
  m_HaloStrength: 0.5
  m_FlareStrength: 1
  m_FlareFadeSpeed: 3
  m_HaloTexture: {fileID: 0}
  m_SpotCookie: {fileID: 10001, guid: 0000000000000000e000000000000000, type: 0}
  m_DefaultReflectionMode: 0
  m_DefaultReflectionResolution: 128
  m_ReflectionBounces: 1
  m_ReflectionIntensity: 1
  m_CustomReflection: {fileID: 0}
  m_Sun: {fileID: 0}
  m_IndirectSpecularColor: {r: 0, g: 0, b: 0, a: 1}
  m_UseRadianceAmbientProbe: 0
--- !u!157 &3
LightmapSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 12
  m_GIWorkflowMode: 1
  m_GISettings:
    serializedVersion: 2
    m_BounceScale: 1
    m_IndirectOutputScale: 1
    m_AlbedoBoost: 1
    m_EnvironmentLightingMode: 0
    m_EnableBakedLightmaps: 0
    m_EnableRealtimeLightmaps: 0
  m_LightmapEditorSettings:
    serializedVersion: 12
    m_Resolution: 2
    m_BakeResolution: 40
    m_AtlasSize: 1024
    m_AO: 0
    m_AOMaxDistance: 1
    m_CompAOExponent: 1
    m_CompAOExponentDirect: 0
    m_ExtractAmbientOcclusion: 0
    m_Padding: 2
    m_LightmapParameters: {fileID: 0}
    m_LightmapsBakeMode: 1
    m_TextureCompression: 1
    m_FinalGather: 0
    m_FinalGatherFiltering: 1
    m_FinalGatherRayCount: 256
    m_ReflectionCompression: 2
    m_MixedBakeMode: 2
    m_BakeBackend: 1
    m_PVRSampling: 1
    m_PVRDirectSampleCount: 32
    m_PVRSampleCount: 512
    m_PVRBounces: 2
    m_PVREnvironmentSampleCount: 256
    m_PVREnvironmentReferencePointCount: 2048
    m_PVRFilteringMode: 1
    m_PVRDenoiserTypeDirect: 1
    m_PVRDenoiserTypeIndirect: 1
    m_PVRDenoiserTypeAO: 1
    m_PVRFilterTypeDirect: 0
    m_PVRFilterTypeIndirect: 0
    m_PVRFilterTypeAO: 0
    m_PVREnvironmentMIS: 1
    m_PVRCulling: 1
    m_PVRFilteringGaussRadiusDirect: 1
    m_PVRFilteringGaussRadiusIndirect: 5
    m_PVRFilteringGaussRadiusAO: 2
    m_PVRFilteringAtrousPositionSigmaDirect: 0.5
    m_PVRFilteringAtrousPositionSigmaIndirect: 2
    m_PVRFilteringAtrousPositionSigmaAO: 1
    m_ExportTrainingData: 0
    m_TrainingDataDestination: TrainingData
    m_LightProbeSampleCountMultiplier: 4
  m_LightingDataAsset: {fileID: 0}
  m_LightingSettings: {fileID: 0}
--- !u!196 &4
NavMeshSettings:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_BuildSettings:
    serializedVersion: 2
    agentTypeID: 0
    agentRadius: 0.5
    agentHeight: 2
    agentSlope: 45
    agentClimb: 0.4
    ledgeDropHeight: 0
    maxJumpAcrossDistance: 0
    minRegionArea: 2
    manualCellSize: 0
    cellSize: 0.16666667
    manualTileSize: 0
    tileSize: 256
    accuratePlacement: 0
    maxJobWorkers: 0
    preserveTilesOutsideBounds: 0
    debug:
      m_Flags: 0
  m_NavMeshData: {fileID: 0}
"""

CAMERA = """--- !u!1 &100
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 101}
  - component: {fileID: 102}
  - component: {fileID: 103}
  - component: {fileID: 104}
  m_Layer: 0
  m_Name: Main Camera
  m_TagString: MainCamera
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &101
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: -10}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_RootOrder: 0
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!20 &102
Camera:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100}
  m_Enabled: 1
  serializedVersion: 2
  m_ClearFlags: 2
  m_BackGroundColor: {r: %(r)s, g: %(g)s, b: %(b)s, a: 1}
  m_projectionMatrixMode: 1
  m_GateFitMode: 2
  m_FOVAxisMode: 0
  m_SensorSize: {x: 36, y: 24}
  m_LensShift: {x: 0, y: 0}
  m_FocalLength: 50
  m_NormalizedViewPortRect:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1
    height: 1
  near clip plane: 0.3
  far clip plane: 1000
  field of view: 34
  orthographic: 1
  orthographic size: 6.75
  m_Depth: -1
  m_CullingMask:
    serializedVersion: 2
    m_Bits: 4294967295
  m_RenderingPath: -1
  m_TargetTexture: {fileID: 0}
  m_TargetDisplay: 0
  m_TargetEye: 0
  m_HDR: 1
  m_AllowMSAA: 0
  m_AllowDynamicResolution: 0
  m_ForceIntoRT: 0
  m_OcclusionCulling: 0
  m_StereoConvergence: 10
  m_StereoSeparation: 0.022
--- !u!81 &103
AudioListener:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100}
  m_Enabled: 1
--- !u!114 &104
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: %(guid_camera)s, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  m_RenderShadows: 1
  m_RequiresDepthTextureOption: 2
  m_RequiresOpaqueTextureOption: 2
  m_CameraType: 0
  m_Cameras: []
  m_RendererIndex: -1
  m_VolumeLayerMask:
    serializedVersion: 2
    m_Bits: 1
  m_VolumeTrigger: {fileID: 0}
  m_RenderPostProcessing: 0
  m_Antialiasing: 0
  m_AntialiasingQuality: 2
  m_StopNaN: 0
  m_Dithering: 0
  m_ClearDepth: 1
  m_AllowXRRendering: 1
  m_RequiresDepthTexture: 0
  m_RequiresColorTexture: 0
  m_Version: 2
"""

LUZ = """--- !u!1 &200
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 201}
  - component: {fileID: 202}
  m_Layer: 0
  m_Name: Global Light 2D
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &201
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 200}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_RootOrder: 1
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!114 &202
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 200}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: %(guid_luz)s, type: 3}
  m_Name:
  m_EditorClassIdentifier:
  m_ComponentVersion: 1
  m_LightType: 4
  m_BlendStyleIndex: 0
  m_FalloffIntensity: 0.5
  m_Color: {r: 1, g: 0.98, b: 0.94, a: 1}
  m_Intensity: 1
  m_LightVolumeIntensity: 1
  m_LightVolumeIntensityEnabled: 0
  m_ApplyToSortingLayers: 00000000
  m_LightCookieSprite: {fileID: 0}
  m_DeprecatedPointLightCookieSprite: {fileID: 0}
  m_LightOrder: 0
  m_OverlapOperation: 0
  m_NormalMapDistance: 3
  m_NormalMapQuality: 2
  m_UseNormalMap: 0
  m_ShadowIntensityEnabled: 0
  m_ShadowIntensity: 0.75
  m_ShadowVolumeIntensityEnabled: 0
  m_ShadowVolumeIntensity: 0.75
  m_Vertices:
  - position: {x: 0.9985302, y: 0.9985302, z: 0}
    color: {r: 0.70710677, g: 0.70710677, b: 0, a: 0}
    uv: {x: 0, y: 0}
  - position: {x: 0.9985302, y: 0.9985302, z: 0}
    color: {r: 0, g: 0, b: 0, a: 1}
    uv: {x: 0, y: 0}
  - position: {x: -0.9985302, y: 0.9985302, z: 0}
    color: {r: -0.70710677, g: 0.70710677, b: 0, a: 0}
    uv: {x: 0, y: 0}
  - position: {x: -0.9985302, y: 0.9985302, z: 0}
    color: {r: 0, g: 0, b: 0, a: 1}
    uv: {x: 0, y: 0}
  - position: {x: -0.99853003, y: -0.9985304, z: 0}
    color: {r: -0.70710665, g: -0.7071069, b: 0, a: 0}
    uv: {x: 0, y: 0}
  - position: {x: -0.99853003, y: -0.9985304, z: 0}
    color: {r: 0, g: 0, b: 0, a: 1}
    uv: {x: 0, y: 0}
  - position: {x: 0.99853003, y: -0.9985304, z: 0}
    color: {r: 0.70710665, g: -0.7071069, b: 0, a: 0}
    uv: {x: 0, y: 0}
  - position: {x: 0.99853003, y: -0.9985304, z: 0}
    color: {r: 0, g: 0, b: 0, a: 1}
    uv: {x: 0, y: 0}
  - position: {x: 0, y: 0, z: 0}
    color: {r: 0, g: 0, b: 0, a: 1}
    uv: {x: 0, y: 0}
  m_Triangles: 030001000800020000000100030002000100050003000800040002000300050004000300070005000800060004000500070006000500010007000800000006000700010000000700
  m_LocalBounds:
    m_Center: {x: 0, y: -0.00000011920929, z: 0}
    m_Extent: {x: 0.9985302, y: 0.99853027, z: 0}
  m_PointLightInnerAngle: 360
  m_PointLightOuterAngle: 360
  m_PointLightInnerRadius: 0.0
  m_PointLightOuterRadius: 1.0
  m_ShapeLightParametricSides: 5
  m_ShapeLightParametricAngleOffset: 0
  m_ShapeLightParametricRadius: 1
  m_ShapeLightFalloffSize: 0.5
  m_ShapeLightFalloffOffset: {x: 0, y: 0}
  m_ShapePath:
  - {x: -0.5, y: -0.5, z: 0}
  - {x: 0.5, y: -0.5, z: 0}
  - {x: 0.5, y: 0.5, z: 0}
  - {x: -0.5, y: 0.5, z: 0}
"""

PRINCIPAL = """--- !u!1 &300
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 301}
  - component: {fileID: 302}
  m_Layer: 0
  m_Name: %(nome_objeto)s
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &301
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 300}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_RootOrder: 2
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!114 &302
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 300}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: %(guid_script)s, type: 3}
  m_Name:
  m_EditorClassIdentifier:
%(campos)s"""


def gerar_cena(nome, script, nome_objeto, campos, cor):
    campos = list(campos) + [
        "materialAceso: {fileID: 2100000, guid: %s, type: 2}" % GUID_SPRITE_LIT,
        "materialBrilho: {fileID: 2100000, guid: %s, type: 2}" % GUID_SPRITE_UNLIT,
    ]
    texto = CABECALHO
    texto += CAMERA % {"r": cor[0], "g": cor[1], "b": cor[2], "guid_camera": GUID_CAMERA_URP}
    texto += LUZ % {"guid_luz": GUID_LUZ_2D}
    texto += PRINCIPAL % {
        "nome_objeto": nome_objeto,
        "guid_script": guid_do_meta(script),
        "campos": "".join("  " + c + "\n" for c in campos),
    }
    rel = "Assets/Scenes/%s.unity" % nome
    caminho = os.path.join(RAIZ, rel.replace("/", os.sep))
    os.makedirs(os.path.dirname(caminho), exist_ok=True)
    with io.open(caminho, "w", encoding="utf-8", newline="\n") as f:
        f.write(texto)
    meta = caminho + ".meta"
    if not os.path.exists(meta):
        with io.open(meta, "w", encoding="utf-8", newline="\n") as f:
            f.write("fileFormatVersion: 2\nguid: %s\nDefaultImporter:\n  externalObjects: {}\n  userData: \n"
                    "  assetBundleName: \n  assetBundleVariant: \n" % guid_de(rel))
    pasta_meta = os.path.join(RAIZ, "Assets", "Scenes.meta")
    if not os.path.exists(pasta_meta):
        with io.open(pasta_meta, "w", encoding="utf-8", newline="\n") as f:
            f.write("fileFormatVersion: 2\nguid: %s\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n"
                    "  userData: \n  assetBundleName: \n  assetBundleVariant: \n" % guid_de("Assets/Scenes"))
    print("  cena: " + rel)
    return rel, guid_do_meta(rel)


def main():
    cenas = [gerar_cena(*c) for c in CENAS]
    linhas = ["%YAML 1.1", "%TAG !u! tag:unity3d.com,2011:", "--- !u!1045 &1", "EditorBuildSettings:",
              "  m_ObjectHideFlags: 0", "  serializedVersion: 2", "  m_Scenes:"]
    for rel, guid in cenas:
        linhas += ["  - enabled: 1", "    path: " + rel, "    guid: " + guid]
    linhas += ["  m_configObjects:",
               "    com.unity.input.settings.actions: {fileID: -944628639613478452, guid: %s, type: 3}" % GUID_ACOES_INPUT]
    caminho = os.path.join(RAIZ, "ProjectSettings", "EditorBuildSettings.asset")
    with io.open(caminho, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(linhas) + "\n")
    print("  ProjectSettings/EditorBuildSettings.asset: %d cenas na Build" % len(cenas))


if __name__ == "__main__":
    main()
