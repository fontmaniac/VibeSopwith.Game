using Microsoft.Xna.Framework;
using Nage.Strata.Abstractions;
using Nage.Strata.Types;
using nkast.Aether.Physics2D.Dynamics;

namespace VibeSopwith.Game;

public interface IAmBehaving<TState>
{
    // Populate Aether2D Body properties from instance properties for simulation purposes.
    public void PreSimulationPrepare(TState projected, GameTime gameTime);

    // Populate instance properties from Aether2D Body properties computed at simulation step.
    public void PostSimulationUpdate(TState projected, GameTime gameTime);
}

public interface ICanRemoveRigging<TSimWorld>
{
    void RemoveRigging(TSimWorld simWorld);
}

public interface ICanRemoveRigging : ICanRemoveRigging<World>;

public interface IHasParts
{
    IBasis PickPart(object tag);
}

public interface IDescribeMyself { string WhoAmI { get; } }

public record Poppet<T>(Func<T, bool> IsAlive, Action<T> Erase)
{
    public Poppet Embrace(T target) => new Poppet(() => IsAlive(target), () => Erase(target));
}

public record Poppet(Func<bool> IsAlive, Action Erase)
{
    public static Poppet<T> Make<T>(Func<T, bool> isAlive, Action<T> erase) => new Poppet<T>(isAlive, erase);
    public static void Killable<T>(out Poppet<T> popout, Func<T, bool> isAlive, Action<T> erase) => popout = new Poppet<T>(isAlive, erase);
    public static void KillableByEffect<T>(out Poppet<T> popout, Func<T, bool> isAlive) => popout = new Poppet<T>(isAlive, _ => { });
    public static void AlwaysAlive<T>(out Poppet<T> popout) => popout = new Poppet<T>(_ => true, _ => { });
}

public interface IHasPoppet { Poppet Poppet { get; } }

public interface ICanDie<T> : IHasPoppet
{
    Action<T> RefreshRigging { get; }
    Func<GameTime, Vector2, T> ExecuteEffect { get; }
}

public interface ICanDieByBomb<T> : ICanDie<T>;

public interface ICanDieByBullet<T> : ICanDie<T>
{
    Func<bool> HitOnce { get; } // Returns true when target supposed to die.
    Func<object, IBasis> ExplosionBindTarget { get; }
}

public interface ICanDieByPlane<T> : ICanDie<T>;

public record CanDie<T>(Poppet Poppet, Action<T> RefreshRigging, Func<GameTime, Vector2, T> ExecuteEffect)
    : ICanDie<T>;

public record CanDieByBomb<T>(string WhoAmI, Poppet Poppet, Action<T> RefreshRigging, Func<GameTime, Vector2, T> ExecuteEffect) 
    : ICanDieByBomb<T>
    , IDescribeMyself;

public record CanDieByBullet<T>(string WhoAmI, Poppet Poppet, Func<bool> HitOnce, Func<object, IBasis> ExplosionBindTarget, Action<T> RefreshRigging, Func<GameTime, Vector2, T> ExecuteEffect) 
    : ICanDieByBullet<T>
    , IDescribeMyself;

public record CanDieByBombOrBullet<T>(string WhoAmI, Poppet Poppet, Func<bool> HitOnce, Func<object, IBasis> ExplosionBindTarget, Action<T> RefreshRigging, Func<GameTime, Vector2, T> ExecuteEffect)
    : ICanDieByBomb<T>
    , ICanDieByBullet<T>
    , IDescribeMyself;

public record CanDieByPlane<T>(string WhoAmI, Poppet Poppet, Action<T> RefreshRigging, Func<GameTime, Vector2, T> ExecuteEffect)
    : ICanDieByPlane<T>
    , IDescribeMyself;

public record CanDieByProjectile<T>(string WhoAmI, Poppet Poppet, Func<bool> HitOnce, Func<object, IBasis> ExplosionBindTarget, Action<T> RefreshRigging, Func<GameTime, Vector2, T> ExecuteEffect) 
    : ICanDieByBullet<T>
    , ICanDieByBomb<T>
    , ICanDieByPlane<T>
    , IDescribeMyself;

public static class Caps
{
    public static Func<bool> AbsorbHits(int maxHits)
    {
        var hits = 0;
        return () => (++hits > maxHits);
    }

    public static Func<bool> ImperviousToHits() => () => false;

    public static Action<Unit> RemoveRigging(ICanRemoveRigging canRemove, World simWorld) => (_) => canRemove.RemoveRigging(simWorld);

    public static Func<GameTime, Vector2, Unit> ExecuteEffect(Action<GameTime, Vector2> doExecute) => (gt, ct) => { doExecute(gt, ct); return Unit.Value; };
    public static Func<GameTime, Vector2, Unit> NoEffect() => (_, _) => default;

    public static Action DoAbsolutelyNothing() => () => { };
    public static Action<T> DoNothing<T>() => (T _) => { };

    public static ICanDie<Unit> JustDie<T>(Poppet<T> poppet, T target, World simWorld, Func<GameTime, Vector2, Unit> effect) where T : ICanRemoveRigging =>
        new CanDie<Unit>(poppet.Embrace(target), Caps.RemoveRigging(target, simWorld), effect);

    public static Func<TCap, bool> CheckAlive<TCap>() where TCap : IHasPoppet => (TCap target) => target.Poppet.IsAlive();

    public static Func<object, IBasis> BindToWorld() => (_) => Basis.Canonical;

    public static Func<object, IBasis> BindTargetSelf<T>(T self) where T : IBasis => (_) => self;
    public static Func<object, IBasis> BindTargetPart<T>(T self) =>
        self switch
        {
            IHasParts parts => tag => parts.PickPart(tag),
            IBasis basis => _ => basis,
            _ => throw new NotSupportedException(),
        };

}

public abstract record DeriveStateOutcome<TState>
{
    public sealed record Proceed(TState Projected) : DeriveStateOutcome<TState>;
    public sealed record Hold() : DeriveStateOutcome<TState>;
    public sealed record Rebirth() : DeriveStateOutcome<TState>;
}

public abstract class DSO
{
    public static DeriveStateOutcome<TState>.Proceed ProceeedWith<TState>(TState projected) => new DeriveStateOutcome<TState>.Proceed(projected);
    public static DeriveStateOutcome<Unit>.Proceed DoNothing<TCtx>(TCtx ctx) => new DeriveStateOutcome<Unit>.Proceed(Unit.Value);
}
