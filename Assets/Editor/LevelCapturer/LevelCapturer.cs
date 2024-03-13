
using UnityEngine;


public class LevelCapturer
{
    public Camera Cam { get; set; }

    public Texture2D CaptureCameraToTexture(LayerMask cullMask)
    {
        if (Cam == null)
        {
            Debug.LogError("Camera reference is null!");
            return null;
        }
        LayerMask tempCullMask = Cam.cullingMask;

        Cam.targetTexture = new RenderTexture(Cam.pixelWidth, Cam.pixelHeight, 24);
        RenderTexture.active = Cam.targetTexture;
        Cam.cullingMask = cullMask;
        Cam.clearFlags = CameraClearFlags.Nothing;
        Cam.Render();

        Texture2D image = new Texture2D(Cam.targetTexture.width, Cam.targetTexture.height, TextureFormat.RGBAFloat, false);
        image.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);
        image.Apply();
        Cam.cullingMask = tempCullMask;
        Cam.targetTexture = null;

        return image;

   

    }

    public Texture2D MergeTextures(Texture2D tex1, Texture2D tex2)
    {
        if (tex1.width != tex2.width || tex1.height != tex2.height)
        {
            Debug.LogError("Textures are not of the same size.");
            return null;
        }
        else if (tex2 == null) 
        {
            tex2 = Texture2D.whiteTexture;
            tex2.width = tex1.width; tex2.height = tex1.height;
        }

        Texture2D mergedTexture = new Texture2D(tex1.width, tex1.height, TextureFormat.RGBAFloat, false);

        for (int x = 0; x < tex1.width; x++)
        {
            for (int y = 0; y < tex1.height; y++)
            {
                Color pixel1 = tex1.GetPixel(x, y);
                Color pixel2 = tex2.GetPixel(x, y);

                Color mergedPixel = new Color(pixel1.r , pixel1.g , pixel1.b , pixel2.r);

                mergedTexture.SetPixel(x, y, mergedPixel);
            }
        }

        mergedTexture.Apply();
        return mergedTexture;
    }



  
}
