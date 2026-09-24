Shader "Example"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _NoiseSpeed("Noise Speed", Float) = 1
        _VertexNoiseScale("Vertex Noise Scale", Float) = 0.1
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // インスタンシング用のpragma
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                // テクスチャ座標(UV)を頂点情報として受け取る
                float2 uv : TEXCOORD0;
                // インスタンスIDを頂点情報として受け取る
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                // 補間してfragmentへ渡すUV
                float2 uv : TEXCOORD0;
            };
            int Seed;
 
            float GetRandomNumber(float2 texCoord, float seed)
            {
                return frac(sin(dot(texCoord.xy, float2(12.9898, 78.233)) + seed) * 43758.5453);
            }
            
            // xyzそれぞれに別の乱数を割り当てた -1〜1 のベクトルを返す
            float3 GetRandomVector(float2 texCoord, float seed)
            {
                float3 r = float3(
                    GetRandomNumber(texCoord, seed),
                    GetRandomNumber(texCoord, seed + 17.13),
                    GetRandomNumber(texCoord, seed + 43.71));
                return r * 2.0 - 1.0;
            }

            float4 MakeNoiseColor(float2 texCoord, float seed)
            {
                float value = GetRandomNumber(texCoord, seed);
                return float4(value, value, value, 1);
            }

            half4 _Color;
            float _NoiseSpeed;
            float _VertexNoiseScale;

            v2f vert(appdata v)
            {
                v2f o;
                float seed = Seed + _Time.y * _NoiseSpeed;

                // インスタンスIDを元に位置やスケールを反映
                UNITY_SETUP_INSTANCE_ID(v);

                // UVをキーにした乱数で頂点をオブジェクト空間上でずらす
                float3 vertex = v.vertex.xyz;
                vertex += GetRandomVector(v.uv, seed) * _VertexNoiseScale;

                o.vertex = UnityObjectToClipPos(vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float mask = step(0.5, length(i.uv-float2(0.5,0.5))); 
                // _Time.y = シーン開始からの経過秒数
                float seed = Seed + _Time.y * _NoiseSpeed;
                return lerp(MakeNoiseColor(i.uv, seed), float4(0.0, 1.0, 1.0, 1.0), mask);
            }
            ENDCG
        }
    }
}
