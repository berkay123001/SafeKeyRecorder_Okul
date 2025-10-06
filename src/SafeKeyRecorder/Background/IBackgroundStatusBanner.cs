namespace SafeKeyRecorder.Background;

public interface IBackgroundStatusBanner
{
    void ShowBackgroundEnabled();

    void ShowBackgroundDisabled();

    void ShowPassive(string message);

    void Hide();
}
