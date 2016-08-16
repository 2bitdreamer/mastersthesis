Shader "Custom/Vertex Colored" {
	Properties{
	}
	SubShader{
		Blend SrcAlpha OneMinusSrcAlpha
		Pass{
		ColorMaterial AmbientAndDiffuse
	}
	}
}