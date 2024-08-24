using UnityEngine;

namespace Code.Scripts.PlayerControllers.UI
{
    public class BlurMaskController : MonoBehaviour
    {
        public Camera mainCamera;
        public Camera blurCamera;
        
        public RectTransform maskRectTransform;
        
        public float maskSize = 100f;
        
        public RenderTexture gameViewTexture;
        
        public Material blurMaterial;
        
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");

        private void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (maskRectTransform == null)
                maskRectTransform = GetComponent<RectTransform>();

            maskRectTransform.sizeDelta = new Vector2(maskSize, maskSize);

            // Set up the blur camera
            if (blurCamera == null || gameViewTexture == null) return;
            
            blurCamera.targetTexture = gameViewTexture;
            if (blurMaterial != null)
            {
                blurMaterial.SetTexture(MainTex, gameViewTexture);
            }
        }

        private void Update()
        {
            Vector2 mousePosition = Input.mousePosition;
            maskRectTransform.position = mousePosition;
        }

    }
}
