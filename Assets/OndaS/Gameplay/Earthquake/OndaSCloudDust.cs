using UnityEngine;

namespace OndaS.Gameplay.Earthquake
{
    // Broad, soft billboards layer into a drifting dust haze rather than visible spheres.
    public static class OndaSCloudDust
    {
        public static void Configure(ParticleSystem particles, Material material, bool impact = false)
        {
            if (particles == null || material == null) return;
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.playOnAwake = false;
            main.loop = !impact;
            main.duration = impact ? 3f : 10f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = impact ? new ParticleSystem.MinMaxCurve(2.2f, 3.5f) : new ParticleSystem.MinMaxCurve(12f, 18f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(.01f, .06f);
            main.startSize = impact ? new ParticleSystem.MinMaxCurve(1.1f, 2.1f) : new ParticleSystem.MinMaxCurve(3.5f, 5.5f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.283185f);
            main.startColor = new Color(.48f, .31f, .19f, impact ? .18f : .12f);
            main.gravityModifier = 0f;
            main.maxParticles = impact ? 18 : 90;
            var emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = impact ? 0f : 3.5f;
            emission.SetBursts(impact ? new[] { new ParticleSystem.Burst(0f, (short)7) } : new ParticleSystem.Burst[0]);
            var shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = impact ? new Vector3(1.2f, .35f, 1.2f) : new Vector3(4f, 2.3f, 11f);
            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-.035f, .035f);
            velocity.y = new ParticleSystem.MinMaxCurve(-.045f, -.012f);
            velocity.z = new ParticleSystem.MinMaxCurve(-.035f, .035f);
            var color = particles.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, .2f), new GradientAlphaKey(.8f, .7f), new GradientAlphaKey(0f, 1f) });
            color.color = new ParticleSystem.MinMaxGradient(gradient);
            var size = particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, .7f, 1f, 1.25f));
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }
    }
}
