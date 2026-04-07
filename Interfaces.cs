using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Utils;

namespace VibeSopwith.Game;

public enum Cardinal { Left, Right }

public static class CardinalModule
{
    public static float ToFactor(this Cardinal c) => c == Cardinal.Left ? -1f : +1f;
    public static Cardinal Toggle(this Cardinal c) => c == Cardinal.Left ? Cardinal.Right : Cardinal.Left;
}

public enum BasisSpin { Down, Up }

public static class SpinModule
{
    public static BasisSpin Toggle(this BasisSpin s) => s == BasisSpin.Down ? BasisSpin.Up : BasisSpin.Down;
    public static float ToFactor(this BasisSpin c) => c == BasisSpin.Down ? +1f : -1f;
}


public interface IBasis
{
    Vector2 Position { get; }   // In world
    Vector2 Direction { get; }
    BasisSpin Spin { get; }
}

public record struct Basis(Vector2 Position, Vector2 Direction, BasisSpin Spin) : IBasis
{
    public static IBasis Canonical = new Basis(Vector2.Zero, Vector2.UnitX, BasisSpin.Down);
    public static IBasis Default = Canonical;
    public static IBasis FixedPos(Vector2 position) => new Basis(position, Vector2.UnitX, BasisSpin.Down);
}

public record struct LiveBasis(Func<Vector2> getPosition, Func<Vector2> getDirection, Func<BasisSpin> getSpin) : IBasis
{
    public Vector2 Position { get => getPosition(); }   
    public Vector2 Direction { get => getDirection(); }
    public BasisSpin Spin { get => getSpin(); }

    public static IBasis FixedPos(Func<Vector2> getPosition) => new LiveBasis(getPosition, () => Basis.Canonical.Direction, () => Basis.Canonical.Spin);
    public static IBasis Bind(IBasis bindWhat, IBasis bindTo)
    {
        var capturedSpin = bindTo.Spin;
        var parentAngle = bindTo.Direction.ToAngle();

        // Compute local offset
        var dPosWorld = bindWhat.Position - bindTo.Position;
        var dPosLocal = dPosWorld.Rotate(-parentAngle);

        // Compute local rotation
        var dAngleLocal = bindWhat.Direction.ToAngle() - parentAngle;

        IBasis makeBasis()
        {
            var posLocal = bindTo.Spin == capturedSpin ? dPosLocal : dPosLocal with { Y = -dPosLocal.Y};
            var angleLocal = bindTo.Spin == capturedSpin ? dAngleLocal : -dAngleLocal;
            return new Basis(
                bindTo.Position + posLocal.Rotate(bindTo.Direction.ToAngle()),
                bindTo.Direction.Rotate(angleLocal),
                bindTo.Spin);
        }

        return new LiveBasis(
            () => makeBasis().Position,
            () => makeBasis().Direction,
            () => makeBasis().Spin
        );
    }
}

public interface IHasLocation : IBasis
{
    float Length { get; }       // Measurement along X-axis 
    float Height { get; }       // Measurement along Y-axis 
}

public record Location(Vector2 Position, Vector2 Direction, BasisSpin Spin, float Length, float Height) : IHasLocation
{
    public static Location OffInterface(IHasLocation src) => new Location(
        src.Position,
        src.Direction,
        src.Spin,
        src.Length,
        src.Height);
}


public interface IAmBehaving<TState>
{
    // Populate Aether2D Body properties from instance properties for simulation purposes.
    public void PreSimulationPrepare(TState projected);

    // Populate instance properties from Aether2D Body properties computed at simulation step.
    public void PostSimulationUpdate(TState projected);
}

public interface ICanRemoveRigging
{
    void RemoveRigging(World collisionWorld);
}

public interface IHasParts
{
    IBasis PickPart(object tag);
}

public interface IDescribeMyself { string WhoAmI { get; } }

public record Poppet<T>(Func<T, bool> IsAlive, Action<T> Kill)
{
    public Poppet Embrace(T target) => new Poppet(() => IsAlive(target), () => Kill(target));
}

public record Poppet(Func<bool> IsAlive, Action Kill)
{
    public static Poppet<T> Make<T>(Func<T, bool> isAlive, Action<T> kill) => new Poppet<T>(isAlive, kill);
    public static void Make<T>(out Poppet<T> popout, Func<T, bool> isAlive, Action<T> kill) => popout = new Poppet<T>(isAlive, kill);
    public static void Make<T>(out Poppet<T> popout) => popout = new Poppet<T>(_ => true, _ => { });
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

    public static Action<Unit> RemoveRigging(ICanRemoveRigging canRemove, World collisionWorld) => (_) => canRemove.RemoveRigging(collisionWorld);

    public static Func<GameTime, Vector2, Unit> ExecuteEffect(Action<GameTime, Vector2> doExecute) => (gt, ct) => { doExecute(gt, ct); return Unit.Value; };
    public static Func<GameTime, Vector2, Unit> NoEffect() => (_, _) => default;

    public static Action<Unit> DoNothing() => _ => { };

    public static ICanDie<Unit> JustDie<T>(Poppet<T> poppet, T target, World collisionWorld, Func<GameTime, Vector2, Unit> effect) where T : ICanRemoveRigging =>
        new CanDie<Unit>(poppet.Embrace(target), Caps.RemoveRigging(target, collisionWorld), effect);

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

