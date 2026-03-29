using Microsoft.Xna.Framework;
using VibeSopwith.Game.Utils;

namespace VibeSopwith.Game.Core
{
    internal static class Autopilot
    {
        //----------------------------------------------------------------------------------------------------
        //                                                                                     ┌───────────┐
        //                                                                 ┌───────────┐       | PreFinal  |
        //                                                                 | Final     |       |   ->      │ 
        //                                                                 |   <-      │       └───────────┘ 
        //                                                                 └───────────┘           
        //                                                                     
        //                                     ┌────────────────┐ 
        //                          PreTouch   │   <- 20° +- 9° │
        //                                     └────────────────┘  T
        // Runway:          -----------------------------------------
        //----------------------------------------------------------------------------------------------------

        public record struct ApproachZone(float EntryX, float ExitX, float BottomEntryY, float TopEntryY, Vector2 FunnelBottom, Vector2 FunnelTop);

        public enum LoopDirection { NoLoop, PitchUp, PitchDown }

        // Zones as per diagram above
        // Preferred loop direction is always UP if not above certain threshold, otherwise DOWN.
        // When autopilot is engaged the choice is to be made between Final and PreFinal for initial approach, depending on initial angle
        // between Plane direction and Zone direction. I.e.
        // - IF plane is positioned at X >= Final.X and flying left choose Final.
        // - IF plane is positioned at X < PreFinal.X and flying right choose PreFinal. 
        // - IF plane is positioned at X < Final.X and flying left choose PreFinal of the opposite-side approach.
        // - IF plane is positioned at X >= PreFinal.X and flying right it is considered already IN PreFinal -> choose Final.
        // Once in PreFinal plane initiates maneuver to orient its Direction with Final.Direction, and to move within boundaries of Final.
        // Once in Final plane initiates maneuver to orient its Direction with PreTouch.Direction, and to move within boundaries of PreTouch.
        // Once in PreTouch plane ceases maneuvering until touchdown. Upon touchdown decelerate to zero.

        public record struct Approach(Ground.Runway Runway, ApproachZone PreFinal, ApproachZone Final, ApproachZone PreTouch, float LoopHeightThreshold, float LandingSpeedDistance);

        public abstract record ApproachPhase()
        {
            public sealed record Failure() : ApproachPhase;
            public sealed record Initial(Approach Approach, ApproachZone TargetZone) : ApproachPhase;
            public sealed record PreFinal(Approach Approach, ApproachZone FinalZone, LoopDirection Loop) : ApproachPhase;
            public sealed record Final(Approach Approach, ApproachZone PreTouchZone) : ApproachPhase;
            public sealed record PreTouch(Approach Approach) : ApproachPhase;
            public sealed record Touchdown(Approach Approach) : ApproachPhase;

            public static Failure Fail = new Failure();
        }

        private enum Cardinal { Left, Right }
        private static float ToFactor(this Cardinal c) => c == Cardinal.Left ? -1f : +1f;

        private static (Approach approach, ApproachZone zone)? ChooseInitialZone(Airplane plane, Approach approach)
        {
            var approachDirection = approach.Final.EntryX < approach.PreTouch.EntryX ? Cardinal.Right : Cardinal.Left;
            var planeDirectionAngle = plane.Direction.ToAngle();
            var flightDirection = 
                plane.Direction.X == 0f ? approachDirection :       // If flying strictly vertically consider it in direction of approach.
                -float.Pi/2f < planeDirectionAngle && planeDirectionAngle < +float.Pi/2f ? Cardinal.Right :
                Cardinal.Left;

            switch (approachDirection)
            {
                case Cardinal.Left:
                    return
                        flightDirection == Cardinal.Left ? 
                            (plane.Position.X >= approach.Final.EntryX 
                            ? (approach, approach.Final) 
                            : null) :
                        flightDirection == Cardinal.Right ? 
                            (plane.Position.X >= approach.PreFinal.EntryX 
                            ? (approach, approach.Final) 
                            : (approach, approach.PreFinal)) :
                        null;

                case Cardinal.Right:
                    return
                        flightDirection == Cardinal.Right ?
                            (plane.Position.X <= approach.Final.EntryX
                            ? (approach, approach.Final)
                            : null) :
                        flightDirection == Cardinal.Left ?
                            (plane.Position.X <= approach.PreFinal.EntryX
                            ? (approach, approach.Final)
                            : (approach, approach.PreFinal)) :
                        null;
                default: 
                    return null;
            }
        }

