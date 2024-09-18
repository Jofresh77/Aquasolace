using System;
using Code.Scripts.Singletons;
using UnityEngine;

namespace Code.Scripts.Level
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

        private Material _boxMaterial;

        private float _minWaterLevel;
        private float _maxWaterLevel;
        private float _currentWaterLevel;
        
        private static readonly int Fill = Shader.PropertyToID("_Fill");

        private void Start()
        {
            if (!waterBox || !waterPlane) return;
            
            // getting the materials
            _boxMaterial = waterBox.GetComponent<MeshRenderer>().material;
            
            // setting min and max
            _minWaterLevel = 0;
            _maxWaterLevel = 85;
            _currentWaterLevel = GameManager.Instance.CurrentGwlPercentage;
            
            // first set the values and fills
            AdjustWaterBoxFill();
            AdjustWaterPlaneHeight();
        }

        private void Update()
        {
            if (!WaterLevelChanged()) return;
            
            AdjustWaterBoxFill();
            AdjustWaterPlaneHeight();
        }

        private void AdjustWaterBoxFill()
        {
            // Ensure that the current value is within the limits
            var waterLevel = MathF.Round(GameManager.Instance.CurrentGwlPercentage, 2);
            var progressValue = ValidateProgressValue(_minWaterLevel, _maxWaterLevel, waterLevel);

            // Calculate the percentage
            float percentage = (progressValue - _minWaterLevel) / (_maxWaterLevel - _minWaterLevel);

            if (percentage > 1f) percentage = 1f;
            
            float fillValue = minFillHeight + percentage * (1 - minFillHeight);

            fillValue = ValidateProgressValue(minFillHeight, 1, fillValue);
            
            // set the result
            _boxMaterial.SetFloat(Fill, fillValue);
        }

        private void AdjustWaterPlaneHeight()
        {
            // Ensure that the current value is within the limits
            var waterLevel = MathF.Round(GameManager.Instance.CurrentGwlPercentage, 2);
            var progressValue = ValidateProgressValue(_minWaterLevel, _maxWaterLevel, waterLevel);

            // Calculate the percentage
            float percentage = (progressValue - _minWaterLevel) / (_maxWaterLevel - _minWaterLevel);
            
            if (percentage > 1f) percentage = 1f;

            float newPlaneHeight = minPlaneHeight + percentage * (maxPlaneHeight - minPlaneHeight);

            newPlaneHeight = ValidateProgressValue(minPlaneHeight, maxPlaneHeight, newPlaneHeight);

            var planePosition = waterPlane.transform.position;
            planePosition.y = newPlaneHeight - Math.Abs(waterPlane.transform.parent.position.y);
            waterPlane.transform.position = planePosition;
        }

        private bool WaterLevelChanged()
        {
            var waterLevel = MathF.Round(GameManager.Instance.CurrentGwlPercentage, 0);
            var progressValue = ValidateProgressValue(_minWaterLevel, _maxWaterLevel, waterLevel);

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
