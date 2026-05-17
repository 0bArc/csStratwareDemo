using CsStratware.Sdk;

[PatchAsset("D_ProcessorRecipes.json")]
public sealed class Processor850Patch : AssetPatch
{
    public override void Apply(JsonAssetEditor editor) =>
        editor.ReplaceAll("RequiredMillijoules", 850);
}
