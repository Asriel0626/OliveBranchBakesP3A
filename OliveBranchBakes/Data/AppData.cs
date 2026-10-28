namespace OliveBranchBakes.Data;

/// <summary>Provides access to the application's shared data store.</summary>
public static class AppData
{
    public static DataStore Store { get; private set; } = null!;
    public static void Initialize() => Store = new DataStore();
}
