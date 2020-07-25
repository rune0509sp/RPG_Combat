namespace RPG.Saving
{
  public interface ISaveable
  {
    object CaptureSate();
    void RestoreSate(object state);
  }
}