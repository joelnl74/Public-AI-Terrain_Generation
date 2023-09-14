using System;
using System.Globalization;
using Models;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace views
{
    public class SettingsView : MonoBehaviour
    {
        [SerializeField] private Button _generateButton;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Coast agent")]
        [SerializeField] private InputField _borderSize;
        
        [Header("Beach agent")]
        [SerializeField] private InputField _amountOfBeaches;
        [SerializeField] private InputField _inlandDistance;
        
        [SerializeField] private InputField _beachMaxHeight;
        [SerializeField] private InputField _beachSeaLevel;
        
        [Header("Mountain agent")]
        [SerializeField] private InputField _minMountainLength;
        [SerializeField] private InputField _maxMountainLength;
        
        [SerializeField] private InputField _minAmountOfAmounts;
        [SerializeField] private InputField _maxAmountOfAmounts;
        
        [SerializeField] private InputField _mountainWidth;
        [SerializeField] private InputField _moutainMaxHeight;
        
        [Header("Mountain agent")]
        [SerializeField] private InputField _minHillLength;
        [SerializeField] private InputField _maxHillLength;
        
        [SerializeField] private InputField _minAmountOfHills;
        [SerializeField] private InputField _maxAmountOfHills;
        
        [SerializeField] private InputField _hillWidth;
        [SerializeField] private InputField _hillMaxHeight;
        
        [Header("Volcano agent")]
        [SerializeField] private InputField _calderaWidth;
        [SerializeField] private InputField _calderaWidthRange;

        [SerializeField] private InputField _volcanoHeight;
        [SerializeField] private InputField _volcanoHeightRange;
        [SerializeField] private InputField _volcanoWidth;

        [Header("Noise agent")]
        [SerializeField] private InputField _noiseChance;
        [SerializeField] private InputField _noiseMinHeight;
        [SerializeField] private InputField _noiseMaxHeight;

        [Header("Toggle")] 
        [SerializeField] private Toggle _islandToggle;
        
        private SettingsModel _settingsModel;
        
        private bool _active;
        
        public void Setup(SettingsModel model, Action<SettingsModel> OnGenerate)
        {
            _settingsModel = model;
            
            // Coast
            _borderSize.text = model.borderSize.ToString();
            
            // Beach agent
            _amountOfBeaches.text = model.numberOfBeaches.ToString();
            _inlandDistance.text = model.inlandDistance.ToString();
            
            _beachMaxHeight.text = model.beachMaxHeight.ToString(CultureInfo.CurrentCulture);
            _beachSeaLevel.text = model.beachSealevel.ToString(CultureInfo.CurrentCulture);
            
            // Mountain
            _minMountainLength.text = model.minLength.ToString();
            _maxMountainLength.text = model.maxLength.ToString();
            
            _minAmountOfAmounts.text = model.minAmountOfMountains.ToString();
            _maxAmountOfAmounts.text = model.maxAmountOfMountains.ToString();
            
            _mountainWidth.text = model.mountainWidth.ToString();
            _moutainMaxHeight.text = model.maxHeight.ToString();
            
            // Hills
            _minHillLength.text = model.minHillLength.ToString();
            _maxHillLength.text = model.maxHillLength.ToString();
            
            _minAmountOfHills.text = model.minAmountOfHill.ToString();
            _maxAmountOfHills.text = model.maxAmountOfHill.ToString();
            
            _hillWidth.text = model.hillWidth.ToString();
            _hillMaxHeight.text = model.maxHillHeight.ToString();
            
            // Noise
            _noiseChance.text = model.randomNoiseGenerationPercentage.ToString();
            _noiseMinHeight.text = model.randomNoiseMinHeight.ToString(CultureInfo.CurrentCulture);
            _noiseMaxHeight.text = model.randomNoiseMaxHeight.ToString(CultureInfo.CurrentCulture);
            
            // Volcano agent
            _calderaWidth.text = model.calderaWidth.ToString();
            _calderaWidthRange.text = model.calderaWidthRange.ToString(CultureInfo.CurrentCulture);

            _volcanoHeight.text = model.volcanoHeight.ToString();
            _volcanoHeightRange.text = model.volcanoHeightRange.ToString(CultureInfo.CurrentCulture);
            _volcanoWidth.text = model.volcanoWidth.ToString();
            
            // Toggle
            _islandToggle.isOn = model.OneIsland;
             
            _generateButton.onClick.AddListener( () =>
            {
                SetModel();
                
                OnGenerate?.Invoke(_settingsModel);
            });
        }

        private void SetModel()
        {
            // Border size
            _settingsModel.borderSize = Convert.ToInt32(_borderSize.text);
            
            // Beach agent
            _settingsModel.minLength = Convert.ToInt32(_amountOfBeaches.text);
            _settingsModel.maxLength = Convert.ToInt32(_inlandDistance.text);
            
            _settingsModel.beachMaxHeight = (float)Convert.ToDouble(_beachMaxHeight.text);
            _settingsModel.beachSealevel = (float)Convert.ToDouble(_beachSeaLevel.text);
            
            // Mountain agent
            _settingsModel.minLength = Convert.ToInt32(_minMountainLength.text);
            _settingsModel.maxLength = Convert.ToInt32(_maxMountainLength.text);
            
            _settingsModel.minAmountOfMountains = Convert.ToInt32(_minAmountOfAmounts.text);
            _settingsModel.maxAmountOfMountains = Convert.ToInt32(_maxAmountOfAmounts.text);
            
            _settingsModel.maxHeight = Convert.ToInt32(_moutainMaxHeight.text);
            _settingsModel.mountainWidth = Convert.ToInt32(_mountainWidth.text);
            
            // Hill agent
            _settingsModel.minHillLength = Convert.ToInt32(_minHillLength.text);
            _settingsModel.maxHillLength = Convert.ToInt32(_maxHillLength.text);
            
            _settingsModel.minAmountOfHill = Convert.ToInt32(_minAmountOfHills.text);
            _settingsModel.maxAmountOfHill = Convert.ToInt32(_maxAmountOfHills.text);
            
            _settingsModel.maxHillHeight = Convert.ToInt32(_hillMaxHeight.text);
            _settingsModel.hillWidth = Convert.ToInt32(_hillWidth.text);
            
            // Noise agent
            _settingsModel.randomNoiseGenerationPercentage = Convert.ToInt32(_noiseChance.text);
            _settingsModel.randomNoiseMinHeight = (float)Convert.ToDouble(_noiseMinHeight.text);
            _settingsModel.randomNoiseMaxHeight = (float)Convert.ToDouble(_noiseMaxHeight.text);
            
            // Volcano agent
            _settingsModel.calderaWidth = Convert.ToInt32(_calderaWidth.text);
            _settingsModel.calderaWidthRange = (float)Convert.ToDouble(_calderaWidthRange.text);
            
            _settingsModel.volcanoHeight = Convert.ToInt32(_volcanoHeight.text);
            _settingsModel.volcanoHeightRange = (float)Convert.ToDouble(_volcanoHeightRange.text);
            _settingsModel.volcanoWidth = Convert.ToInt32(_volcanoWidth.text);

            _settingsModel.OneIsland = _islandToggle.isOn;
        }

        private void Update()
        {
            var key = Input.GetKeyDown(KeyCode.Escape);

            if (!key)
            {
                return;
            }
            
            _active = !_active;

            if (_active)
            {
                _canvasGroup.alpha = 1;
                _canvasGroup.interactable = true;
            }
            else
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.interactable = false;
            }
        }
    }
}
