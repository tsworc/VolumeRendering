Shader "Unlit/VolumeRender2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        FrontS ("front", 2D) = "gray" {}
        BackS ("back", 2D) = "white" {}
        VolumeS ("volume", 3D) = "white" {}
		_offset ("offset", Vector) = (0,0,0,0)
		boundsMax ("Max Bounds", Vector) = (1,1,1,0)
		boundsMin ("Min Bounds", Vector) = (0,0,0,0)
    }
    SubShader
    {
        //Tags { "RenderType"="Opaque" }
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
		Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				float4 pos:TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D BackS;
            sampler2D FrontS;
			sampler3D VolumeS;
            float4 _MainTex_ST;
			int Iterations = 1;
			float StepSize = 0.1f;
			float4 _offset;
			float4 boundsMax;
			float4 boundsMin;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.pos = o.vertex;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            /*fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }*/
			fixed4 frag(v2f input) : SV_Target {
				//calculate projective texture coordinates
				//used to project the front and back position textures onto the cube
				float2 texC = input.pos.xy /= input.pos.w;
				texC.x =  0.5f*texC.x + 0.5f;
				texC.y = -0.5f*texC.y + 0.5f;
				//return float4(texC,0,1);
 
				float3 front = tex2D(FrontS, texC).xyz;
				float3 back = tex2D(BackS, texC).xyz;
 
				float3 dir = normalize(back - front);
				//float3 dir = (back - front);

				//return float4(front,1);
				//return float4(back,1);
				//return float4(dir,1);
				//return float4(back - front,1);

				float4 pos = float4(front, 0);
				//return tex3Dlod(VolumeS, pos);
 
				float4 dst = float4(0, 0, 0, 0);
				float4 src = 0;
 
				float value = 0;
 
				//float3 Step = dir * StepSize;
				//float3 Step = dir * 0.0283;
				float3 Step = dir * 0.00283;

				//return float4(1,0,0,0.5);
 
				for(int i = 0; i < 500; i++)
				//for(int i = 0; i < 50; i++)
				{
					//return float4(1,0,0,1);
					pos.w = 0;
					//value = tex3Dlod(VolumeS, pos).r;
					//return value;
					value = tex3Dlod(VolumeS, pos).r;
					//return tex3Dlod(VolumeS, float4(front,0)).r;
             
					src = (float4)value;
					if(pos.x > boundsMax.x || pos.y > boundsMax.y ||  pos.z > boundsMax.z ||
						pos.x < boundsMin.x || pos.y < boundsMin.y || pos.z < boundsMin.z)
						src = 0;
					//src.a = .5f; //reduce the alpha to have a more transparent result 
					src.a *= .5f; //reduce the alpha to have a more transparent result 
         
					//Front to back blending
					// dst.rgb = dst.rgb + (1 - dst.a) * src.a * src.rgb
					// dst.a   = dst.a   + (1 - dst.a) * src.a     
					src.rgb *= src.a;
					dst = (1.0f - dst.a)*src + dst;     
     
					//break from the loop when alpha gets high enough
					if(dst.a >= .95f)
						break; 
     
					//advance the current position
					pos.xyz += Step;
     
					//break if the position is greater than <1, 1, 1>
					if(pos.x > 1 || pos.y > 1 ||  pos.z > 1 || 
						pos.x < 0 || pos.y < 0 || pos.z < 0)
						break;
				}
 
				//return value;
					
				return dst;
			}
            ENDCG
        }
    }
}
