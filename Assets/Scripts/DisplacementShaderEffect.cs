using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class DisplacementShaderEffect : MonoBehaviour {

    public Material DisplacementMaterial;

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        Graphics.Blit(src, dst, DisplacementMaterial);
    }
}
