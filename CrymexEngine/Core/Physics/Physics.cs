using CrymexEngine.Utils;
using nkast.Aether.Physics2D.Dynamics;
using OpenTK.Mathematics;

namespace CrymexEngine
{
    public static class Physics
    {
        public static bool Active => _initialized;
        public static float PhysicsLoopDeltaTime => _physicsDeltaTime;

        public static Vector2 Gravity => _gravity;

        private static World _aetherWorld;
        private static Vector2 _gravity;

        private static float _updatePeriod = -1;
        private static bool _usePhysics = false;
        private static bool _initialized = false;
        private static float _lastFrameTime;
        private static float _physicsDeltaTime;

        internal static void Init()
        {
            if (Window.Loaded || _initialized) return;

            // Determine whether to use physics and return if no
            if (Settings.GlobalSettings.GetSetting("UsePhysics", out SettingOption physicsOption, SettingType.Bool))
            {
                _usePhysics = physicsOption.GetValue<bool>();
            }
            if (!_usePhysics) return;

            // Get the physics update frequency and add the event
            if (Settings.GlobalSettings.GetSetting("PhysicsUpdateFrequency", out SettingOption updateFreqOption, SettingType.Float))
            {
                // Clamp the frequency between a low number and 144 for it is not usefull above that
                _updatePeriod = 1f / Math.Clamp(updateFreqOption.GetValue<float>(), 0.001f, 144);
                Debug.LogLocalInfo("Physics Solver", $"Physics frame update every {DataUtil.FloatToShortString(_updatePeriod * 1000, 2)} ms");
            }
            if (_updatePeriod == -1)
            {
                _usePhysics = false;
                Debug.LogWarning("No physics update frequency setting found. Physics solver will remain inactive");
                return;
            }

            EventSystem.AddEventRepeat("CE_PhysicsLoop", Update, _updatePeriod, true);

            // Reading the gravity setting
            if (Settings.GlobalSettings.GetSetting("Gravity", out SettingOption gravityOption, SettingType.Vector2))
            {
                _gravity = gravityOption.GetValue<Vector2>();
            }

            _aetherWorld = new World(OpenTKToAether(_gravity));

            _initialized = true;

            Debug.LogLocalInfo("Physics Solver", "Physics solver active");
        }

        private static void Update()
        {
            if (!_initialized) return;

            _physicsDeltaTime = Time.GameTime - _lastFrameTime;
            _lastFrameTime = Time.GameTime;

            _aetherWorld.Step(_physicsDeltaTime);
        }

        internal static Body RegisterBody(Collider collider, BodyType bodyType, Body? reference = null)
        {
            Body physicsBody;
            if (reference != null)
            {
                physicsBody = reference;
                physicsBody.BodyType = bodyType;
                foreach (var fixture in physicsBody.FixtureList.ToArray())
                {
                    physicsBody.Remove(fixture);
                }
            }
            else physicsBody = _aetherWorld.CreateBody(OpenTKToAether(collider.entity.Position), collider.entity.Rotation, bodyType);

            if (collider is BoxCollider box)
            {
                physicsBody.CreateRectangle(box.Size.X, box.Size.Y, box.density, OpenTKToAether(box.offset));
            }
            else if (collider is CircleCollider circle)
            {
                physicsBody.CreateCircle(circle.Radius, circle.density, OpenTKToAether(circle.offset));
            }
            else throw new Exception($"{collider.GetType()} is not a supported collider type");

            return physicsBody;
        }

        public static nkast.Aether.Physics2D.Common.Vector2 OpenTKToAether(Vector2 vector)
        {
            return new nkast.Aether.Physics2D.Common.Vector2(vector.X, vector.Y);
        }
        public static Vector2 AetherToOpenTK(nkast.Aether.Physics2D.Common.Vector2 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
    }
}