        /// <summary>
        /// Initiates auto landing.
        /// </summary>
        /// <param name="approaches">Prioritised sequence of approaches to choose from</param>
        public static ApproachPhase InitiateAutoLanding(Airplane plane, IEnumerable<Approach> approaches) => 
            approaches
                .Select(approach => ChooseInitialZone(plane, approach))
                .Where(a => a != null)
                .Select(a => new ApproachPhase.Initial(a!.Value.approach, a!.Value.zone) as ApproachPhase)
                .Append(ApproachPhase.Fail)
                .First();

        // Find and return a point `Pe` and vector `Ve` such that
        // - Pe.X = zone.EntryX - i.e. Pe lies on a vertical "axis" X==zone.EntryX
        // - Pe.Y is between zone.BottomEntryY and zone.TopEntryY
        // - vector `Ve` (Pe - currentPos) is between zone.FunnelBottom and zone.FunnelTop. I suggest using Physics.ToAngle extensions for that.
        // - Generally, up to the full Y-range at X==zone.EntryX may be suitable for Pe. Pick a point vertically closest to the middle of the range between zone.BottomEntryY and zone.TopEntryY.
        // Return null if such point+vector combination cannot be computed.
        private static (Vector2 Pe, Vector2 Ve)? GetProjectedEntry(Vector2 currentPos, ApproachZone zone)
        {
            float dx = zone.EntryX - currentPos.X;

            // Compute intersections of funnel rays with the entry line.
            // Invariants guarantee: FunnelBottom.X != 0 and FunnelTop.X != 0.
            float tB = dx / zone.FunnelBottom.X;
            float tT = dx / zone.FunnelTop.X;

            // Only forward intersections are valid.
            if (tB <= 0f || tT <= 0f)
                return null;

            float Yb = currentPos.Y + tB * zone.FunnelBottom.Y;
            float Yt = currentPos.Y + tT * zone.FunnelTop.Y;

            float minFunnelY = MathF.Min(Yb, Yt);
            float maxFunnelY = MathF.Max(Yb, Yt);

            // Intersect funnel Y-range with the allowed entry window.
            float entryMin = MathF.Max(zone.BottomEntryY, minFunnelY);
            float entryMax = MathF.Min(zone.TopEntryY, maxFunnelY);

            if (entryMin > entryMax)
                return null;

            // Choose Y closest to the midpoint of the allowed entry window.
            float targetMid = 0.5f * (zone.BottomEntryY + zone.TopEntryY);

            float PeY =
                targetMid < entryMin ? entryMin :
                targetMid > entryMax ? entryMax :
                targetMid;

            var Pe = new Vector2(zone.EntryX, PeY);
            var Ve = Pe - currentPos;

            return (Pe, Ve);
        }

        private static Airplane.Inputs throttleForCruise(Airplane plane, Airplane.Inputs inputs) =>
            plane.Speed > Airplane.CruiseSpeed
            ? inputs
            : inputs with { Throttle = Airplane.ThrottleInput.Throttling };

        private static Airplane.Inputs brakeForLanding(Airplane plane, Airplane.Inputs inputs) =>
            plane.Speed <= Airplane.MaxLandingSpeed
            ? inputs
            : inputs with { Throttle = Airplane.ThrottleInput.Reversing };

