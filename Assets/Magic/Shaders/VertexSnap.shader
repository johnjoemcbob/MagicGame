Shader "Custom/VertexSnap"
{
	Properties
	{
		_Colour( "Colour", Color ) = ( 1, 1, 1, 1 )
		_MainTex( "Texture (RGB)", 2D ) = "white" {}
		_SnapAmount( "Snap Amount", float ) = 1
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

            uniform float4 _Colour;
            uniform sampler2D _MainTex;
            uniform float _SnapAmount;

			struct vertexinput
			{
				float4 vertex : POSITION;
				float4 texcoord0 : TEXCOORD0;
			};

			struct fragmentinput
			{
				float4 position : SV_POSITION;
				float4 texcoord0 : TEXCOORD0;
			};

			fragmentinput vert( vertexinput v )
			{
				fragmentinput o;
				{
					// Snap vertex position
					float4 snap = float4( 0, 0, 0, 0 );
					{
						snap.x = floor( ( v.vertex.x / _SnapAmount ) + 0.5f ) * _SnapAmount;
						snap.y = floor( ( v.vertex.y / _SnapAmount ) + 0.5f ) * _SnapAmount;
						snap.z = floor( ( v.vertex.z / _SnapAmount ) + 0.5f ) * _SnapAmount;
					}
					o.position = mul( UNITY_MATRIX_MVP, v.vertex - snap );
					o.texcoord0 = v.texcoord0;
				}
				return o;
			}

			fixed4 frag( fragmentinput i ) : SV_Target
			{
				return tex2D( _MainTex, i.texcoord0 ) * _Colour;
			}

			ENDCG
		}
	} 
	FallBack "Diffuse"
}