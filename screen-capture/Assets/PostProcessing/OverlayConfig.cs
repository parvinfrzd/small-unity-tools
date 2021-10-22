using System.Collections;
using System.Collections.Generic;

using UnityEngine;
[ExecuteInEditMode]
public class OverlayConfig : Singleton<OverlayConfig> {
    public Texture2D texture;

    public void SetTexture(Texture2D value) {
        texture = value;
    }
}
