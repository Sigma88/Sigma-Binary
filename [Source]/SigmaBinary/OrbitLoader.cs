using System;
using Kopernicus;
using Kopernicus.Components;
using Kopernicus.Configuration;


namespace SigmaBinaryPlugin
{
    public class SigmaOrbitLoader : OrbitLoader
    {
        public SigmaOrbitLoader(PSystemBody body)
        {
            // Is this the parser context?
            if (!Injector.IsInPrefab)
            {
                throw new InvalidOperationException("Must be executed in Injector context.");
            }

            // If this body needs orbit controllers, create them
            if (body.orbitDriver == null)
            {
                body.orbitDriver = body.celestialBody.gameObject.AddComponent<OrbitDriver>();
                body.orbitRenderer = body.celestialBody.gameObject.AddComponent<OrbitRenderer>();
            }
            body.celestialBody.gameObject.AddOrGetComponent<OrbitRendererUpdater>();
            body.orbitDriver.updateMode = OrbitDriver.UpdateMode.UPDATE;

            // Store values
            Value = body.celestialBody;
            Value.orbitDriver = body.orbitDriver;
            Value.orbitDriver.orbit = body.orbitDriver.orbit ?? new Orbit();
        }
    }
}
