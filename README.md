# volume-project-2
Completed by YUAN Yunjie
    
Note：   
Due to a problem with unity's renderstexture itself, the saved 3d texture content may be emptied.     
(That is to say, the contents of LaplaceOperator may be emptied soon after the end of running TestVolume)     
A possible improvement is to convert the rendertexture to texture3D and then save it via Texture3D.         

     

Regarding the adjustment parameters (optimization will be attempted later).     
1. Activate VolumeStipping and adjust the parameters. (At the same time it is better to turn off TestVolume)     
2. When using TestVolume, it is better to turn off volumeStipping     
    
    
    
The display results can be viewed directly through the saved "assets",      
i.e.LaplaceTextureForDebug.asset,OriginalParametersStippleTexture.asset, StippleTextureForDebug.asset.      
    
VolumeStipping saves the result as StippingTextureForDebug    
TestVolume (filtered by Laplace) results in LaplaceTextureForDebug     
