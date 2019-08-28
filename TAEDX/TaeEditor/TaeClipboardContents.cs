using SoulsFormats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TAEDX.TaeEditor
{
    public class TaeClipboardContents
    {
        public class Clip
        {
            public int EventType { get; set; }
            public int EventUnk04 { get; set; }
            public float EventStartTime { get; set; }
            public float EventEndTime { get; set; }
            public int EventRow { get; set; }
            public byte[] EventParamBytes { get; set; }
        }

        public IEnumerable<Clip> EventClips { get; set; }
        public int StartRow { get; set; } = 0;
        public float StartTime { get; set; } = 0;
        public bool IsBigEndian { get; set; }

        public TaeEditAnimEventGraph ParentGraph { get; set; }

        public TaeClipboardContents(TaeEditAnimEventGraph graph)
        {
            ParentGraph = graph;
        }

        public TaeClipboardContents(TaeEditAnimEventGraph graph, IEnumerable<TaeEditAnimEventBox> events, int startRow, float startTime, bool isBigEndian)
        {
            ParentGraph = graph;
            StartRow = startRow;
            StartTime = startTime;
            EventClips = events.Select(x => EventToClip(x, isBigEndian));
            IsBigEndian = isBigEndian;
        }

        public IEnumerable<TaeEditAnimEventBox> GetEvents()
        {
            return EventClips.Select(x => ClipToEvent(ParentGraph, x, IsBigEndian));
        }

        public static Clip EventToClip(TaeEditAnimEventBox ev, bool isBigEndian)
        {
            var clip = new Clip();
            clip.EventType = ev.MyEvent.Type;
            clip.EventUnk04 = ev.MyEvent.Unk04;
            clip.EventStartTime = ev.MyEvent.StartTime;
            clip.EventEndTime = ev.MyEvent.EndTime;
            clip.EventRow = ev.Row;
            clip.EventParamBytes = ev.MyEvent.GetParameterBytes(isBigEndian);
            return clip;
        }

        public static TaeEditAnimEventBox ClipToEvent(TaeEditAnimEventGraph graph, Clip c, bool isBigEndian)
        {
            var newEvent = new TAE.Event(c.EventStartTime, c.EventEndTime, c.EventType, c.EventUnk04, c.EventParamBytes, isBigEndian);
            var newEventBox = new TaeEditAnimEventBox(graph, newEvent);
            newEventBox.Row = c.EventRow;
            return newEventBox;
        }

        
    }
}
