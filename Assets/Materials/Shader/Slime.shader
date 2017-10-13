Shader "Custom/Slime"
{
	Properties
	{
		_Color("Main Color", Color) = (0,0.1,0.7,0.5)
	}
	
	SubShader
	{
		Tags{ "RenderType" = "Transparent" }
		LOD 100
		Cull Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float3 normal : NORMAL0;
				float4 position : POSITION;
				float4 color : COLOR0;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float4 color : COLOR0;
			};

			uniform float4 _Color;

			v2f vert(appdata v)
			{
				v2f o;
				float4 worldPos = mul(unity_ObjectToWorld, float4(v.position.xyz, 1.0));

				o.position = UnityWorldToClipPos(worldPos);
				o.color = _Color;
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = float4(i.color.rgb, 0);
				return col;
			}
			ENDCG
		}
	}
}
