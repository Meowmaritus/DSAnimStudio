using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DSAnimStudio.TaeEditor
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
            public TAE.EventGroup EventGroup { get; set; }
        }

        public List<Clip> EventClips { get; set; }
        public int StartRow { get; set; } = 0;
        public float StartTime { get; set; } = 0;
        public bool IsBigEndian { get; set; }

        // Sorry this is ugly
        public static TaeEditAnimEventGraph ParentGraph;

        public TaeClipboardContents()
        {

        }

        public TaeClipboardContents(IEnumerable<TaeEditAnimEventBox> events, int startRow, float startTime, bool isBigEndian)
        {
            StartRow = startRow;
            StartTime = startTime;
            EventClips = events.Select(x => EventToClip(x, isBigEndian)).ToList();
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
            clip.EventGroup = ev.MyEvent.Group;
            return clip;
        }

        public static TaeEditAnimEventBox ClipToEvent(TaeEditAnimEventGraph graph, Clip c, bool isBigEndian)
        {
            var newEvent = new TAE.Event(c.EventStartTime, c.EventEndTime, c.EventType, c.EventUnk04, c.EventParamBytes, isBigEndian);
            var newEventBox = new TaeEditAnimEventBox(graph, newEvent, graph.AnimRef);
            newEventBox.MyEvent.Group = c.EventGroup;
            newEventBox.Row = c.EventRow;
            return newEventBox;
        }

        
    }
}
