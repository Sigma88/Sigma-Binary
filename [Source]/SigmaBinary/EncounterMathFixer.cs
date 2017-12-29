using System;
using UnityEngine;


namespace SigmaBinaryPlugin
{
    public static class EncounterMathFixer
    {
        public static Vector3d getPositionAtUTSigma(this Orbit orbit, double UT)
        {
            return orbit.getPositionAtT(orbit.getObtAtUTSigma(UT));
        }

        public static double getObtAtUTSigma(this Orbit orbit, double UT)
        {
            double num;
            if (orbit.eccentricity < 1.0)
            {
                if (double.IsPositiveInfinity(UT))
                    UT = double.MaxValue;
                if (double.IsNegativeInfinity(UT))
                    UT = double.MinValue;
                if (double.IsNaN(UT))
                    UT = 0;

                num = (UT - orbit.epoch + orbit.ObTAtEpoch) % orbit.period;

                if (num > orbit.period / 2.0)
                {
                    num -= orbit.period;
                }
            }
            else
            {
                if (double.IsInfinity(UT))
                {
                    return UT;
                }
                num = orbit.ObTAtEpoch + (UT - orbit.epoch);
            }
            return num;
        }

        public static double SolveClosestApproachWithoutComplaining(Orbit p, Orbit s, ref double UT, double dT, double threshold, double MinUT, double MaxUT, double epsilon, int maxIterations, ref int iterationCount)
        {
            if (UT < MinUT)
            {
                return -1.0;
            }
            if (UT > MaxUT)
            {
                return -1.0;
            }
            iterationCount = 0;
            double num = Math.Abs((p.getPositionAtUTSigma(UT) - s.getPositionAtUTSigma(UT)).sqrMagnitude);
            while (dT > epsilon && iterationCount < maxIterations)
            {
                double num2 = (p.getPositionAtUTSigma(UT + dT) - s.getPositionAtUTSigma(UT + dT)).sqrMagnitude;
                double num3 = (p.getPositionAtUTSigma(UT - dT) - s.getPositionAtUTSigma(UT - dT)).sqrMagnitude;
                if (UT - dT < MinUT)
                {
                    num3 = 1.7976931348623157E+308;
                }
                if (UT + dT > MaxUT)
                {
                    num2 = 1.7976931348623157E+308;
                }
                num = Math.Min(num, Math.Min(num2, num3));
                if (num == num3)
                {
                    UT -= dT;
                }
                else if (num == num2)
                {
                    UT += dT;
                }
                dT /= 2.0;
                iterationCount++;
                UnityEngine.Debug.DrawLine(ScaledSpace.LocalToScaledSpace(p.referenceBody.position), ScaledSpace.LocalToScaledSpace(p.getPositionAtUTSigma(UT)), XKCDColors.Lime * 0.5f);
            }
            return Math.Sqrt(num);
        }

        public static int FindClosestPointsRevertedCauseNewOneSucks(Orbit p, Orbit s, ref double CD, ref double CCD, ref double FFp, ref double FFs, ref double SFp, ref double SFs, double epsilon, int maxIterations, ref int iterationCount)
        {
            Orbit.FindClosestPoints_old(p, s, ref CD, ref CCD, ref FFp, ref FFs, ref SFp, ref SFs, epsilon, maxIterations, ref iterationCount);
            return 2;
        }

