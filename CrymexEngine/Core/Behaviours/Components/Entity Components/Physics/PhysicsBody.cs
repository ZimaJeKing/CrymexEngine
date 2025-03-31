using nkast.Aether.Physics2D.Dynamics;
using OpenTK.Mathematics;

namespace CrymexEngine.AetherPhysics
{
    [SingularComponent]
    public class PhysicsBody : EntityComponent
    {
        public Collider Collider => _collider;

        public BodyType BodyType
        {
            get
            {
                return _bodyType;
            }
            set
            {
                if (!Physics.Active) return;

                if (!_hasCollider) return;
                _bodyType = value;
                _aetherBody = Physics.RegisterBody(_collider, _bodyType, _aetherBody);
            }
        }

        public float Mass
        {
            get
            {
                return _mass;
            }
            set
            {
                if (!Physics.Active) return;

                if (!_hasCollider) return;
                _mass = value;
                _aetherBody.Mass = value;
            }
        }

        private Collider _collider;
        private Body _aetherBody;
        private float _mass = 1;
        private BodyType _bodyType = BodyType.Static;
        private bool _hasCollider = false;

        /// <summary>
        /// Overrides the base physics collider
        /// </summary>
        public void OverrideCollider(Collider collider)
        {
            if (_collider == null || collider.entity != entity) return;
            _collider = collider;
            _aetherBody = collider.AetherBody;
            _hasCollider = true;
        }

        public void AddForceImpulse(Vector2 force)
        {
            if (!_hasCollider || !Physics.Active) return;

            _aetherBody.ApplyLinearImpulse(Physics.OpenTKToAether(force));
        }

        public void AddForce(Vector2 force)
        {
            if (!_hasCollider || !Physics.Active) return;

            _aetherBody.ApplyForce(Physics.OpenTKToAether(force));
        }

        protected override void Load() 
        { 
            GetActiveCollider();
        }
        protected override void Update() 
        {
            if (_aetherBody == null) return;
            entity.Position = Physics.AetherToOpenTK(_aetherBody.Position);
            entity.Rotation = MathHelper.RadiansToDegrees(_aetherBody.Rotation);
        }

        public override void PreRender() { }

        private void GetActiveCollider()
        {
            BoxCollider boxCollider = entity.GetComponent<BoxCollider>();
            CircleCollider circleCollider = entity.GetComponent<CircleCollider>();

            if (circleCollider != null)
            {
                _collider = circleCollider;
                _mass = MathF.Pow(circleCollider.Radius, 2) * MathF.PI * circleCollider.density;
            }
            else if (boxCollider != null)
            {
                _collider = boxCollider;
                _mass = boxCollider.Size.X * boxCollider.Size.Y * boxCollider.density;
            }

            if (_collider != null)
            {
                _aetherBody = _collider.AetherBody;
                _hasCollider = true;
            }
            else
            {
                Debug.LogWarning($"Entity '{entity.name}' is missing a collider. All entities should have colliders when adding a physics body.");
                enabled = false;
            }
        }
    }
}
