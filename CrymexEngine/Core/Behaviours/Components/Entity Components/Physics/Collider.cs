using CrymexEngine.Scenes;
using nkast.Aether.Physics2D.Dynamics;
using OpenTK.Mathematics;

namespace CrymexEngine.AetherPhysics
{
    public partial class Collider : EntityComponent
    {
        public Body AetherBody => aetherBody;

        public Vector2 offset = Vector2.Zero;
        public float density = 1;

        protected Body aetherBody;
        protected PhysicsBody? physicsBody;

        protected sealed override void Load()
        {
            Scene.Current.colliders.Add(this);
        }

        public sealed override void PreRender() { }

        protected sealed override void Update() 
        {
        }

        protected BodyType GetBodyType()
        {
            if (physicsBody == null) return BodyType.Static;
            return physicsBody.BodyType;
        }
    }

    public class CircleCollider : Collider
    {
        public float Radius
        {
            get
            {
                return _radius;
            }
            set
            {
                if (!Physics.Active) return;

                _radius = value;

                BodyType bodyType = BodyType.Static;
                if (aetherBody != null) bodyType = aetherBody.BodyType;
                aetherBody = Physics.RegisterBody(this, bodyType, aetherBody);
            }
        }

        private float _radius = float.MinValue;
    }

    public class BoxCollider : Collider
    {
        public Vector2 Size
        {
            get
            {
                return _size;
            }
            set
            {
                if (!Physics.Active) return;

                _size = value;

                BodyType bodyType = BodyType.Static;
                if (aetherBody != null) bodyType = aetherBody.BodyType;
                aetherBody = Physics.RegisterBody(this, bodyType, aetherBody);
            }
        }

        private Vector2 _size;
    }
}
