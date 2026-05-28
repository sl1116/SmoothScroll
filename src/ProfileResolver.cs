namespace SmoothScrollLocal;

public static class ProfileResolver
{
    public static ScrollProfile Resolve(AppSettings settings, string? processName)
    {
        if (!string.IsNullOrWhiteSpace(processName) &&
            settings.AppProfiles.TryGetValue(processName, out var profile))
        {
            return profile.Clone();
        }

        return settings.ToScrollProfile();
    }
}
