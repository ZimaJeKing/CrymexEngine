using OpenTK.Mathematics;
using System.Security.Principal;

namespace CrymexEngine
{
    public class LineGroup : Behaviour
    {
        private Color4 _color;
        private float _width;
        private float _depth;
        private bool _screenSpace;
        private Vector2[] _points;
        private Line[] _lines;

        public LineGroup(Vector2[] points, Color4 color, float width = 1, float depth = 0, bool screenSpace = false)
        {
            _color = color;
            _width = width;
            _depth = depth;
            _screenSpace = screenSpace;
            _points = points;

            if (points == null || points.Length < 2) throw new ArgumentException("You have to provide at least two points for a line group");

            GenLines();

            Scenes.Scene.Current.lines.Add(this);
        }

        public float Depth
        {
            get => _depth;
            set
            {
                _depth = -Math.Clamp(value, 0.001f, 1f);

                foreach (Line line in _lines)
                {
                    line.Depth = value;
                }
            }
        }
        public float Width
        {
            get => _width;
            set
            {
                _width = Math.Clamp(value, 0.1f, 100f);

                foreach (Line line in _lines)
                {
                    line.Width = value;
                }
            }
        }
        public bool ScreenSpace
        {
            get => _screenSpace;
            set
            {
                _screenSpace = value;
                foreach (Line line in _lines)
                {
                    line.ScreenSpace = value;
                }
            }
        }
        public Color4 Color
        {
            get => _color;
            set
            {
                _color = value;
                foreach (Line line in _lines)
                {
                    line.color = value;
                }
            }
        }

        public Vector2[] Points
        {
            set
            {
                if (value == null || value.Length < 2) return;

                _points = value;

                GenLines();
            }
        }

        internal LineGroup(Line line)
        {
            if (line.group != null) throw new ArgumentException("Line already has an assigned line group");

            _lines = [line];
            _color = line.color;
            _width = line.Width;
            _depth = line.Depth;
            _screenSpace = line.ScreenSpace;
            _points = [ line.Start, line.End ];

            Scenes.Scene.Current.lines.Add(this);
        }

        public void SetPoint(int index, Vector2 point)
        {
            if (index >= _points.Length || index < 0) throw new IndexOutOfRangeException();

            _points[index] = point;
            GenLines();
        }

        public void Delete()
        {
            Scenes.Scene.Current.lineDeleteQueue.Add(this);
        }

        protected override void Load()
        {
        }

        protected override void Update()
        {
            foreach (Line line in _lines)
            {
                UpdateBehaviour(line);
            }
        }

        private void GenLines()
        {
            _lines = new Line[_points.Length - 1];
            for (int i = 0; i < _points.Length - 1; i++)
            {
                _lines[i] = new Line(_points[i], _points[i + 1], _color, this, _width, _depth, _screenSpace);
            }
        }

        protected override void OnQuit() { }
    }
}
