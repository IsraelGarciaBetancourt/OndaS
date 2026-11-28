using UnityEngine;

namespace OndaS.Gameplay
{
    internal sealed class OndaSFootsteps
    {
        private const float StartingProgress = 0.65f;
        private const float MinimumSpeed = 0.18f;

        private readonly AudioSource source;
        private readonly AudioClip[] clips;
        private readonly System.Random variation = new System.Random();
        private float strideProgress = StartingProgress;
        private int lastClipIndex = -1;

        public OndaSFootsteps(GameObject player)
        {
            clips = Resources.LoadAll<AudioClip>("Audio/Footsteps");
            source = player.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
        }

        public void UpdateMovement(float horizontalSpeed, bool hasMovementInput, bool grounded,
            bool sprinting, bool crouching, float deltaTime)
        {
            if (!hasMovementInput || horizontalSpeed < MinimumSpeed)
            {
                ResetStride();
                return;
            }

            if (!grounded || clips.Length == 0) return;

            float stepLength = sprinting ? 1.55f : crouching ? 1.05f : 1.5f;
            strideProgress += horizontalSpeed * deltaTime / stepLength;
            if (strideProgress < 1f) return;

            strideProgress = Mathf.Repeat(strideProgress, 1f);
            int index = clips.Length > 1 && lastClipIndex >= 0
                ? (lastClipIndex + 1 + variation.Next(clips.Length - 1)) % clips.Length
                : variation.Next(clips.Length);
            lastClipIndex = index;
            float basePitch = sprinting ? 0.93f : 1f;
            source.pitch = basePitch + Mathf.Lerp(-0.06f, 0.06f, (float)variation.NextDouble());
            float volume = sprinting ? 0.75f : crouching ? 0.26f : 0.48f;
            source.PlayOneShot(clips[index], volume);
        }

        public void ResetStride() => strideProgress = StartingProgress;

        public void Stop()
        {
            ResetStride();
            if (source != null) source.Stop();
        }
    }
}