        // Schematic implementation of SteerTowards:
        // public static (LoopDirection? ChosenLoop, Airplane.Inputs Inputs) SteerTowards(this Airplane plane, Approach approach, ApproachZone zone, LoopDirection? chosenLoop)
        //
        //
        // Find a point `Pe` and vector `Ve` by `GetProjectedEntry` function.
        //
        // Determine appropriate Spin `Se` for the plane to be oriented "gear down" when flying in direction Ve.
        // Which would be SpinBasis.Down when Direction is pointing to right (X-plus), and SpinBasis.Up when Direction is pointing to left (X-minus)
        // If appropriate Spin differs from current plane.Spin, emit Roll input.
        //
        // If we are flying away from that point, "initiate" a loop according to current plane position and approach.LoopHeightThreshold
        // If loop already initiated, assume continued looping in chosenLoop direction.
        //
        // According to the chosenLoop, computed Pe and Ve, current plane.Direction and computed `Se` spin emit Pitch input.
        // 
        // Speed is to be kept at current level until we are (horizontally only) within approach.LandingSpeedDistance from approach.PreTouchZone.EntryX - i.e. no Throttle input.
        // 
        // Return chosen loop direction and record of emitted inputs as a tuple.

        /// <summary>
        /// Returns a set of inputs which, when applied to plane would move it closer to entry into a given "zone" 
        /// </summary>
        public static (LoopDirection ChosenLoop, Airplane.Inputs Inputs) SteerTowards(this Airplane plane, Approach approach, ApproachZone zone, LoopDirection chosenLoop)
        {
            // Default: no inputs, preserve chosenLoop
            var inputs = Airplane.Inputs.Clean();

            // 1. Project desired entry point and vector
            var projected = GetProjectedEntry(plane.Position, zone);
            if (projected == null)
                return (chosenLoop, inputs);

            var (Pe, Ve) = projected.Value;

            // 2. Determine desired spin Se (gear down when flying Ve)
            var desiredSpin = Ve.X >= 0f ? BasisSpin.Down : BasisSpin.Up;

            // 3. Roll if current spin differs from desired spin
            inputs.Roll = plane.Spin == desiredSpin
                ? Airplane.RollInput.None
                : Airplane.RollInput.Roll;

            // 4. Determine if we are flying away from Pe (Option B: angle > 90°)
            float veAngle = Ve.ToAngle();
            float diff = (veAngle - plane.Direction.ToAngle()).ToNormal().ToAngle();

            bool flyingAway = MathF.Abs(diff) > (MathF.PI * 0.5f);

            // 5. Choose / maintain loop direction
            var newLoop = chosenLoop;
            if (flyingAway && chosenLoop == LoopDirection.NoLoop)
            {
                // Preferred loop direction is always UP if not above certain threshold, otherwise DOWN.
                // But the plane here is inverted so UP and DOWN swapped.
                newLoop = plane.Position.Y < approach.LoopHeightThreshold
                    ? LoopDirection.PitchDown
                    : LoopDirection.PitchUp;
            }

            // 6. Compute Pitch input
            // Use desiredSpin (Se) as the spin basis for deciding pitch direction.
            var spinForPitch = desiredSpin;
            var rollFactor = spinForPitch == BasisSpin.Down ? +1f : -1f;

            if (flyingAway && newLoop != LoopDirection.NoLoop)  // newLoop != null is redundant here, but let it be.
            {
                // Looping: always pitch in loop direction
                inputs.Pitch = newLoop == LoopDirection.PitchUp ? Airplane.PitchInput.Backward : Airplane.PitchInput.Forward;
            }
            else    
            {
                // Not flying away hence not looping: steer shortest arc towards Ve
                var threshold = MathHelper.ToRadians(Airplane.PitchAngle);

                inputs.Pitch =
                    // Ve is to the left of nose → need to pitch "backward" relative to spin
                    diff > threshold ? (rollFactor > 0f ? Airplane.PitchInput.Backward : Airplane.PitchInput.Forward) :
                    // Ve is to the right of nose → need to pitch "forward" relative to spin
                    diff < -threshold ? (rollFactor > 0f ? Airplane.PitchInput.Forward : Airplane.PitchInput.Backward) :
                    // Within PitchAngle -> hold pitch steady (no jitter)
                    Airplane.PitchInput.None;   
            }

            return (newLoop, throttleForCruise(plane, inputs));
        }

