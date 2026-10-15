Shader "Custom/InstancedSpheres_Lit"
{
    Properties
    {
        _BaseColorA("Color A", Color) = (0.9, 0.35, 0.2, 1)
        _BaseColorB("Color B", Color) = (0.9, 0.35, 0.2, 1)
        _Radius("Radius", Float) = 0.1
       [IntRange] _ColorSteps("Color Steps", Range(1, 20)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma target 4.5                 
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            StructuredBuffer<float3> _Positions;
            float  _Radius;
            half4  _BaseColorA;
            half4  _BaseColorB;
            float    _ColorSteps;

            struct Attributes
            {
                float3 positionOS : POSITION;
                uint   instanceID : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 color      : TEXCOORD2;
            };

            Varyings vert(Attributes i)
            {
                float3 worldPos = i.positionOS * _Radius + _Positions[i.instanceID];

                Varyings o;
                o.positionWS = worldPos;
                o.positionCS = TransformWorldToHClip(worldPos);
                o.normalWS   = i.positionOS; //object space position is also the normal for a sphere, with no rotation object space direction is worldSpace direction
                o.color = lerp(_BaseColorA.rgb, _BaseColorB.rgb, frac( i.instanceID / _ColorSteps));
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 normalWS = normalize(i.normalWS);

                float4 shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                Light mainLight = GetMainLight(shadowCoord);

                //lambert 
                half3 diffuse = mainLight.color  * (saturate(dot(normalWS, mainLight.direction)) * mainLight.shadowAttenuation); 
                half3 ambient = SampleSH(normalWS);
                return half4(i.color * (diffuse + ambient), 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vertShadow
            #pragma fragment fragShadow
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            StructuredBuffer<float3> _Positions;
            float  _Radius;
            float3 _LightDirection;   
            float3 _LightPosition;   

            struct Attributes
            {
                float3 positionOS : POSITION;
                uint   instanceID : SV_InstanceID;
            };

            float4 vertShadow(Attributes i) : SV_POSITION
            {
                float3 worldPos  = i.positionOS * _Radius + _Positions[i.instanceID];
                float3 normalWS  = normalize(i.positionOS); 

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDir = normalize(_LightPosition - worldPos);
                #else
                    float3 lightDir = _LightDirection;
                #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(worldPos, normalWS, lightDir));


                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE * positionCS.w);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE * positionCS.w);
                #endif
                return positionCS;
            }

            half4 fragShadow() : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
