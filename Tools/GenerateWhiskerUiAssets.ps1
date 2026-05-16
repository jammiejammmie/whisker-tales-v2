# One-shot generator: creates .meta files for the new Whisker UI .cs scripts,
# stamps minimal .unity scenes for Loading/NabiRoom/SleepMode, and updates
# EditorBuildSettings to include all 4 Whisker scenes.
#
# Idempotent: skips .meta files that already exist and preserves their GUIDs.

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot

function New-Guid32 { return [guid]::NewGuid().ToString("N") }
function Get-EpochSeconds { return [int][Math]::Floor(([DateTime]::UtcNow - [DateTime]'1970-01-01').TotalSeconds) }

function Write-CsMeta {
    param([string]$csPath)
    $metaPath = "$csPath.meta"
    if (Test-Path -LiteralPath $metaPath) { return @{ guid = (Get-MetaGuid $metaPath); created = $false } }
    $guid = New-Guid32
    $now = Get-EpochSeconds
    $body = "fileFormatVersion: 2`nguid: $guid`nMonoImporter:`n  externalObjects: {}`n  serializedVersion: 2`n  defaultReferences: []`n  executionOrder: 0`n  icon: {instanceID: 0}`n  userData: `n  assetBundleName: `n  assetBundleVariant: `n"
    [System.IO.File]::WriteAllText($metaPath, $body)
    return @{ guid = $guid; created = $true }
}

function Write-TtfMeta {
    param([string]$ttfPath)
    $metaPath = "$ttfPath.meta"
    if (Test-Path -LiteralPath $metaPath) { return @{ guid = (Get-MetaGuid $metaPath); created = $false } }
    $guid = New-Guid32
    $body = "fileFormatVersion: 2`nguid: $guid`nTrueTypeFontImporter:`n  externalObjects: {}`n  serializedVersion: 4`n  fontSize: 16`n  forceTextureCase: -2`n  characterSpacing: 0`n  characterPadding: 1`n  includeFontData: 1`n  fontNames:`n  - NanumMyeongjo`n  fallbackFontReferences: []`n  customCharacters: `n  fontRenderingMode: 0`n  ascentCalculationMode: 1`n  useLegacyBoundsCalculation: 0`n  shouldRoundAdvanceValue: 1`n  userData: `n  assetBundleName: `n  assetBundleVariant: `n"
    [System.IO.File]::WriteAllText($metaPath, $body)
    return @{ guid = $guid; created = $true }
}

function Get-MetaGuid {
    param([string]$metaPath)
    $line = (Get-Content -LiteralPath $metaPath | Where-Object { $_ -match '^guid:' } | Select-Object -First 1)
    if ($line) { return ($line -replace '^guid:\s*', '').Trim() }
    return $null
}

function Write-SceneMeta {
    param([string]$scenePath)
    $metaPath = "$scenePath.meta"
    if (Test-Path -LiteralPath $metaPath) { return @{ guid = (Get-MetaGuid $metaPath); created = $false } }
    $guid = New-Guid32
    $body = "fileFormatVersion: 2`nguid: $guid`nDefaultImporter:`n  externalObjects: {}`n  userData: `n  assetBundleName: `n  assetBundleVariant: `n"
    [System.IO.File]::WriteAllText($metaPath, $body)
    return @{ guid = $guid; created = $true }
}

function Write-MinimalScene {
    param([string]$scenePath, [string]$cameraName)
    if (Test-Path -LiteralPath $scenePath) { return $false }
    $yaml = @"
%YAML 1.1
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
    m_BakeBackend: 0
    m_PVRSampling: 1
    m_PVRDirectSampleCount: 32
    m_PVRSampleCount: 500
    m_PVRBounces: 2
    m_PVREnvironmentSampleCount: 500
    m_PVREnvironmentReferencePointCount: 2048
    m_PVRFilteringMode: 2
    m_PVRDenoiserTypeDirect: 0
    m_PVRDenoiserTypeIndirect: 0
    m_PVRDenoiserTypeAO: 0
    m_PVRFilterTypeDirect: 0
    m_PVRFilterTypeIndirect: 0
    m_PVRFilterTypeAO: 0
    m_PVREnvironmentMIS: 0
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
--- !u!1 &519420028
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 519420032}
  - component: {fileID: 519420031}
  - component: {fileID: 519420029}
  m_Layer: 0
  m_Name: $cameraName
  m_TagString: MainCamera
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!81 &519420029
AudioListener:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 519420028}
  m_Enabled: 1
--- !u!20 &519420031
Camera:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 519420028}
  m_Enabled: 1
  serializedVersion: 2
  m_ClearFlags: 2
  m_BackGroundColor: {r: 0.08, g: 0.06, b: 0.05, a: 1}
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
  field of view: 60
  orthographic: 1
  orthographic size: 5
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
--- !u!4 &519420032
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 519420028}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: -10}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_Children: []
  m_Father: {fileID: 0}
  m_RootOrder: 0
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
"@
    [System.IO.File]::WriteAllText($scenePath, $yaml)
    return $true
}

