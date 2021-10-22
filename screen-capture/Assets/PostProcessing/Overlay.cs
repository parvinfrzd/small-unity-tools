using System;

using UnityEngine;
using WC;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(OverlayRenderer), PostProcessEvent.AfterStack, "PostProcessing/Overlay")]
public sealed class Overlay : PostProcessEffectSettings {
}
public sealed class OverlayRenderer : PostProcessEffectRenderer<Overlay> {
    public override void Render(PostProcessRenderContext context) {
        var sheet = context.propertySheets.Get(Shader.Find("WC/PostProcessing/Overlay"));
        if (OverlayConfig.Instance != null) {
            sheet.properties.SetTexture("_LogoTex", OverlayConfig.Instance.texture);
        }
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}
