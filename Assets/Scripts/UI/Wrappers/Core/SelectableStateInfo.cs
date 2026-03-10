namespace UI.Wrappers.Core
{
    public struct SelectableStateInfo
    {
        public SelectableState State;
        public bool Instant;

        public SelectableStateInfo(SelectableState state, bool instant)
        {
            State = state;
            Instant = instant;
        }
    }
}