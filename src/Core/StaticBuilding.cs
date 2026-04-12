using Microsoft.Xna.Framework;
using Nage.Strata.Abstractions.Spatial;
using Nage.Strata.Physics;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Utils;

namespace VibeSopwith.Game.Core
{
    internal class StaticBuilding : IHasLocation, ICanRemoveRigging
    {
        public Body Body = null!;

        public Vector2 Position { get; }
        public Vector2 Direction { get; }
        public BasisSpin Spin { get; }
        public float Length => 4f;
        public float Height => 4f;

        public enum BuildingType { Factory, Cistern, ArmyBase };
        public BuildingType TheType { get; }

        public bool Exploded = false;

        private float FlipFactor { get => Spin == BasisSpin.Down ? +1f : -1f; }

        public StaticBuilding(BuildingType buildingType, Vector2 position, BasisSpin spin)
        {
            Position = position;
            TheType = buildingType;
            Spin = spin;
            Direction = Vector2.UnitX * (spin == BasisSpin.Down ? +1f : -1f);
        }

        public void RemoveRigging(World simWorld)
        {
            simWorld.Remove(Body); 
            Body = null!;
        }

        private Body SetupRigging_Cistern(World simWorld)
        {
            var body = simWorld.CreateBody(Position.ToAether(), 0f, BodyType.Static);
            body.Rotation = Direction.ToAngle();
            body.Tag = this;
            body.FixedRotation = true;
            body.Mass = 500f;
            body.Inertia = 0f;
            body.LinearDamping = 0.0f;
            body.AngularDamping = 0.0f;

            var ff = FlipFactor;

            // Add fixture 0
            var vertices0 = new[]
            {
                (-1.9219f, ff * 0f),
                (-1.9063f, ff * 2.5508f),
                (-1.5742f, ff * 3.375f),
                (-0.6484f, ff * 3.7188f),
                (0.7969f, ff * 3.7227f),
                (1.832f, ff * 2.7773f),
                (1.9336f, ff * 2.1602f),
                (1.9297f, ff * 0f)
            };

            var fixture0 = vertices0.ToPolygon(body);
            fixture0.Friction = 0.0f;
            fixture0.Restitution = 0.0f;

            return body;
        }

        private Body SetupRigging_Factory(World simWorld)
        {
            var body = simWorld.CreateBody(Position.ToAether(), 0f, BodyType.Static);
            body.Rotation = Direction.ToAngle();
            body.Tag = this;
            body.FixedRotation = true;
            body.Mass = 500f;
            body.Inertia = 0f;
            body.LinearDamping = 0.0f;
            body.AngularDamping = 0.0f;

            var ff = FlipFactor;

            // Add fixture 0
            var vertices0 = new[]
            {
                (-1.8438f, ff * -0.0078f),
                (-1.8516f, ff * 2.1055f),
                (-0.7109f, ff * 3.8594f),
                (1.8398f, ff * 3.8555f),
                (1.8555f, ff * -0.0078f)
            };

            var fixture0 = vertices0.ToPolygon(body);
            fixture0.Friction = 0.0f;
            fixture0.Restitution = 0.0f;

            return body;
        }

        public Body SetupRigging_ArmyBase(World simWorld)
        {
            var body = simWorld.CreateBody(Position.ToAether(), 0f, BodyType.Static);
            body.Rotation = Direction.ToAngle();
            body.Tag = this;
            body.FixedRotation = true;
            body.Mass = 500f;
            body.Inertia = 0f;
            body.LinearDamping = 0.0f;
            body.AngularDamping = 0.0f;

            var ff = FlipFactor;

            // Add fixture 0
            var vertices0 = new[]
            {
                (-1.9961f, ff * 0.0039f),
                (-2f, ff * 2.8633f),
                (0.0508f, ff * 2.8867f),
                (0.0273f, ff * 1.5977f),
                (1.8086f, ff * 1.6094f),
                (1.8594f, ff * 0f)
            };

            var fixture0 = vertices0.ToPolygon(body);
            fixture0.Friction = 0.0f;
            fixture0.Restitution = 0.0f;

            return body;
        }

        public void SetupRigging(World simWorld, Func<object>? makeTag = null)
        {
            Body =
                TheType == BuildingType.Factory  ? SetupRigging_Factory(simWorld) :
                TheType == BuildingType.Cistern  ? SetupRigging_Cistern(simWorld) :
                TheType == BuildingType.ArmyBase ? SetupRigging_ArmyBase(simWorld) :
                throw new ApplicationException("Unsupported building type");

            makeTag = makeTag ?? (() => this);
            Body.Tag = makeTag();
        }

    }
}
