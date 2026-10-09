using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace StarStrike.Gameplay.FX
{
    public class PostProcessingSetup : MonoBehaviour
    {
        private void Start()
        {
            SetupGlobalVolume();
            EnableCameraPostProcessing();
        }

        private void SetupGlobalVolume()
        {
            GameObject volumeObj = new GameObject("GlobalVolume");
            Volume volume = volumeObj.AddComponent<Volume>();
            volume.isGlobal = true;

            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            
            // Add Bloom
            if (!profile.Has<Bloom>())
            {
                Bloom bloom = profile.Add<Bloom>(true);
                bloom.intensity.Override(1.5f);
                bloom.threshold.Override(0.9f);
                bloom.scatter.Override(0.7f);
            }

            // Add Chromatic Aberration
            if (!profile.Has<ChromaticAberration>())
            {
                ChromaticAberration ca = profile.Add<ChromaticAberration>(true);
                ca.intensity.Override(0.2f);
            }

            // Add Vignette
            if (!profile.Has<Vignette>())
            {
                Vignette vignette = profile.Add<Vignette>(true);
                vignette.intensity.Override(0.35f);
                vignette.smoothness.Override(0.2f);
                vignette.color.Override(Color.black);
            }

            volume.profile = profile;
            DontDestroyOnLoad(volumeObj);
        }

        private void EnableCameraPostProcessing()
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                UniversalAdditionalCameraData camData = mainCam.GetComponent<UniversalAdditionalCameraData>();
                if (camData == null)
                {
                    camData = mainCam.gameObject.AddComponent<UniversalAdditionalCameraData>();
                }
                camData.renderPostProcessing = true;
            }
        }
    }
}
