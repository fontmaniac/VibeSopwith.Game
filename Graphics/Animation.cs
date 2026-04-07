using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeSopwith.Game.Graphics
{
    internal static class Animation
    {
        public interface IPhase<TCtx> where TCtx : IHasLocation
        {
            TimeSpan GetDuration(TCtx ctx);
            HandedSlice GetSlice(TCtx ctx);
        }

        public abstract record AnimationStatus
        {
            private AnimationStatus() {}

            public sealed record NotStarted() : AnimationStatus;
            public sealed record Ongoing(int Phase) : AnimationStatus;
            public sealed record Completed() : AnimationStatus;
        }

        public interface ISequence<TCtx> where TCtx : IHasLocation
        {
            TimeSpan StartTime { get; }
            IPhase<TCtx>[] Phases { get; }
            bool IsInfiniteLoop { get; }
        }

        public record Sequence<TCtx>(TimeSpan StartTime, IPhase<TCtx>[] Phases, bool IsInfiniteLoop) : ISequence<TCtx> where TCtx : IHasLocation;

        public static Sequence<TCtx> Make<TCtx>(TimeSpan startTime, IPhase<TCtx>[] phases, bool isInfiniteLoop) where TCtx : IHasLocation =>
            new Sequence<TCtx>(startTime, phases, isInfiniteLoop);

        public static AnimationStatus Draw<TCtx>(TCtx ctx, ISequence<TCtx> sequence, GameTime gameTime, SpriteBatch spriteBatch) where TCtx : IHasLocation 
        {
            var animationTicks = sequence.StartTime.Ticks;
            var currentTicks = gameTime.TotalGameTime.Ticks;

            if (currentTicks < animationTicks)
                return new AnimationStatus.NotStarted();

            // Draw phase instance corresponding to the gameTime.
            for (var i = 0; i < sequence.Phases.Length; ++i) 
            { 
                var phase = sequence.Phases[i];
                animationTicks += phase.GetDuration(ctx).Ticks;
                if (animationTicks >= currentTicks)
                {
                    DrawHelper.DrawSlice(ctx, phase.GetSlice(ctx), spriteBatch, null);
                    return new AnimationStatus.Ongoing(i);
                }
            }

            if (sequence.IsInfiniteLoop)
            {
                var sequenceLength = animationTicks - sequence.StartTime.Ticks;
                var timeSinceStart = gameTime.TotalGameTime.Ticks - sequence.StartTime.Ticks;
                var wholeCyclesPassed = timeSinceStart / sequenceLength;
                var newStart = sequence.StartTime.Add(TimeSpan.FromTicks(wholeCyclesPassed * sequenceLength));

                return Draw(ctx, new Sequence<TCtx>(newStart, sequence.Phases, false), gameTime, spriteBatch);
            }

            return new AnimationStatus.Completed();
        }
    }
}
