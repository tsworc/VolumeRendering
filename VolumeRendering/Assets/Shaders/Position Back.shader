Shader "Unlit/PositionBack"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Scale("scale", Vector) = (1,1,1,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
		Cull front
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
				float3 pos:TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _Scale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				//o.pos = v.vertex/2;//.rgb+.5;
				float4 vertex = v.vertex;// float4(v.vertex,1);
				float4 world = mul(unity_ObjectToWorld, vertex);//.rgb+.5;
				//o.pos = mul(_Scale, vertex).xyz;
				o.pos = vertex.xyz * _Scale.xyz;
				//o.pos = world.xyz;
				o.pos = o.pos+.5;
				//o.pos = o.pos/2+.5;
				//o.pos = mul(v.vertex, unity_ObjectToWorld);//.rgb+.5;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				return float4(i.pos,1);
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
