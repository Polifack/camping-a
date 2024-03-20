Shader "Custom/StencilLight Mask"
{
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        LOD 0
        
        Pass
        {
            
            Ztest Greater
            Zwrite off
            Cull Off
            Colormask 0
            Stencil
            {
                Comp Always
                Ref 0
                Pass Replace
            }
        }
        
        
    }
}