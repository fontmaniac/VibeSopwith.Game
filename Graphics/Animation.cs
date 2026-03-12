using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeSopwith.Game.Graphics
{
    internal static class Animation
    {
        public interface IPhase<TCtx>
        {
            TimeSpan GetDuration(TCtx ctx);
            void Draw(SpriteBatch sb, TCtx ctx);
        }

        public abstract record AnimationStatus
        {
            private AnimationStatus() {}

            public sealed record NotStarted() : AnimationStatus;
            public sealed record Ongoing(int Phase) : AnimationStatus;
            public sealed record Completed() : AnimationStatus;
        }

        public static AnimationStatus Draw<TCtx>(TCtx ctx, TimeSpan startTime, IPhase<TCtx>[] phases, bool infiniteLoop, GameTime gameTime, SpriteBatch spriteBatch)
        {
            var animationTicks = startTime.Ticks;
            var currentTicks = gameTime.TotalGameTime.Ticks;

            if (currentTicks < animationTicks)
                return new AnimationStatus.NotStarted();

            // Draw phase instance corresponding to the gameTime.
            for (var i = 0; i < phases.Length; ++i) 
            { 
                var phase = phases[i];
                animationTicks += phase.GetDuration(ctx).Ticks;
                if (animationTicks >= currentTicks)
                {
                    phase.Draw(spriteBatch, ctx);
                    return new AnimationStatus.Ongoing(i);
                }
            }

            if (infiniteLoop)
            {
                var sequenceLength = animationTicks - startTime.Ticks;
                var timeSinceStart = gameTime.TotalGameTime.Ticks - startTime.Ticks;
                var wholeCyclesPassed = timeSinceStart / sequenceLength;
                var newStart = startTime.Add(TimeSpan.FromTicks(wholeCyclesPassed * sequenceLength));

                return Draw(ctx, newStart, phases, false, gameTime, spriteBatch);
            }

            return new AnimationStatus.Completed();
        }
    }
}