        private enum ZoneMatch { BeforeZone, InZone, AfterZone }

        public static (ApproachPhase Phase, Airplane.Inputs Inputs) Transition(this Airplane plane, ApproachPhase phase)
        {
            var inZone = (ApproachZone zone) => 
                zone.EntryX < zone.ExitX
                ? (plane.Position.X < zone.EntryX ? ZoneMatch.BeforeZone :
                    plane.Position.X < zone.ExitX ? ZoneMatch.InZone :
                    ZoneMatch.AfterZone)
                : (plane.Position.X > zone.EntryX ? ZoneMatch.BeforeZone :
                    plane.Position.X > zone.ExitX ? ZoneMatch.InZone :
                    ZoneMatch.AfterZone);

            (ApproachPhase Phase, Airplane.Inputs Inputs) steerToFinal(ApproachPhase.PreFinal x) 
            {
                var (loop, inputs) = plane.SteerTowards(x.Approach, x.Approach.Final, x.Loop);
                return (x with { Loop = loop }, inputs);
            }

            (ApproachPhase Phase, Airplane.Inputs Inputs) steerToPreTouch(ApproachPhase.Final x)
            {
                var (loop, inputs) = plane.SteerTowards(x.Approach, x.Approach.PreTouch, LoopDirection.NoLoop);
                if (Math.Abs(plane.Position.X - x.Approach.PreTouch.EntryX) < x.Approach.LandingSpeedDistance)
                    inputs = brakeForLanding(plane, inputs);
                return (x, inputs);
            }

            var approachFail = () => (ApproachPhase.Fail, Airplane.Inputs.Clean());

            return phase switch
            {
                ApproachPhase.Initial x when x.TargetZone == x.Approach.PreFinal =>
                    inZone(x.TargetZone) switch
                    {
                        ZoneMatch.BeforeZone => (x, plane.SteerTowards(x.Approach, x.Approach.PreFinal, LoopDirection.NoLoop).Inputs),
                        ZoneMatch.InZone or 
                        ZoneMatch.AfterZone => steerToFinal(new ApproachPhase.PreFinal(x.Approach, x.Approach.Final, LoopDirection.NoLoop)),       
                        _ => approachFail(),
                    },
                ApproachPhase.Initial x when x.TargetZone == x.Approach.Final =>
                    inZone(x.TargetZone) switch
                    {
                        ZoneMatch.BeforeZone => steerToFinal(new ApproachPhase.PreFinal(x.Approach, x.Approach.Final, LoopDirection.NoLoop)),
                        ZoneMatch.InZone => steerToPreTouch(new ApproachPhase.Final(x.Approach, x.Approach.PreTouch)),
                        _ => approachFail(),
                    },
                ApproachPhase.PreFinal x =>
                    inZone(x.FinalZone) switch
                    {
                        ZoneMatch.BeforeZone => steerToFinal(x),
                        ZoneMatch.InZone => steerToPreTouch(new ApproachPhase.Final(x.Approach, x.Approach.PreTouch)),
                        _ => approachFail(),
                    },
                ApproachPhase.Final x =>
                    inZone(x.PreTouchZone) switch
                    {
                        ZoneMatch.BeforeZone => steerToPreTouch(x),
                        ZoneMatch.InZone => (new ApproachPhase.PreTouch(x.Approach), Airplane.Inputs.Clean()),
                        _ => approachFail(),
                    },
                ApproachPhase.PreTouch x =>
                    inZone(x.Approach.PreTouch) switch
                    {
                        ZoneMatch.InZone when !plane.Landing => (x, brakeForLanding(plane, Airplane.Inputs.Clean())),
                        ZoneMatch.InZone when plane.Landing => (new ApproachPhase.Touchdown(x.Approach), Airplane.Inputs.Clean()),
                        _ => approachFail(),
                    },
                ApproachPhase.Touchdown x when plane.Speed > 0 => (x, Airplane.Inputs.Clean() with { Throttle = Airplane.ThrottleInput.Reversing }),
                _ => approachFail(),
            };
        }
    }
}
