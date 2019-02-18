Shader "Sprites/NonWhiteDefaultColor" {
Properties {
    _Color ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	[PerRendererData] _MainTex ("Particle Texture", 2D) = "white" {}
}

Category {
	Tags {"IgnoreProjector"="True" "RenderType"="Transparent"  "Queue"="Transparent" }
	Name "MainPass"
	Blend SrcAlpha OneMinusSrcAlpha
	AlphaTest Greater .01
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}
	
	// ---- Fragment program cards
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma fragmentoption ARB_fog_exp2

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _Color;
			
			struct appdata_t {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				//return i.color * (1 - (1 - i.color) * (1 - _Color)) * tex2D(_MainTex, i.texcoord);
				half4 c = tex2D(_MainTex, i.texcoord).rgba; 			//Here is the texture
				if (any(c.rgb != half3(1,1,1)))                               //if the texture color is different from white
					c.rgba *= i.color.rgba;                                   //then color it by using the _Color property
				return c;   
			}
			ENDCG 
		}
	} 	
}
}