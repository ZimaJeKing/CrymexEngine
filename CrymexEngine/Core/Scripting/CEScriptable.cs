using OpenTK.Mathematics;

namespace CrymexEngine
{
    public class CEScriptable
    {
        public virtual void Update() { }
        public virtual void Load() { }
        public virtual void TickLoop() { }
        public virtual void Render() { }

        protected static float Sin(float x) { return MathF.Sin(MathHelper.DegreesToRadians(x)); }
        protected static float Cos(float y) { return MathF.Cos(MathHelper.DegreesToRadians(y)); }
        protected static float Floor(float x) { return MathF.Floor(x); }
        protected static int iFloor(float x) { return (int)MathF.Floor(x); }
        protected static float Ceil(float x) { return MathF.Ceiling(x); }
        protected static int iCeil(float x) { return (int)MathF.Ceiling(x); }
        protected static float Pow(float x, float power) { return MathF.Pow(x, power); }
        protected static float Sqrt(float x) { return MathF.Sqrt(x); }
        protected static float Abs(float x) { return MathF.Abs(x); }
    }
}
