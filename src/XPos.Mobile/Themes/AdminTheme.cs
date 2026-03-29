using MudBlazor;

namespace XPos.Mobile.Themes;

public static class AdminTheme
{
    public static MudTheme DefaultTheme => new MudTheme()
    {
        PaletteDark = new PaletteDark()
        {
            Primary = "#6366F1",
            Secondary = "#8B5CF6",
            Tertiary = "#EC4899",
            Info = "#38BDF8",
            Success = "#34D399",
            Warning = "#FBBF24",
            Error = "#F87171",
            Background = "#0F172A",
            Surface = "#1E293B",
            AppbarBackground = "#1E293B",
            AppbarText = "#E2E8F0",
            DrawerBackground = "#1E293B",
            TextPrimary = "#E2E8F0",
            TextSecondary = "#94A3B8",
        },
        PaletteLight = new PaletteLight()
        {
            Primary = "#4F46E5",
            Secondary = "#7C3AED",
            Tertiary = "#DB2777",
            Info = "#0EA5E9",
            Success = "#10B981",
            Warning = "#F59E0B",
            Error = "#EF4444",
            Background = "#F8FAFC",
            Surface = "#FFFFFF",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#1E293B",
            DrawerBackground = "#1E293B",
            DrawerText = "#E2E8F0",
            TextPrimary = "#1E293B",
            TextSecondary = "#64748B",
        },
        LayoutProperties = new LayoutProperties()
        {
            AppbarHeight = "56px",
        },
        Typography = new Typography()
        {
            Default = new DefaultTypography()
            {
                FontFamily = new[] { "Inter", "system-ui", "sans-serif" },
            }
        },
    };
}
