using UnityEngine;

namespace OndaS.Gameplay.Earthquake
{
    // The building remains intact during preparation and the shaking sequence.
    public sealed class OndaSEarthquakeAftermath : MonoBehaviour
    {
        [SerializeField] private OndaSFirstEarthquake earthquake;
        [SerializeField] private GameObject aftermath;
        [SerializeField] private GameObject[] removeAfterEarthquake;

        private void Awake()
        {
            if (aftermath != null) aftermath.SetActive(false);
        }

        private void Update()
        {
            if (earthquake == null || aftermath == null || !earthquake.HasFinished) return;
            if (removeAfterEarthquake != null)
                foreach (var piece in removeAfterEarthquake)
                    if (piece != null) piece.SetActive(false);
            aftermath.SetActive(true);
            enabled = false;
        }
    }
}
