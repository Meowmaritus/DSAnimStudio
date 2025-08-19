namespace DSAnimStudio
{
    public partial class TaeAnimSequencer
    {
        public abstract class Command
        {
            public float Time;
            public abstract void DoCommand(TaeAnimSequencer sequencer, TaeAnimSequencer.Channel channel);
        }
    }
}