using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StandardShaderUtils {

	public enum BlendMode {
		Opaque,
		Cutout,
		Fade,
		Transparent
	}

    public static Material ChangeRenderMode(Material standardShaderMaterial, BlendMode blendMode = BlendMode.Opaque, float metalness = 0.0f, float smoothness = 0.0f, bool highlights = false, bool reflections = false)
    {
        switch (blendMode) {
            case BlendMode.Opaque:
                standardShaderMaterial.SetFloat("_Mode", 0);
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                standardShaderMaterial.SetInt("_ZWrite", 1);
                standardShaderMaterial.EnableKeyword("_METALLICGLOSSMAP");
                standardShaderMaterial.SetFloat("_Glossiness", smoothness);
                standardShaderMaterial.SetFloat("_Metallic", metalness);
                standardShaderMaterial.EnableKeyword("_NORMALMAP");
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.DisableKeyword("_EMISSION");
                standardShaderMaterial.renderQueue = -1;
                break;
            case BlendMode.Cutout:
                standardShaderMaterial.SetFloat("_Mode", 1);
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                standardShaderMaterial.SetInt("_ZWrite", 1);
                standardShaderMaterial.EnableKeyword("_METALLICGLOSSMAP");
                standardShaderMaterial.SetFloat("_Glossiness", smoothness);
                standardShaderMaterial.SetFloat("_Metallic", metalness);
                standardShaderMaterial.EnableKeyword("_NORMALMAP");
                standardShaderMaterial.EnableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.DisableKeyword("_EMISSION");
                standardShaderMaterial.renderQueue = 2450;
                break;
            case BlendMode.Fade:
                standardShaderMaterial.SetFloat("_Mode", 2);
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                standardShaderMaterial.SetInt("_ZWrite", 0);
                standardShaderMaterial.EnableKeyword("_METALLICGLOSSMAP");
                standardShaderMaterial.SetFloat("_Glossiness", smoothness);
                standardShaderMaterial.SetFloat("_Metallic", metalness);
                standardShaderMaterial.EnableKeyword("_NORMALMAP");
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.EnableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.DisableKeyword("_EMISSION");
                standardShaderMaterial.renderQueue = 3000;
                break;
            case BlendMode.Transparent:
                standardShaderMaterial.SetFloat("_Mode", 3);
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                standardShaderMaterial.SetInt("_ZWrite", 0);
                standardShaderMaterial.EnableKeyword("_METALLICGLOSSMAP");
                standardShaderMaterial.SetFloat("_Glossiness", smoothness);
                standardShaderMaterial.SetFloat("_Metallic", metalness);
                standardShaderMaterial.EnableKeyword("_NORMALMAP");
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.DisableKeyword("_EMISSION");
                standardShaderMaterial.renderQueue = 3000;
                break;
        }
        if (highlights)
        {
            standardShaderMaterial.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
            standardShaderMaterial.SetFloat("_SpecularHighlights", 1f);
        }
        else
        {
            standardShaderMaterial.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            standardShaderMaterial.SetFloat("_SpecularHighlights", 0f);
        }
        if (reflections)
        {
            standardShaderMaterial.DisableKeyword("_GLOSSYREFLECTIONS_OFF");
            standardShaderMaterial.SetFloat("_GlossyReflections", 1f);
        }
        else
        {
            standardShaderMaterial.EnableKeyword("_GLOSSYREFLECTIONS_OFF");
            standardShaderMaterial.SetFloat("_GlossyReflections", 0f);
        }
        return standardShaderMaterial;
    }
}
