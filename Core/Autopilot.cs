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

        public record struct Funnel(Vector2 Def, float AngleDeg1, float AngleDeg2)
        {
            public Vector2 Ray1 = Def.Rotate(MathHelper.ToRadians(AngleDeg1));
            public Vector2 Ray2 = Def.Rotate(MathHelper.ToRadians(AngleDeg2));
        }

        public record struct ApproachZone(float EntryX, Cardinal Direction, float Width, float BottomEntryY, float TopEntryY, Funnel Funnel)
        {
            public float ExitX => EntryX + Width * Direction.ToFactor();
        }

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

        public record struct Approach(
            Cardinal Direction,
            Ground.Runway Runway, 
            Approach.Node CounterDirectNode,
            Approach.Node CoDirectNode, 
            ApproachZone PreTouch, 
            float LoopHeightThreshold, 
            float LandingSpeedDistance)
        {
            public sealed record Node(ApproachZone Zone, Node? Next)
            {
                public static Node Of(ApproachZone zone) => new(zone, null);
                public Node WithNext(ApproachZone nextZone) => this with { Next = Next == null ? Of(nextZone) : Next.WithNext(nextZone) };
            }
        }

        public abstract record ApproachPhase()
        {
            public sealed record Failure() : ApproachPhase;
            public sealed record Initial(Approach Approach, Approach.Node TargetNode) : ApproachPhase;
            public sealed record CounterDirect(Approach Approach, Approach.Node CoDirectNode, LoopDirection Loop) : ApproachPhase;
            public sealed record CoDirect(Approach Approach, Approach.Node NextNode) : ApproachPhase;
            public sealed record Final(Approach Approach, ApproachZone PreTouchZone) : ApproachPhase;
            public sealed record PreTouch(Approach Approach) : ApproachPhase;
            public sealed record Touchdown(Approach Approach) : ApproachPhase;

            public static Failure Fail = new Failure();
        }

        private static bool AheadLeft(float zoneX, float planeX) => zoneX <= planeX;
        private static bool AheadRight(float zoneX, float planeX) => zoneX >= planeX;

        private static Approach.Node? PickAheadNode(Approach.Node start, float planePosX, Func<float, float, bool> isAhead)
        {
            for (var node = start; node != null; node = node.Next)
                if (isAhead(node.Zone.EntryX, planePosX))
                    return node;

            return null;
        }

        private static Approach.Node? ChooseInitialZone(Airplane plane, Approach approach)
        {
            var planeDirectionAngle = plane.Direction.ToAngle();
            var flightDirection =
                plane.Direction.X == 0f ? approach.Direction :       // If flying strictly vertically consider it in direction of approach.
                -float.Pi / 2f < planeDirectionAngle && planeDirectionAngle < +float.Pi / 2f ? Cardinal.Right :
                Cardinal.Left;

            var node = (approach.Direction, flightDirection) switch
            {
                (Cardinal.Left,  Cardinal.Left)  => PickAheadNode(approach.CoDirectNode, plane.Position.X, AheadLeft),
                (Cardinal.Left,  Cardinal.Right) => PickAheadNode(approach.CounterDirectNode, plane.Position.X, AheadRight),
                (Cardinal.Right, Cardinal.Right) => PickAheadNode(approach.CoDirectNode, plane.Position.X, AheadRight),
                (Cardinal.Right, Cardinal.Left)  => PickAheadNode(approach.CounterDirectNode, plane.Position.X, AheadLeft),
                _ => throw new ApplicationException("Logic error")
            };

            // If we cannot enter CoDirect zone through prescribed funnel, try next one until we can.
            if (flightDirection == approach.Direction)
                while (node != null && GetProjectedEntry(plane.Position, node.Zone) == null)
                    node = node.Next;
            
            return node;
        }

        /// <summary>
        /// Initiates auto landing.
        /// </summary>
        /// <param name="approaches">Prioritised sequence of approaches to choose from</param>
        public static ApproachPhase InitiateAutoLanding(Airplane plane, IEnumerable<Approach> approaches) =>
            approaches
                .Select(approach => new { approach, node = ChooseInitialZone(plane, approach) })
                .Where(a => a.node != null)
                .Select(a => new ApproachPhase.Initial(a.approach, a.node!) as ApproachPhase)
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
            float tB = dx / zone.Funnel.Ray1.X;
            float tT = dx / zone.Funnel.Ray2.X;

            // Only forward intersections are valid.
            if (tB <= 0f || tT <= 0f)
                return null;

            float Yb = currentPos.Y + tB * zone.Funnel.Ray1.Y;
            float Yt = currentPos.Y + tT * zone.Funnel.Ray2.Y;

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
            plane.Speed <= Airplane.MaxAutoLandingSpeed
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
        public static (LoopDirection ChosenLoop, Airplane.Inputs Inputs) SteerTowards(this Airplane plane, Approach approach, ApproachZone zone, LoopDirection chosenLoop, float ups)
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
                var threshold = MathHelper.ToRadians(Airplane.PitchAngle / ups);

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

        // ----------------------------------

        public static (ApproachPhase Phase, Airplane.Inputs Inputs) Transition(this Airplane plane, ApproachPhase phase, float ups)
        {
            var inZone = (ApproachZone zone) =>
                zone.Direction == Cardinal.Right
                ? (plane.Position.X < zone.EntryX ? ZoneMatch.BeforeZone :
                    plane.Position.X < zone.EntryX + zone.Width ? ZoneMatch.InZone :
                    ZoneMatch.AfterZone)
                : (plane.Position.X > zone.EntryX ? ZoneMatch.BeforeZone :
                    plane.Position.X > zone.EntryX - zone.Width ? ZoneMatch.InZone :
                    ZoneMatch.AfterZone);

            (ApproachPhase Phase, Airplane.Inputs Inputs) steerToCounterTarget(ApproachPhase.CounterDirect x)
            {
                var (loop, inputs) = plane.SteerTowards(x.Approach, x.CoDirectNode.Zone, x.Loop, ups);
                return (x with { Loop = loop }, inputs);
            }

            (ApproachPhase Phase, Airplane.Inputs Inputs) steerToCoTarget(ApproachPhase.CoDirect x)
            {
                var (loop, inputs) = plane.SteerTowards(x.Approach, x.NextNode.Zone, LoopDirection.NoLoop, ups);
                return (x, inputs);
            }

            (ApproachPhase Phase, Airplane.Inputs Inputs) steerToPreTouch(ApproachPhase.Final x)
            {
                var (loop, inputs) = plane.SteerTowards(x.Approach, x.Approach.PreTouch, LoopDirection.NoLoop, ups);
                if (Math.Abs(plane.Position.X - x.Approach.PreTouch.EntryX) < x.Approach.LandingSpeedDistance)
                    inputs = brakeForLanding(plane, inputs);
                return (x, inputs);
            }

            var approachFail = () => (ApproachPhase.Fail, Airplane.Inputs.Clean());
            var pickAheadNode = (Approach a) => PickAheadNode(a.CoDirectNode, plane.Position.X, a.Direction == Cardinal.Left ? AheadLeft : AheadRight)!;


            return phase switch
            {
                ApproachPhase.Initial x when x.TargetNode.Zone.Direction != x.Approach.Direction =>
                    inZone(x.TargetNode.Zone) switch
                    {
                        ZoneMatch.BeforeZone => (x, plane.SteerTowards(x.Approach, x.TargetNode.Zone, LoopDirection.NoLoop, ups).Inputs),
                        ZoneMatch.InZone or
                        ZoneMatch.AfterZone => steerToCounterTarget(new ApproachPhase.CounterDirect(x.Approach, pickAheadNode(x.Approach), LoopDirection.NoLoop)),
                        _ => approachFail(),
                    },
                ApproachPhase.Initial x when x.TargetNode.Zone.Direction == x.Approach.Direction =>
                    inZone(x.TargetNode.Zone) switch
                    {
                        ZoneMatch.BeforeZone => steerToCounterTarget(new ApproachPhase.CounterDirect(x.Approach, x.TargetNode, LoopDirection.NoLoop)),
                        ZoneMatch.InZone => 
                            x.TargetNode.Next == null
                            ? steerToPreTouch(new ApproachPhase.Final(x.Approach, x.Approach.PreTouch))
                            : steerToCoTarget(new ApproachPhase.CoDirect(x.Approach, x.TargetNode.Next)),
                        _ => approachFail(),
                    },
                ApproachPhase.CounterDirect x =>
                    inZone(x.CoDirectNode.Zone) switch
                    {
                        ZoneMatch.BeforeZone => steerToCounterTarget(x),
                        ZoneMatch.InZone or
                        ZoneMatch.AfterZone =>
                            x.CoDirectNode.Next == null
                            ? steerToPreTouch(new ApproachPhase.Final(x.Approach, x.Approach.PreTouch))
                            : steerToCoTarget(new ApproachPhase.CoDirect(x.Approach, x.CoDirectNode.Next)),
                        _ => approachFail(),
                    },
                ApproachPhase.CoDirect x =>
                    inZone(x.NextNode.Zone) switch
                    {
                        ZoneMatch.BeforeZone => steerToCoTarget(x),
                        ZoneMatch.InZone =>
                            x.NextNode.Next == null
                            ? steerToPreTouch(new ApproachPhase.Final(x.Approach, x.Approach.PreTouch))
                            : steerToCoTarget(new ApproachPhase.CoDirect(x.Approach, x.NextNode.Next)),
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
