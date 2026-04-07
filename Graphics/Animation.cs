using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeSopwith.Game.Graphics
{
    internal static class Animation
    {
        public interface IDynamicPhase<TCtx> where TCtx : IHasLocation
        {
            HandedSlice GetSlice(TCtx ctx);
        }

        public interface IStaticPhase<TCtx> : IDynamicPhase<TCtx> where TCtx : IHasLocation
        {
            TimeSpan GetDuration(TCtx ctx);
        }

        public abstract record AnimationStatus
        {
            private AnimationStatus() {}

            public sealed record NotStarted() : AnimationStatus;
            public sealed record Ongoing(int Phase) : AnimationStatus;
            public sealed record Completed() : AnimationStatus;
        }

        public interface IStaticSequence<TCtx> where TCtx : IHasLocation
        {
            TimeSpan StartTime { get; }
            IStaticPhase<TCtx>[] Phases { get; }
            bool IsInfiniteLoop { get; }
        }

        public interface IDynamicSequence<TCtx> where TCtx : IHasLocation
        {
            IDynamicPhase<TCtx> GetPhase(TCtx ctx, GameTime gameTime);
        }


        public record StaticSequence<TCtx>(TimeSpan StartTime, IStaticPhase<TCtx>[] Phases, bool IsInfiniteLoop) : IStaticSequence<TCtx> where TCtx : IHasLocation;

        public static StaticSequence<TCtx> Make<TCtx>(TimeSpan startTime, IStaticPhase<TCtx>[] phases, bool isInfiniteLoop) where TCtx : IHasLocation =>
            new StaticSequence<TCtx>(startTime, phases, isInfiniteLoop);

        public static AnimationStatus Draw<TCtx>(TCtx ctx, IStaticSequence<TCtx> sequence, GameTime gameTime, SpriteBatch spriteBatch) where TCtx : IHasLocation
        {
            var startTicks = sequence.StartTime.Ticks;
            var currentTicks = gameTime.TotalGameTime.Ticks;

            if (currentTicks < startTicks)
                return new AnimationStatus.NotStarted();

            // Compute local time inside the sequence
            long localTicks = currentTicks - startTicks;

            // Will repeat at least Once and at most Twice, effectively calculating total sequence length at first pass.
            while (true)
            {
                var accumulatedTicks = 0L;
                for (int i = 0; i < sequence.Phases.Length; i++)
                {
                    var phase = sequence.Phases[i];
                    accumulatedTicks += phase.GetDuration(ctx).Ticks;

                    if (localTicks < accumulatedTicks)
                    {
                        DrawHelper.DrawSlice(ctx, phase.GetSlice(ctx), spriteBatch, null);
                        return new AnimationStatus.Ongoing(i);
                    }
                }

                if (!sequence.IsInfiniteLoop)
                    break;

                localTicks %= accumulatedTicks; // accumulatedTicks is total sequence length here.
            }

            return new AnimationStatus.Completed();
        }
    }
}
