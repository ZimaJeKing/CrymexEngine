using CrymexEngine.UI;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CrymexEngine
{
    public static class Input
    {
        public static string textInput = "";

        public static Vector2 MousePosition
        {
            get
            {
                Vector2 pos = Window.Instance.GLFWWindow.MousePosition - Window.HalfSize;
                pos.Y = -pos.Y;
                return pos;
            }
        }

        public static Vector2 MouseScrollDelta
        {
            get
            {
                return Window.Instance.GLFWWindow.MouseState.ScrollDelta;
            }
        }

        public static bool Key(Key key)
        {
            return Window.Instance.GLFWWindow.IsKeyDown((Keys)key);
        }
        public static bool KeyDown(Key key)
        {
            return Window.Instance.GLFWWindow.IsKeyPressed((Keys)key);
        }
        public static bool KeyUp(Key key)
        {
            return Window.Instance.GLFWWindow.IsKeyReleased((Keys)key);
        }

        public static bool Mouse(MouseButton button)
        {
            return Window.Instance.GLFWWindow.MouseState.IsButtonDown((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)button);
        }
        public static bool MouseDown(MouseButton button)
        {
            return Window.Instance.GLFWWindow.MouseState.IsButtonPressed((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)button);
        }
        public static bool MouseUp(MouseButton button)
        {
            return Window.Instance.GLFWWindow.MouseState.IsButtonReleased((OpenTK.Windowing.GraphicsLibraryFramework.MouseButton)button);
        }

        public static bool CursorOverlap(Vector2 position, Vector2 scale, float rotation = 0)
        {
            position *= 0.5f;

            Vector2 point = Camera.ScreenSpaceToWorldSpace(MousePosition);

            float translatedX = point.X - position.X;
            float translatedY = position.Y - point.Y;

            float radians = MathHelper.DegreesToRadians(-rotation);
            float rotatedX = MathF.Cos(radians) * translatedX - MathF.Sin(radians) * translatedY;
            float rotatedY = MathF.Sin(radians) * translatedX + MathF.Cos(radians) * translatedY;

            Vector2 halfScale = scale * 0.5f;
            return rotatedX >= -halfScale.X && rotatedX <= halfScale.X && rotatedY >= -halfScale.Y && rotatedY <= halfScale.Y;
        }

        public static bool CursorOverlap(Entity entity)
        {
            if (!entity.enabled || entity.Renderer == null || !entity.interactible) return false;

            Vector2 point = Camera.ScreenSpaceToWorldSpace(MousePosition);

            float translatedX = point.X - entity.Position.X * 0.5f;
            float translatedY = entity.Position.Y * 0.5f - point.Y;

            if (translatedX * translatedX + translatedY * translatedY > entity.Scale.LengthSquared) return false;

            float radians = MathHelper.DegreesToRadians(-entity.Rotation);
            float rotatedX = MathF.Cos(radians) * translatedX - MathF.Sin(radians) * translatedY;
            float rotatedY = MathF.Sin(radians) * translatedX + MathF.Cos(radians) * translatedY;

            // Alpha testing
            if (rotatedX >= -entity.HalfScale.X && rotatedX <= entity.HalfScale.X && rotatedY >= -entity.HalfScale.Y && rotatedY <= entity.HalfScale.Y)
            {
                int texX = (int)(((rotatedX + entity.HalfScale.X) / entity.Scale.X) * entity.Renderer.texture.width);
                int texY = (int)(((rotatedY + entity.HalfScale.Y) / entity.Scale.Y) * entity.Renderer.texture.height);
                if (entity.Renderer.texture.GetPixel(texX, entity.Renderer.texture.height - texY - 1).A != 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CursorOverlap(UIElement element)
        {
            if (!element.enabled || !element.interactible) return false;

            float translatedX = MousePosition.X - element.Position.X * 0.5f;
            float translatedY = element.Position.Y * 0.5f - MousePosition.Y;

            if (translatedX * translatedX + translatedY * translatedY > element.Scale.LengthSquared) return false;

            float radians = MathHelper.DegreesToRadians(-element.Rotation);
            float rotatedX = MathF.Cos(radians) * translatedX - MathF.Sin(radians) * translatedY;
            float rotatedY = MathF.Sin(radians) * translatedX + MathF.Cos(radians) * translatedY;

            // Alpha testing
            if (rotatedX >= -element.HalfScale.X && rotatedX <= element.HalfScale.X && rotatedY >= -element.HalfScale.Y && rotatedY <= element.HalfScale.Y)
            {
                int texX = (int)(((rotatedX + element.HalfScale.X) / element.Scale.X) * element.Renderer.texture.width);
                int texY = (int)(((rotatedY + element.HalfScale.Y) / element.Scale.Y) * element.Renderer.texture.height);
                if (element.Renderer.texture.GetPixel(texX, element.Renderer.texture.height - texY - 1).A != 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CursorOverlap(TextObject text)
        {
            if (!text.enabled) return false;

            Vector2 point = Camera.ScreenSpaceToWorldSpace(MousePosition);

            float translatedX = point.X - text.position.X;
            float translatedY = text.position.Y - point.Y;

            if (translatedX * translatedX + translatedY * translatedY > (text.Scale.X * text.Scale.X + text.Scale.Y * text.Scale.Y)) return false;

            // Alpha testing
            if (translatedX >= -text.HalfScale.X && translatedX <= text.HalfScale.X && translatedY >= -text.HalfScale.Y && translatedY <= text.HalfScale.Y)
            {
                int texX = (int)(((translatedX + text.HalfScale.X) / text.Scale.X) * text.InternalTexture.width);
                int texY = (int)(((translatedY + text.HalfScale.Y) / text.Scale.Y) * text.InternalTexture.height);
                if (text.InternalTexture.GetPixel(texX, texY).A != 0)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public struct Axis
    {
        public readonly Key positive;
        public readonly Key negative;

        public static readonly Axis WS = new Axis(Key.W, Key.S);
        public static readonly Axis AD = new Axis(Key.D, Key.A);
        public static readonly Axis ArrowsLR = new Axis(Key.Right, Key.Left);
        public static readonly Axis ArrowsUD = new Axis(Key.Up, Key.Down);

        public Axis(Key positive, Key negative)
        {
            this.positive = positive;
            this.negative = negative;
        }

        public float GetValue()
        {
            float val = 0;
            if (Input.Key(positive)) val += 1;
            if (Input.Key(negative)) val -= 1;
            return val;
        }
    }
    public struct Axis2D
    {
        public readonly Key positiveX;
        public readonly Key negativeX;
        public readonly Key positiveY;
        public readonly Key negativeY;

        public static readonly Axis2D WSAD = new Axis2D(Key.D, Key.A, Key.W, Key.S);
        public static readonly Axis2D Arrows = new Axis2D(Key.Right, Key.Left, Key.Up, Key.Down);

        public Axis2D(Key positiveX, Key negativeX, Key positiveY, Key negativeY)
        {
            this.positiveX = positiveX;
            this.negativeX = negativeX;
            this.positiveY = positiveY;
            this.negativeY = negativeY;
        }

        public Vector2 GetValue()
        {
            Vector2 val = Vector2.Zero;
            if (Input.Key(positiveX)) val.X += 1;
            if (Input.Key(negativeX)) val.X -= 1;
            if (Input.Key(positiveY)) val.Y += 1;
            if (Input.Key(negativeY)) val.Y -= 1;
            return val;
        }
    }

    public enum MouseButton
    {
        Left = 0,
        //
        // Summary:
        //     The right mouse button. This corresponds to OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Button2.
        Right = 1,
        //
        // Summary:
        //     The middle mouse button. This corresponds to OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Button3.
        Middle = 2,
    }

    public enum Key
    {
        //
        // Summary:
        //     An unknown key.
        Unknown = -1,
        //
        // Summary:
        //     The spacebar key.
        Space = 32,
        //
        // Summary:
        //     The apostrophe key.
        Apostrophe = 39,
        //
        // Summary:
        //     The comma key.
        Comma = 44,
        //
        // Summary:
        //     The minus key.
        Minus = 45,
        //
        // Summary:
        //     The period key.
        Period = 46,
        //
        // Summary:
        //     The slash key.
        Slash = 47,
        //
        // Summary:
        //     The 0 key.
        D0 = 48,
        //
        // Summary:
        //     The 1 key.
        D1 = 49,
        //
        // Summary:
        //     The 2 key.
        D2 = 50,
        //
        // Summary:
        //     The 3 key.
        D3 = 51,
        //
        // Summary:
        //     The 4 key.
        D4 = 52,
        //
        // Summary:
        //     The 5 key.
        D5 = 53,
        //
        // Summary:
        //     The 6 key.
        D6 = 54,
        //
        // Summary:
        //     The 7 key.
        D7 = 55,
        //
        // Summary:
        //     The 8 key.
        D8 = 56,
        //
        // Summary:
        //     The 9 key.
        D9 = 57,
        //
        // Summary:
        //     The semicolon key.
        Semicolon = 59,
        //
        // Summary:
        //     The equal key.
        Equal = 61,
        //
        // Summary:
        //     The A key.
        A = 65,
        //
        // Summary:
        //     The B key.
        B = 66,
        //
        // Summary:
        //     The C key.
        C = 67,
        //
        // Summary:
        //     The D key.
        D = 68,
        //
        // Summary:
        //     The E key.
        E = 69,
        //
        // Summary:
        //     The F key.
        F = 70,
        //
        // Summary:
        //     The G key.
        G = 71,
        //
        // Summary:
        //     The H key.
        H = 72,
        //
        // Summary:
        //     The I key.
        I = 73,
        //
        // Summary:
        //     The J key.
        J = 74,
        //
        // Summary:
        //     The K key.
        K = 75,
        //
        // Summary:
        //     The L key.
        L = 76,
        //
        // Summary:
        //     The M key.
        M = 77,
        //
        // Summary:
        //     The N key.
        N = 78,
        //
        // Summary:
        //     The O key.
        O = 79,
        //
        // Summary:
        //     The P key.
        P = 80,
        //
        // Summary:
        //     The Q key.
        Q = 81,
        //
        // Summary:
        //     The R key.
        R = 82,
        //
        // Summary:
        //     The S key.
        S = 83,
        //
        // Summary:
        //     The T key.
        T = 84,
        //
        // Summary:
        //     The U key.
        U = 85,
        //
        // Summary:
        //     The V key.
        V = 86,
        //
        // Summary:
        //     The W key.
        W = 87,
        //
        // Summary:
        //     The X key.
        X = 88,
        //
        // Summary:
        //     The Y key.
        Y = 89,
        //
        // Summary:
        //     The Z key.
        Z = 90,
        //
        // Summary:
        //     The left bracket(opening bracket) key.
        LeftBracket = 91,
        //
        // Summary:
        //     The backslash.
        Backslash = 92,
        //
        // Summary:
        //     The right bracket(closing bracket) key.
        RightBracket = 93,
        //
        // Summary:
        //     The grave accent key.
        GraveAccent = 96,
        //
        // Summary:
        //     The escape key.
        Escape = 256,
        //
        // Summary:
        //     The enter key.
        Enter = 257,
        //
        // Summary:
        //     The tab key.
        Tab = 258,
        //
        // Summary:
        //     The backspace key.
        Backspace = 259,
        //
        // Summary:
        //     The insert key.
        Insert = 260,
        //
        // Summary:
        //     The delete key.
        Delete = 261,
        //
        // Summary:
        //     The right arrow key.
        Right = 262,
        //
        // Summary:
        //     The left arrow key.
        Left = 263,
        //
        // Summary:
        //     The down arrow key.
        Down = 264,
        //
        // Summary:
        //     The up arrow key.
        Up = 265,
        //
        // Summary:
        //     The page up key.
        PageUp = 266,
        //
        // Summary:
        //     The page down key.
        PageDown = 267,
        //
        // Summary:
        //     The home key.
        Home = 268,
        //
        // Summary:
        //     The end key.
        End = 269,
        //
        // Summary:
        //     The caps lock key.
        CapsLock = 280,
        //
        // Summary:
        //     The scroll lock key.
        ScrollLock = 281,
        //
        // Summary:
        //     The num lock key.
        NumLock = 282,
        //
        // Summary:
        //     The print screen key.
        PrintScreen = 283,
        //
        // Summary:
        //     The pause key.
        Pause = 284,
        //
        // Summary:
        //     The F1 key.
        F1 = 290,
        //
        // Summary:
        //     The F2 key.
        F2 = 291,
        //
        // Summary:
        //     The F3 key.
        F3 = 292,
        //
        // Summary:
        //     The F4 key.
        F4 = 293,
        //
        // Summary:
        //     The F5 key.
        F5 = 294,
        //
        // Summary:
        //     The F6 key.
        F6 = 295,
        //
        // Summary:
        //     The F7 key.
        F7 = 296,
        //
        // Summary:
        //     The F8 key.
        F8 = 297,
        //
        // Summary:
        //     The F9 key.
        F9 = 298,
        //
        // Summary:
        //     The F10 key.
        F10 = 299,
        //
        // Summary:
        //     The F11 key.
        F11 = 300,
        //
        // Summary:
        //     The F12 key.
        F12 = 301,
        //
        // Summary:
        //     The F13 key.
        F13 = 302,
        //
        // Summary:
        //     The F14 key.
        F14 = 303,
        //
        // Summary:
        //     The F15 key.
        F15 = 304,
        //
        // Summary:
        //     The F16 key.
        F16 = 305,
        //
        // Summary:
        //     The F17 key.
        F17 = 306,
        //
        // Summary:
        //     The F18 key.
        F18 = 307,
        //
        // Summary:
        //     The F19 key.
        F19 = 308,
        //
        // Summary:
        //     The F20 key.
        F20 = 309,
        //
        // Summary:
        //     The F21 key.
        F21 = 310,
        //
        // Summary:
        //     The F22 key.
        F22 = 311,
        //
        // Summary:
        //     The F23 key.
        F23 = 312,
        //
        // Summary:
        //     The F24 key.
        F24 = 313,
        //
        // Summary:
        //     The F25 key.
        F25 = 314,
        //
        // Summary:
        //     The 0 key on the key pad.
        KeyPad0 = 320,
        //
        // Summary:
        //     The 1 key on the key pad.
        KeyPad1 = 321,
        //
        // Summary:
        //     The 2 key on the key pad.
        KeyPad2 = 322,
        //
        // Summary:
        //     The 3 key on the key pad.
        KeyPad3 = 323,
        //
        // Summary:
        //     The 4 key on the key pad.
        KeyPad4 = 324,
        //
        // Summary:
        //     The 5 key on the key pad.
        KeyPad5 = 325,
        //
        // Summary:
        //     The 6 key on the key pad.
        KeyPad6 = 326,
        //
        // Summary:
        //     The 7 key on the key pad.
        KeyPad7 = 327,
        //
        // Summary:
        //     The 8 key on the key pad.
        KeyPad8 = 328,
        //
        // Summary:
        //     The 9 key on the key pad.
        KeyPad9 = 329,
        //
        // Summary:
        //     The decimal key on the key pad.
        KeyPadDecimal = 330,
        //
        // Summary:
        //     The divide key on the key pad.
        KeyPadDivide = 331,
        //
        // Summary:
        //     The multiply key on the key pad.
        KeyPadMultiply = 332,
        //
        // Summary:
        //     The subtract key on the key pad.
        KeyPadSubtract = 333,
        //
        // Summary:
        //     The add key on the key pad.
        KeyPadAdd = 334,
        //
        // Summary:
        //     The enter key on the key pad.
        KeyPadEnter = 335,
        //
        // Summary:
        //     The equal key on the key pad.
        KeyPadEqual = 336,
        //
        // Summary:
        //     The left shift key.
        LeftShift = 340,
        //
        // Summary:
        //     The left control key.
        LeftControl = 341,
        //
        // Summary:
        //     The left alt key.
        LeftAlt = 342,
        //
        // Summary:
        //     The left super key.
        LeftSuper = 343,
        //
        // Summary:
        //     The right shift key.
        RightShift = 344,
        //
        // Summary:
        //     The right control key.
        RightControl = 345,
        //
        // Summary:
        //     The right alt key.
        RightAlt = 346,
        //
        // Summary:
        //     The right super key.
        RightSuper = 347,
        //
        // Summary:
        //     The menu key.
        Menu = 348,
        //
        // Summary:
        //     The last valid key in this enum.
        LastKey = 348
    }
}
