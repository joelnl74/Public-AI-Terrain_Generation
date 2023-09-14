using Models;
using UnityEngine;
using views;

namespace Controllers
{
    public class SettingsController : MonoBehaviour
    {
        [SerializeField] private SettingsView _settingsView;

        private void Start()
        {
            _settingsView.Setup(new SettingsModel(), GenerateTerrain);
        }

        private void GenerateTerrain(SettingsModel settingsModel)
        {
            TerrainManager.Instance.GenerateTerrain(settingsModel);
        }
    }
}
