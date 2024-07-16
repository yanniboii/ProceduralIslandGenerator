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
    }

    // The SubShader block containing the Shader code. 
    SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            // The HLSL code block. Unity SRP uses the HLSL language.
            HLSLPROGRAM
            // This line defines the name of the vertex shader. 
            #pragma vertex vert
            // This line defines the name of the fragment shader. 
            #pragma fragment frag

            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            

            // The structure definition defines which variables it contains.
            // This example uses the Attributes structure as an input structure in
            // the vertex shader.
            struct Attributes
            {
                // The positionOS variable contains the vertex positions in object
                // space.
                float4 positionOS   : POSITION;   
                float2 uv : TEXCOORD0;              
            };

            struct Varyings
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS  : SV_POSITION;
                float2 uv : TEXCOORD0;              
                float3 worldPos : TEXCOORD1;
            };            


            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                half _Glossiness;
                half _Metallic;
                sampler2D _MainTex;
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

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 baseColor = tex2D(_MainTex, input.uv) * _BaseColor;

                // Use the world position in some way, for example to modify the color
                if (input.worldPos.y > 1)
                {
                    baseColor.rgb = baseColor = tex2D(_MainTex, input.uv) * float4(0, 1, 0,0); // Set to green color
                }
                if (input.worldPos.y > 8)
                {
                    baseColor.rgb = float3(0.2, 0.2, 0.2); // Set to green color
                }                
                if (input.worldPos.y > 14)
                {
                    baseColor.rgb = float3(1, 1, 1); // Set to green color
                }
                return baseColor;
            }
            ENDHLSL
        }
    }
}