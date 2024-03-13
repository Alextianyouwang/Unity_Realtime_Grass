
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using System.Linq;
using UnityEngine.Experimental.Rendering.Universal;
using System.IO;

public class LevelCapturer_Editor : EditorWindow
{
    private LevelCapturer _capturer;
    private string _path;
    private string _name;
    private LayerMask _mask;
    private LayerMask _depthMask;

    private Texture2D _colSceneTex;
    private Texture2D _depthSceneTex;
    private bool _renderDepth,_prev_renderDepth;

    private UniversalAdditionalCameraData _urp_cam;

    private UniversalRendererData _rendererData;
    private ScriptableRendererFeature _feature;
    private string _featureName = "Render Depth";
    private string _savePath= "CaptureData";
    [MenuItem("Tool/Level Capturer")]
    public static void ShowWindow()
    {
        LevelCapturer_Editor window = GetWindow<LevelCapturer_Editor>("Level Capturer");
        window.minSize = new Vector2(600, 800);
        window.Show();
    }
    private void OnEnable()
    {
        _capturer = new LevelCapturer();
        _capturer.Cam = FindObjectOfType<Camera>();
        _urp_cam = _capturer.Cam.GetComponent<UniversalAdditionalCameraData>();
        _rendererData = EditorUtil. GetObject<UniversalRendererData>("URPAsset_Renderer.asset");
        
    }
    private void OnDisable() 
    {
    
    }
 
   
    private bool TryGetFeature(out ScriptableRendererFeature feature) 
    {
        if (_rendererData == null) 
        {
            Debug.LogWarning("No Renderer Data Detected, check if 'URPAsset_Renderer' exists");
            feature = null;
            return false;
        }
          
        feature = _rendererData.rendererFeatures.Where(f => f.name == _featureName).FirstOrDefault();
        return feature != null;
    }
    public void SaveTexture(Texture2D image, string path, string name)
    {
        byte[] bytes = image.EncodeToPNG();
        File.WriteAllBytes(Path.Combine(Application.dataPath, path, name + ".png"), bytes);
        Debug.Log($"Saved camera capture to: {path}");
        AssetDatabase.Refresh();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Scene Capture", EditorStyles.boldLabel);
       
        _capturer.Cam = (Camera)EditorGUILayout.ObjectField("Camera", _capturer.Cam , typeof(Camera), true);
        if (!_capturer.Cam)
            return;

        _path = EditorGUILayout.TextField("Save Path", _path == null? _savePath : _path);
        _name = EditorGUILayout.TextField("Name", _name);
        _mask = EditorGUILayout.MaskField("Culling Mask",_mask, UnityEditorInternal.InternalEditorUtility.layers);
        if (GUILayout.Button("Capture Scene"))
        {
            _urp_cam.renderPostProcessing = false;
            _colSceneTex = _capturer.CaptureCameraToTexture(_mask);
            _urp_cam.renderPostProcessing = true;

            SaveTexture(_colSceneTex, _path, _name);
        }
        EditorUtil.DrawSeparator();

        _renderDepth = EditorGUILayout.Toggle("Render Alternate (URP)", _renderDepth);

        if (_renderDepth)
        {
            _rendererData = (UniversalRendererData)EditorGUILayout.ObjectField("URP Renderer", _rendererData, typeof(UniversalRendererData), false);
            _featureName = EditorGUILayout.TextField("Featrue Name", _featureName);
            _depthMask = EditorGUILayout.MaskField("Capture Mask", _depthMask, UnityEditorInternal.InternalEditorUtility.layers);

            if (_feature) 
            {
                RenderObjects ro = _feature as RenderObjects;
                ro.settings.filterSettings.LayerMask = _depthMask;
                ro.Create();
            }
        
            if (GUILayout.Button("Capture")) {
                if (TryGetFeature(out _feature))
                    _feature.SetActive(true);
                _urp_cam.renderPostProcessing = false;
                _depthSceneTex = _capturer.CaptureCameraToTexture(_depthMask);
                _urp_cam.renderPostProcessing = true;
                SaveTexture(_depthSceneTex, _path, _name + "_alternate");
                if (TryGetFeature(out _feature))
                    _feature.SetActive(false);
            }
            EditorUtil.DrawSeparator();

        }
        else
            if (TryGetFeature(out _feature))
            _feature.SetActive(false);

  
        if (_colSceneTex && _depthSceneTex) 
        {
            EditorGUILayout.LabelField("Merge Alternate Texture 'r' to Scene Texture 'a'");
            if (GUILayout.Button("Merge"))
                SaveTexture(_capturer.MergeTextures(_colSceneTex,_depthSceneTex), _path, _name + "_comp");
        }

     
        if(_prev_renderDepth != _renderDepth && _renderDepth)
            if (TryGetFeature(out _feature))
                _feature.SetActive(true);
        _prev_renderDepth = _renderDepth;
    }
}