using UnityEngine;

namespace NNCam {

public sealed class Compositor : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] WebcamInput _input = null;
    [SerializeField] Texture2D _background = null;
    [SerializeField, Range(0.001f, 0.99f)] float _threshold = 0.001f;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] Shader _shader = null;

    #endregion

    #region Internal objects

    SegmentationFilter _filter;
    Material _material;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _filter = new SegmentationFilter(_resources);
        _material = new Material(_shader);
    }

    void OnDestroy()
    {
        _filter.Dispose();
        Destroy(_material);
    }

    void Update()
      => _filter.ProcessImage(_input.Texture);

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        _material.SetTexture("_Background", _background);
        _material.SetTexture("_CameraFeed", _input.Texture);
        _material.SetTexture("_Mask", _filter.MaskTexture);
        _material.SetFloat("_Threshold", _threshold);
        Graphics.Blit(null, destination, _material, 0);

        // Debug mask
        // Graphics.Blit(_filter.MaskTexture, destination);
        
        // Atau untuk perbandingan
        // Material debugMaterial = new Material(Shader.Find("Unlit/Texture"));
        // debugMaterial.mainTexture = _filter.MaskTexture;
        // Graphics.Blit(null, destination, debugMaterial);
    }

    #endregion
}

} // namespace NNCam
