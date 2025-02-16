using CrymexEngine.Utils;

namespace CrymexEngine
{
    public class EventSystem
    {
        /// <summary>
        /// An internal instance
        /// </summary>
        public static EventSystem Instance
        {
            get
            {
                return _instance;
            }
        }

        private static List<GameTimeEvent> events = new List<GameTimeEvent>();

        private static EventSystem _instance = new EventSystem();

        internal void Update()
        {
            for (int i = 0; i < events.Count; i++)
            {
                GameTimeEvent e = events[i];
                float dif = Time.GameTime - e.startTime;

                while (dif > e.time)
                {
                    e.action?.Invoke();

                    if (!e.repeat)
                    {
                        events.Remove(e);
                        break;
                    }
                    else e.startTime = Time.GameTime;

                    dif -= e.time;
                }
            }
        }

        public static void AddEvent(string name, Action action, float time, bool locked = false)
        {
            GameTimeEvent e = new GameTimeEvent(name, action, time, out bool successful, false, locked);
            if (successful) events.Add(e);
        }
        public static void AddEventRepeat(string name, Action action, float time, bool locked = false)
        {
            GameTimeEvent e = new GameTimeEvent(name, action, time, out bool successful, true, locked);
            if (successful) events.Add(e);
        }

        public static void AddListener(string name, Action listener)
        {
            foreach (GameTimeEvent ev in events)
            {
                if (ev.name == name && !ev.isLocked)
                {
                    ev.action += listener;
                    return;
                }
            }
        }

        public static void RemoveEvent(string name)
        {
            foreach (GameTimeEvent e in events)
            {
                if (e.name == name)
                {
                    events.Remove(e);
                }
            }
        }

        public static bool IsEventActive(string name)
        {
            foreach (GameTimeEvent ev in events)
            {
                if (ev.name == name) return true;
            }
            return false;
        }

        public static bool IsEventRepeat(string name)
        {
            foreach (GameTimeEvent ev in events)
            {
                if (ev.name == name) return ev.repeat;
            }
            return false;
        }

        /// <returns>float.NaN if not found</returns>
        public static float GetEventPeriod(string name)
        {
            foreach (GameTimeEvent ev in events)
            {
                if (ev.name == name) return ev.time;
            }
            return float.NaN;
        }
    }

    class GameTimeEvent
    {
        public float startTime;
        public readonly string name;
        public readonly float time;
        public readonly bool repeat;
        public readonly bool isLocked;
        public Action action;

        public GameTimeEvent(string name, Action action, float time, out bool successful, bool repeat = false, bool isLocked = false)
        {
            this.name = name;
            this.action = action;
            this.time = time;
            this.repeat = repeat;
            this.isLocked = isLocked;
            startTime = Time.GameTime;

            if (time <= 0 || !float.IsNormal(time))
            {
                successful = false;
                Debug.LogWarning($"Time setting for the event '{name}' is not in the correct format ({DataUtilities.FloatToShortString(time, 3)})");
            }
            else successful = true;
        }
    }
}
