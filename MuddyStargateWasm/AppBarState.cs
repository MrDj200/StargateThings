namespace MuddyStargateWasm
{
    public class AppBarState
    {
        public bool ShowRefreshButton { get; private set; }
        public Action? OnRefreshClicked { get; private set; }

        public event Action? OnChange;

        public void SetRefresh(Action onClick)
        {
            ShowRefreshButton = true;
            OnRefreshClicked = onClick;
            NotifyStateChanged();
        }

        public void ClearRefresh()
        {
            ShowRefreshButton = false;
            OnRefreshClicked = null;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