# 1. .meta for the new .cs files
$uiDir = Join-Path $RepoRoot "Assets\WhiskerTales\UI"
$csFiles = @(
    "WhiskerTheme.cs", "WhiskerButton.cs", "WhiskerScreens.cs",
    "WhiskerPuzzleHud.cs", "WhiskerLoadingScreen.cs",
    "WhiskerNabiRoomScreen.cs", "WhiskerSleepModeScreen.cs"
)
foreach ($cs in $csFiles) {
    $path = Join-Path $uiDir $cs
    $r = Write-CsMeta -csPath $path
    Write-Host "[cs.meta] $cs  guid=$($r.guid)  created=$($r.created)"
}

# 2. .meta for the copied font
$fontPath = Join-Path $RepoRoot "Assets\WhiskerTales\UI\Resources\NanumMyeongjo-Regular.ttf"
$rf = Write-TtfMeta -ttfPath $fontPath
Write-Host "[ttf.meta] Resources/NanumMyeongjo guid=$($rf.guid) created=$($rf.created)"

# 3. .meta for the Resources folder
$resDir = Join-Path $RepoRoot "Assets\WhiskerTales\UI\Resources"
$resMeta = "$resDir.meta"
if (-not (Test-Path -LiteralPath $resMeta)) {
    $g = New-Guid32
    [System.IO.File]::WriteAllText($resMeta, "fileFormatVersion: 2`nguid: $g`nfolderAsset: yes`nDefaultImporter:`n  externalObjects: {}`n  userData: `n  assetBundleName: `n  assetBundleVariant: `n")
    Write-Host "[folder.meta] Resources guid=$g"
}

# Folder .meta for Assets/WhiskerTales/UI itself
$uiMeta = "$uiDir.meta"
if (-not (Test-Path -LiteralPath $uiMeta)) {
    $g = New-Guid32
    [System.IO.File]::WriteAllText($uiMeta, "fileFormatVersion: 2`nguid: $g`nfolderAsset: yes`nDefaultImporter:`n  externalObjects: {}`n  userData: `n  assetBundleName: `n  assetBundleVariant: `n")
    Write-Host "[folder.meta] UI guid=$g"
}

# 4. Three minimal scene files + their meta
$sceneDir = Join-Path $RepoRoot "Assets\WhiskerTales\UI\Scenes"
if (-not (Test-Path -LiteralPath $sceneDir)) {
    New-Item -ItemType Directory -Path $sceneDir | Out-Null
    $g = New-Guid32
    [System.IO.File]::WriteAllText("$sceneDir.meta", "fileFormatVersion: 2`nguid: $g`nfolderAsset: yes`nDefaultImporter:`n  externalObjects: {}`n  userData: `n  assetBundleName: `n  assetBundleVariant: `n")
    Write-Host "[folder.meta] Scenes guid=$g"
}

$scenes = @(
    @{ name = "WhiskerLoadingScene";   camera = "LoadingCamera" },
    @{ name = "WhiskerNabiRoomScene";  camera = "NabiRoomCamera" },
    @{ name = "WhiskerSleepModeScene"; camera = "SleepModeCamera" }
)
$sceneGuids = @{}
foreach ($s in $scenes) {
    $scenePath = Join-Path $sceneDir "$($s.name).unity"
    $created = Write-MinimalScene -scenePath $scenePath -cameraName $s.camera
    $meta = Write-SceneMeta -scenePath $scenePath
    $sceneGuids[$s.name] = $meta.guid
    Write-Host "[scene] $($s.name)  scene_created=$created  guid=$($meta.guid)"
}

# 5. Update EditorBuildSettings.asset
$buildSettingsPath = Join-Path $RepoRoot "ProjectSettings\EditorBuildSettings.asset"

# Discover existing scene guids we need to include
$whiskerGameSceneMeta = Join-Path $RepoRoot "Assets\WhiskerTales\Puzzle\Scenes\WhiskerGameScene.unity.meta"
$whiskerGameGuid = if (Test-Path -LiteralPath $whiskerGameSceneMeta) { Get-MetaGuid $whiskerGameSceneMeta } else { $null }

$scenesYaml = ""
if ($whiskerGameGuid) {
    $scenesYaml += "  - enabled: 1`n    path: Assets/WhiskerTales/Puzzle/Scenes/WhiskerGameScene.unity`n    guid: $whiskerGameGuid`n"
}
foreach ($s in $scenes) {
    $g = $sceneGuids[$s.name]
    $scenesYaml += "  - enabled: 1`n    path: Assets/WhiskerTales/UI/Scenes/$($s.name).unity`n    guid: $g`n"
}

$newBuildSettings = "%YAML 1.1`n%TAG !u! tag:unity3d.com,2011:`n--- !u!1045 &1`nEditorBuildSettings:`n  m_ObjectHideFlags: 0`n  serializedVersion: 2`n  m_Scenes:`n$scenesYaml  m_configObjects: {}`n"
[System.IO.File]::WriteAllText($buildSettingsPath, $newBuildSettings)
Write-Host "[build] EditorBuildSettings updated with $($scenes.Count + ($(if($whiskerGameGuid){1}else{0}))) scene(s)"

Write-Host ""
Write-Host "Done."
