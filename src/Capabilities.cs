using Microsoft.Xna.Framework;
using Nage.Strata.Abstractions.Behavioral;
using Nage.Strata.Abstractions.Spatial;
using Nage.Strata.Types;
using nkast.Aether.Physics2D.Dynamics;

namespace VibeSopwith.Game;

public interface ICanRemoveRigging : ICanRemoveRigging<World>;

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
    public static bool Remove(bool isExpired, Func<bool> remove) => isExpired ? remove() : false;
    public static bool RemoveWithRigging(ICanRemoveRigging<World> canRemove, World simWorld, bool isExpired, Func<bool> remove)
    {
        if (!isExpired) return false; 
        canRemove.RemoveRigging(simWorld);
        return remove();
    }

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

