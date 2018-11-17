using MeowDSIO;
using MeowDSIO.DataTypes.TAE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX.TaeEditor
{
    public class TaeClipboardContents
    {
        public class Clip
        {
            public TimeActEventType EventType { get; set; }
            public float EventStartTime { get; set; }
            public float EventEndTime { get; set; }
            public int EventRow { get; set; }
            public byte[] EventParamBytes { get; set; }
        }

        public IEnumerable<Clip> EventClips { get; set; }
        public int StartRow { get; set; } = 0;
        public float StartTime { get; set; } = 0;

        public TaeClipboardContents()
        {

        }

        public TaeClipboardContents(IEnumerable<TimeActEventBase> events, int startRow, float startTime)
        {
            StartRow = startRow;
            StartTime = startTime;
            EventClips = events.Select(x => EventToClip(x));
        }

        public IEnumerable<TimeActEventBase> GetEvents()
        {
            return EventClips.Select(x => ClipToEvent(x));
        }

        public static Clip EventToClip(TimeActEventBase ev)
        {
            var clip = new Clip();
            clip.EventType = ev.EventType;
            clip.EventStartTime = ev.StartTime;
            clip.EventEndTime = ev.EndTime;
            clip.EventRow = ev.Row;
            using (var paramMemStream = new MemoryStream())
            {
                using (var bin = new DSBinaryWriter("", paramMemStream))
                {
                    ev.WriteParameters(bin);
                }
                clip.EventParamBytes = paramMemStream.ToArray();
            }
            return clip;
        }

        public static TimeActEventBase ClipToEvent(Clip c)
        {
            var newEvent = TimeActEventBase.GetNewEvent(c.EventType, c.EventStartTime, c.EventEndTime);
            using(var paramMemStream = new MemoryStream(c.EventParamBytes))
            {
                using (var bin = new DSBinaryReader("", paramMemStream))
                {
                    newEvent.ReadParameters(bin);
                }
            }
            newEvent.Row = c.EventRow;
            return newEvent;
        }

        
    }
}