        public static bool CheckEncounterButDontBitchAboutIt(Orbit p, Orbit nextPatch, double startEpoch, OrbitDriver sec, CelestialBody targetBody, PatchedConics.SolverParameters pars)
        {
            Orbit orbit = sec.orbit;
            double num = 1.1;
            if (GameSettings.ALWAYS_SHOW_TARGET_APPROACH_MARKERS && sec.celestialBody == targetBody)
            {
                num = Math.Sqrt(orbit.semiMajorAxis / sec.celestialBody.sphereOfInfluence);
            }
            if (!Orbit.PeApIntersects(p, orbit, sec.celestialBody.sphereOfInfluence * num))
            {
                return false;
            }
            if (p.closestEncounterLevel < Orbit.EncounterSolutionLevel.ORBIT_INTERSECT)
            {
                p.closestEncounterLevel = Orbit.EncounterSolutionLevel.ORBIT_INTERSECT;
                p.closestEncounterBody = sec.celestialBody;
            }
            double clEctr = p.ClEctr1;
            double clEctr2 = p.ClEctr2;
            double fEVp = p.FEVp;
            double fEVs = p.FEVs;
            double sEVp = p.SEVp;
            double sEVs = p.SEVs;
            int num2 = FindClosestPointsRevertedCauseNewOneSucks(p, orbit, ref clEctr, ref clEctr2, ref fEVp, ref fEVs, ref sEVp, ref sEVs, 0.0001, pars.maxGeometrySolverIterations, ref pars.GeoSolverIterations);
            if (num2 < 1)
            {
                return false;
            }
            double dTforTrueAnomaly = p.GetDTforTrueAnomaly(fEVp, 0.0);
            double dTforTrueAnomaly2 = p.GetDTforTrueAnomaly(sEVp, 0.0);
            double num3 = dTforTrueAnomaly + startEpoch;
            double num4 = dTforTrueAnomaly2 + startEpoch;

            // avoid bad numbers
            if (double.IsInfinity(num3) && !double.IsInfinity(num4))
                num3 = num4;
            if (double.IsInfinity(num4) && !double.IsInfinity(num3))
                num4 = num3;

            if (double.IsInfinity(num3) && double.IsInfinity(num4))
            {
                UnityEngine.Debug.Log("CheckEncounter: both intercept UTs are infinite");
                return false;
            }
            if ((num3 < p.StartUT || num3 > p.EndUT) && (num4 < p.StartUT || num4 > p.EndUT))
            {
                return false;
            }
            if (num4 < num3 || num3 < p.StartUT || num3 > p.EndUT)
            {
                UtilMath.SwapValues(ref fEVp, ref sEVp);
                UtilMath.SwapValues(ref fEVs, ref sEVs);
                UtilMath.SwapValues(ref clEctr, ref clEctr2);
                UtilMath.SwapValues(ref dTforTrueAnomaly, ref dTforTrueAnomaly2);
                UtilMath.SwapValues(ref num3, ref num4);
            }
            if (num4 < p.StartUT || num4 > p.EndUT || double.IsInfinity(num4))
            {
                num2 = 1;
            }
            p.numClosePoints = num2;
            p.FEVp = fEVp;
            p.FEVs = fEVs;
            p.SEVp = sEVp;
            p.SEVs = sEVs;
            p.ClEctr1 = clEctr;
            p.ClEctr2 = clEctr2;
            if (Math.Min(p.ClEctr1, p.ClEctr2) > sec.celestialBody.sphereOfInfluence)
            {
                if (GameSettings.ALWAYS_SHOW_TARGET_APPROACH_MARKERS && sec.celestialBody == targetBody)
                {
                    p.UTappr = startEpoch;
                    p.ClAppr = PatchedConics.GetClosestApproach(p, orbit, startEpoch, p.nearestTT * 0.5, pars);
                    p.closestTgtApprUT = p.UTappr;
                }
                return false;
            }
            if (p.closestEncounterLevel < Orbit.EncounterSolutionLevel.SOI_INTERSECT_1)
            {
                p.closestEncounterLevel = Orbit.EncounterSolutionLevel.SOI_INTERSECT_1;
                p.closestEncounterBody = sec.celestialBody;
            }
            p.timeToTransition1 = dTforTrueAnomaly;
            p.secondaryPosAtTransition1 = orbit.getPositionAtUTSigma(num3);
            UnityEngine.Debug.DrawLine(ScaledSpace.LocalToScaledSpace(p.referenceBody.position), ScaledSpace.LocalToScaledSpace(p.secondaryPosAtTransition1), Color.yellow);
            p.timeToTransition2 = dTforTrueAnomaly2;
            p.secondaryPosAtTransition2 = orbit.getPositionAtUTSigma(num4);
            UnityEngine.Debug.DrawLine(ScaledSpace.LocalToScaledSpace(p.referenceBody.position), ScaledSpace.LocalToScaledSpace(p.secondaryPosAtTransition2), Color.red);
            p.nearestTT = p.timeToTransition1;
            p.nextTT = p.timeToTransition2;
            if (double.IsNaN(p.nearestTT))
            {
                UnityEngine.Debug.Log(string.Concat(new object[] { "nearestTT is NaN! t1: ", p.timeToTransition1, ", t2: ", p.timeToTransition2, ", FEVp: ", p.FEVp, ", SEVp: ", p.SEVp }));
            }
            p.UTappr = startEpoch;
            p.ClAppr = PatchedConics.GetClosestApproach(p, orbit, startEpoch, p.nearestTT * 0.5, pars);
            if (PatchedConics.EncountersBody(p, orbit, nextPatch, sec, startEpoch, pars))
            {
                return true;
            }
            if (num2 > 1)
            {
                p.closestEncounterLevel = Orbit.EncounterSolutionLevel.SOI_INTERSECT_2;
                p.closestEncounterBody = sec.celestialBody;
                UnityEngine.Debug.DrawLine(ScaledSpace.LocalToScaledSpace(p.getPositionAtUTSigma(p.UTappr)), ScaledSpace.LocalToScaledSpace(orbit.getPositionAtUTSigma(p.UTappr)), XKCDColors.Orange * 0.5f);
                p.UTappr = startEpoch + p.nearestTT;
                p.ClAppr = PatchedConics.GetClosestApproach(p, orbit, startEpoch, (p.nextTT - p.nearestTT) * 0.5, pars);
                if (PatchedConics.EncountersBody(p, orbit, nextPatch, sec, startEpoch, pars))
                {
                    return true;
                }
                UnityEngine.Debug.DrawLine(ScaledSpace.LocalToScaledSpace(p.getPositionAtUTSigma(p.UTappr)), ScaledSpace.LocalToScaledSpace(orbit.getPositionAtUTSigma(p.UTappr)), XKCDColors.Orange);
            }
            if (sec.celestialBody == targetBody)
            {
                p.closestTgtApprUT = p.UTappr;
            }
            return false;
        }
    }
}
