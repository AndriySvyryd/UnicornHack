namespace UnicornHack.Utils.MessagingECS;

public interface ITrackable
{
    void StartTracking(object tracker);
    void StopTracking(object tracker);
}
