# csStratwareDemo

Icarus mod workspace for [csStratware](../csStratware). Sample mod **`mods/processor-850`** sets every `RequiredMillijoules` in `D_ProcessorRecipes.json` to **850**.

## Prerequisites

- .NET 8 SDK
- [csStratware](../csStratware) built → `csmanager` on `PATH` (see main repo `build.cs`)
- Icarus (Steam) for `pak find @icarus` and `pak build-mod`
- Copy `csstratware.json.example` → `csstratware.json` and set your paths

```powershell
cd F:\Data\personal\c#\csStratware
dotnet run --project build.csproj -c Release
$env:PATH = "F:\Data\personal\c#\csStratware\dist\csmanager;" + $env:PATH
```

## Quick test

```powershell
cd F:\Data\personal\c#\csStratwareDemo
csmanager validate mods
csmanager list mods
csmanager pak find @icarus ProcessorRecipes --path-only --max 5
csmanager compile mods\processor-850
csmanager pak build-mod mods\processor-850
csmanager pak list mods\processor-850\dist\processor-850_P.pak
```

Output: `mods/processor-850/dist/processor-850_P.pak` (UnrealPak-packed, game-loadable `*_P.pak`).

Optional extract (no game pak read during prepare if `extracted/` exists):

```text
csmanager pak ue extract <data.pak> extracted --filter *D_ProcessorRecipes*
```

## Sample mod: `mods/processor-850`

| Path | Purpose |
|------|---------|
| `mod.json` | Manifest, `sourcePak`, `useUnrealPak` |
| `code/Processor850Patch.cs` | C# patch (active) |
| `patches/processor-recipes.json` | Same change via JSON (optional) |

### C# patch (default)

`mod.json`: `"codeProject": "code/Processor850.csproj"`, `"patchFiles": []`.

```csharp
[PatchAsset("D_ProcessorRecipes.json")]
public sealed class Processor850Patch : AssetPatch
{
    public override void Apply(JsonAssetEditor editor)
    {
        editor.ReplaceAll("RequiredMillijoules", 850);
    }
}
```

Flow:

1. **Source** — `data.pak` (`@icarus-data`), or `extracted/`, or `.cache/source/`
2. **Patch in memory** — `JsonAssetEditor` loads full JSON; `ReplaceAll` rewrites every matching property
3. **Write prepared** — `.cache/prepared/D_ProcessorRecipes.json` (full file)
4. **Pack** — **UnrealPak** → `dist/processor-850_P.pak`

Sdk ref in `code/Processor850.csproj`:

```xml
<ProjectReference Include="..\..\..\..\csStratware\src\CsStratware.Sdk\CsStratware.Sdk.csproj" />
```

### JSON patch (no C#)

`patches/processor-recipes.json`:

```json
{
  "patches": [
    {
      "assetPath": "D_ProcessorRecipes.json",
      "operations": [
        { "op": "replaceAll", "path": "/RequiredMillijoules", "value": 850 }
      ]
    }
  ]
}
```

In `mod.json`: set `"patchFiles": ["patches/processor-recipes.json"]`, remove `"codeProject"`.

## How the framework works

```
mod.json
   │
   ├─ patchFiles ──► ModLoader (JSON ops) ──┐
   ├─ code/*.csproj ► compile ─► Sdk patches ┼─► ModAssetPreparer ─► .cache/prepared/
   └─ contentRoots ──────────────────────────┘              │
                                                            ▼
                                              UnrealPak (game mods)  or  PakBuilder (tooling)
                                                            ▼
                                                      dist/*_P.pak
```

| Library | What it does |
|---------|----------------|
| **CsStratware.Core** | Models (`ModManifest`, pak settings), JSON contracts |
| **CsStratware.Sdk** | `AssetPatch`, `[PatchAsset]`, `JsonAssetEditor` (`Replace`, `ReplaceAll`, …) |
| **CsStratware.ModLoader** | Discover mods, JSON patches, compile/run C# DLLs, `JsonAssetPatcher` |
| **CsStratware.Pak** | Pak index/search, `ModAssetPreparer`, **UnrealPakRunner**, built-in `PakBuilder` |
| **CsStratware.Cli** | `csmanager` — `validate`, `compile`, `pak build-mod`, … |

More: [csStratware/src/README.md](../csStratware/src/README.md)

## Pak: UnrealPak vs built-in

| Use case | Packer |
|----------|--------|
| Icarus / UE4 `*_P.pak` override | **UnrealPak** (`useUnrealPak: true` or `sourcePak`) |
| Plain content / tooling | C# `PakBuilder` via `contentRoots` |

This demo uses UnrealPak. Success line: `Built mod pak (UnrealPak): ...`

Deploy: copy `processor-850_P.pak` to Icarus mod folder.
