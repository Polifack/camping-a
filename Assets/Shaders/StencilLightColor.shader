Shader "Custom/StencilLight Color"
{
    Properties
    {
        [HDR]_Color("Color",Color) = (1,1,1,1) 
    }
    HLSLINCLUDE
    
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"        
    struct appdata
    {
        float4 vertex : POSITION;
    };
    
    struct v2f
    {
        float4 vertex : SV_POSITION;
    };
    
    
    
    v2f vert(appdata v)
    {
        v2f o;
        o.vertex = TransformObjectToHClip(v.vertex.xyz);
        return o;
    }

    CBUFFER_START(UnityPerMaterial)
        float4 _Color;    
    CBUFFER_END
    
    float4 frag(v2f i) : SV_Target
    {
        return _Color * _Color.a;
    }
    ENDHLSL

    SubShader
    {
        Tags{"Queue" = "Transparent" "RenderType" = "Transparent"}  
        Pass
        {
            Tags
            {
                "RenderType" = "Transparent"          
                "RenderPipeline" = "UniversalPipeline"            
            }         
            
            ZWrite off
            ZTest GEqual
            
            Cull Back
            Blend DstColor One
            
            Stencil
            {
                comp equal
                Ref 1
                pass zero
                fail zero
                zfail zero
            }         
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag         
            ENDHLSL
        }      
        Pass
        {
            Tags
            {
                "RenderPipeline" = "UniversalPipeline"
                "LightMode" = "UniversalForward"
            }
            
            ZWrite off
            ZTest GEqual
            
            Cull Front
            Blend DstColor One

            Stencil
            {
                comp equal
                Ref 1
                pass zero
                fail zero
                zfail zero
            }         
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag         
            ENDHLSL
        }     
    }
}