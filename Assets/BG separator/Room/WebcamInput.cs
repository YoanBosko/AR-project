using UnityEngine;
using System.Collections;

namespace NNCam {

public sealed class WebcamInput : MonoBehaviour
{
    #region Editable attributes
    [SerializeField] string _deviceName = "";
    #endregion

    #region Platform-specific settings
    #if PLATFORM_ANDROID
        private const int WIDTH = 640;
        private const int HEIGHT = 480;
    #else
        private const int WIDTH = 1280;
        private const int HEIGHT = 720;
    #endif
    #endregion

    #region Internal objects
    WebCamTexture _webcam;
    RenderTexture _buffer;
    #endregion

    #region Public properties
    public Texture Texture => _buffer;
    #endregion

    #region MonoBehaviour implementation
    void Start()
    {
        #if PLATFORM_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
            return;
        }
        #endif

        InitializeWebcam();
    }

    void InitializeWebcam()
    {
        // Debug output untuk device dan kamera yang tersedia
        Debug.Log($"System Info: {SystemInfo.deviceModel}");
        Debug.Log($"GPU: {SystemInfo.graphicsDeviceName}");
        Debug.Log($"Compute Shader Support: {SystemInfo.supportsComputeShaders}");

        WebCamDevice[] devices = WebCamTexture.devices;
        Debug.Log($"Detected cameras: {devices.Length}");
        foreach (WebCamDevice device in devices)
        {
            Debug.Log($"Camera: {device.name} (isFrontFacing: {device.isFrontFacing})");
        }

        // Inisialisasi webcam
        if (string.IsNullOrEmpty(_deviceName) && devices.Length > 0)
        {
            _deviceName = devices[0].name;
        }

        _webcam = new WebCamTexture(_deviceName);
        _webcam.requestedWidth = 1280;
        _webcam.requestedHeight = 720;
        
        _buffer = new RenderTexture(WIDTH, HEIGHT, 0);
        _webcam.Play();

        StartCoroutine(WaitForWebcam());
    }

    private IEnumerator WaitForWebcam()
    {
        while (_webcam.width <= 16)
        {
            Debug.Log($"Waiting for webcam... Current size: {_webcam.width}x{_webcam.height}");
            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log($"Webcam initialized: {_webcam.width}x{_webcam.height}");
    }

    void OnDestroy()
    {
        if (_webcam != null)
        {
            _webcam.Stop();
            Destroy(_webcam);
        }
        if (_buffer != null) Destroy(_buffer);
    }

    void Update()
    {
        if (_webcam != null)
        {
            if (!_webcam.isPlaying)
            {
                Debug.LogWarning("Webcam stopped playing, attempting to restart...");
                _webcam.Play();
            }
            
            if (_webcam.width <= 16) // Unity returns 16x16 when camera fails
            {
                Debug.LogError("Invalid webcam texture size");
                return;
            }
        }
        
        // Existing update code
        if (!_webcam.didUpdateThisFrame) return;
        var vflip = _webcam.videoVerticallyMirrored;
        var scale = new Vector2(1, vflip ? -1 : 1);
        var offset = new Vector2(0, vflip ? 1 : 0);
        Graphics.Blit(_webcam, _buffer, scale, offset);
    }

    #if PLATFORM_ANDROID
    void OnApplicationFocus(bool focus)
    {
        if (focus && UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
        {
            if (_webcam == null)
                InitializeWebcam();
        }
    }
    #endif

    #endregion
}

} // namespace NNCam
