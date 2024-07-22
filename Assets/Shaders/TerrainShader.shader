// This shader fills the mesh shape with a color predefined in the code.
Shader "Custom/TerrainShader"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    { 
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _MainTex("Base Map", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0, 1)) = 0.5
        _Metallic("Metallic", Range(0, 1)) = 0.0
        _Roughness("Roughness", Range(0, 1)) = 0.5
        _Perlin("Perlin", 2D) = "white" {}
        regionTextures("textureArray", 2DArray) = "white" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}
    }

    // The SubShader block containing the Shader code. 
    SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            LOD 200
            // The HLSL code block. Unity SRP uses the HLSL language.
            HLSLPROGRAM
            // This line defines the name of the vertex shader. 
            #pragma vertex vert
            // This line defines the name of the fragment shader. 
            #pragma fragment frag

            #pragma shader_feature_local_fragment _OCCLUSIONMAP
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

            #pragma require 2darray
            
            

            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
            // The structure definition defines which variables it contains.
            // This example uses the Attributes structure as an input structure in
            // the vertex shader.
            struct Attributes
            {
                // The positionOS variable contains the vertex positions in object
                // space.
                float4 positionOS   : POSITION;   
                float2 uv : TEXCOORD0;      
                float3 normalOS : NORMAL;
        
            };

            struct Varyings
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS  : SV_POSITION;
                float2 uv : TEXCOORD0;              
                float3 worldPos : TEXCOORD1;
                half3 lightAmount : TEXCOORD2;
                float3 normalWS : TEXCOORD3;
                float4 shadowCoords : TEXCOORD4;
            };

            const static int maxRegionsCount = 16;


            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                half _Glossiness;
                half _Metallic;
                half _Roughness;
                sampler2D _MainTex;
                sampler2D _Perlin;

                int regionsCount;
                float4 regions[maxRegionsCount];
                float regionHeights[maxRegionsCount];
            CBUFFER_END
            // The vertex shader definition with properties defined in the Varyings 
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);

                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformWorldToHClip(worldPos);
                output.uv = input.uv;
                output.worldPos = worldPos;

                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);

                float4 shadowCoordinates = GetShadowCoord(positions);
                output.shadowCoords = shadowCoordinates;

                output.normalWS = TransformObjectToWorldNormal(input.normalOS);    

                Light light = GetMainLight();

                output.lightAmount = LightingSpecular(light.color, light.direction, output.normalWS.xyz, normalize(_WorldSpaceCameraPos-worldPos), 1024, _Glossiness);

                return output;
            }



            half4 frag(Varyings input) : SV_Target
            {

                half shadowAmount = MainLightRealtimeShadow(input.shadowCoords);
                half4 baseColor = tex2D(_MainTex, input.uv) * _BaseColor;

                Light light = GetMainLight();

                float diff = max(dot(normalize(light.direction), input.normalWS.xyz), 0.0); // diffuse term intesity

                float3 diffuse = 8* diff * light.color * baseColor.rgb;

                float3 reflectDir = reflect(-normalize(light.direction),input.normalWS.xyz);
                float roughenssFactor = 1 - _Roughness;
                float spec = pow(max(dot(normalize(_WorldSpaceCameraPos-input.worldPos),reflectDir),0.0),32*roughenssFactor); // specular term dot is the cosine of the angle specular intesity is the shininess shininess changes the frequency of the cosine
                spec *= _Glossiness;


                for(int i = 0; i <= regionsCount; i++){  
                    if(input.worldPos.y > (int)regionHeights[i]){
                        baseColor.rgb =  regions[i].rgb;
                    }
                }


                float3 specular =  spec * baseColor.rgb * light.color;


                float3 finalColor = baseColor.rgb +diffuse * baseColor.rgb + specular;

                finalColor = saturate(finalColor);

                return float4(finalColor.rgb,1);
                //return float4(baseColor.rgb,1);
                //return float4(heightPercent.xxx,0);
            }
            ENDHLSL
        }
    }
}