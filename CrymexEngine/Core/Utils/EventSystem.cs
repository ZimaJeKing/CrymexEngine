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

        private static List<Event> events = new List<Event>();

        private static EventSystem _instance = new EventSystem();

        public void Update()
        {
            foreach (Event e in events)
            {
                if (Time.GameTime - e.startTime > e.time)
                {
                    e.action?.Invoke();

                    if (!e.repeat)
                    {
                        events.Remove(e);
                        continue;
                    }

                    e.startTime = Time.GameTime;
                }
            }
        }

        public static void AddEvent(string name, Action action, float time)
        {
            events.Add(new Event(name, action, time));
        }
        public static void AddEventRepeat(string name, Action action, float time)
        {
            events.Add(new Event(name, action, time, true));
        }

        public static void RemoveEvent(string name)
        {
            foreach (Event e in events)
            {
                if (e.name == name)
                {
                    events.Remove(e);
                }
            }
        }
    }

    class Event
    {
        public float startTime;
        public readonly string name;
        public readonly float time;
        public readonly bool repeat;
        public readonly Action action;

        public Event(string name, Action action, float time, bool repeat = false)
        {
            this.name = name;
            this.action = action;
            startTime = Time.GameTime;
            this.time = time;
            this.repeat = repeat;
        }
    }
}
