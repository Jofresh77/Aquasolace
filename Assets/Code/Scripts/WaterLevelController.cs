using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Code.Scripts
{
    public class WaterLevelController : MonoBehaviour
    {

        [SerializeField] private GameObject waterBox;
        [SerializeField] private GameObject waterPlane;

        [Header("Values for adjusting the box by script")][Space]
        [SerializeField] private float minFillHeight = 0.2f;
        
        [Header("Values for adjusting the plane by script")][Space]
        [SerializeField] private float minPlaneHeight = 2.11f;
        [SerializeField] private float maxPlaneHeight = 5.72f;

        private Material _boxMaterial; // the box outside
        // private Material _planeMaterial; // the plane inside the box fit foam and so on

        private float _minWaterLevel;
        private float _maxWaterLevel;
        private float _currentWaterLevel;

        // Start is called before the first frame update
        void Start()
        {
            if (!waterBox || !waterPlane) return;
            
            // getting the materials
            _boxMaterial = waterBox.GetComponent<MeshRenderer>().material;
            // _planeMaterial = waterPlane.GetComponent<MeshRenderer>().material;
            
            // setting min and max
            _minWaterLevel = GameManager.Instance.GetGwlNegThreshold();
            _maxWaterLevel = GameManager.Instance.GetGwlPosThreshold();
            _currentWaterLevel = GameManager.Instance.GroundWaterLevel;
            
            // first set the values and fills
            AdjustWaterBoxFill();
            AdjustWaterPlaneHeight();
        }

        // Update is called once per frame
        void Update()
        {
            if (WaterLevelChanged())
            {
                AdjustWaterBoxFill();
                AdjustWaterPlaneHeight();
            }
        }

        private void AdjustWaterPlaneHeight()
        {
            // Ensure that the current value is within the limits
            var waterLevel = MathF.Round(GameManager.Instance.GroundWaterLevel, 2);
            var progressValue = ValidateProgressValue(GameManager.Instance.GetGwlNegThreshold(), GameManager.Instance.GetGwlPosThreshold(), waterLevel);

            // Calculate the percentage
            float percentage = (progressValue - _minWaterLevel) / (_maxWaterLevel - _minWaterLevel);
            
            if (percentage > 1f) percentage = 1f;

            float newPlaneHeight = minPlaneHeight + percentage * (maxPlaneHeight - minPlaneHeight);

            newPlaneHeight = ValidateProgressValue(minPlaneHeight, maxPlaneHeight, newPlaneHeight);

            var planePosition = waterPlane.transform.position;
            planePosition.y = newPlaneHeight - Math.Abs(waterPlane.transform.parent.position.y);
            waterPlane.transform.position = planePosition;
        }

        private void AdjustWaterBoxFill()
        {
            // Ensure that the current value is within the limits
            var waterLevel = MathF.Round(GameManager.Instance.GroundWaterLevel, 2);
            var progressValue = ValidateProgressValue(GameManager.Instance.GetGwlNegThreshold(), GameManager.Instance.GetGwlPosThreshold(), waterLevel);

            // Calculate the percentage
            float percentage = (progressValue - _minWaterLevel) / (_maxWaterLevel - _minWaterLevel);

            if (percentage > 1f) percentage = 1f;
            
            float fillValue = minFillHeight + percentage * (1 - minFillHeight);

            fillValue = ValidateProgressValue(minFillHeight, 1, fillValue);
            
            // set the result
            _boxMaterial.SetFloat("_Fill", fillValue);
        }

        private bool WaterLevelChanged()
        {
            var waterLevel = MathF.Round(GameManager.Instance.GroundWaterLevel, 0);
            var progressValue = ValidateProgressValue(GameManager.Instance.GetGwlNegThreshold(), GameManager.Instance.GetGwlPosThreshold(), waterLevel);

            if (Math.Abs(_currentWaterLevel - progressValue) > 0.1f)
            {
                return true;
            }

            return false;
        }
        
        private float ValidateProgressValue(float minValue, float maxValue, float currentValue)
        {
            // Ensure that the current value is within the limits
            return Mathf.Clamp(currentValue, minValue, maxValue);
        }
    }
}
