using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
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

        //public record struct ApproachZone(Vector2 Center, Vector2 Direction, float Width, float Height, float MaxAngleDeviation);

        public record struct ApproachZone(float EntryX, float ExitX, float BottomEntryY, float TopEntryY, Vector2 FunnelBottom, Vector2 FunnelTop);

        public enum LoopDirection { PitchUp, PitchDown }

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
            public sealed record PreFinal(Approach Approach, ApproachZone FinalZone) : ApproachPhase;
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
        /// <param name="plane"></param>
        /// <param name="approaches">Prioritised sequence of approaches to choose from</param>
        /// <returns></returns>
        public static ApproachPhase InitiateAutoLanding(Airplane plane, IEnumerable<Approach> approaches) => 
            approaches
                .Select(approach => ChooseInitialZone(plane, approach))
                .Where(a => a != null)
                .Select(a => new ApproachPhase.Initial(a!.Value.approach, a!.Value.zone) as ApproachPhase)
                .Append(ApproachPhase.Fail)
                .First();
    }
}
