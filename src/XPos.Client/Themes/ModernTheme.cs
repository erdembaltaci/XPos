using MudBlazor;

namespace XPos.Client.Themes;

public static class ModernTheme
{
    public static MudTheme DefaultTheme => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#FF5500",        // Vibrant Orange (Appetizing)
            Secondary = "#101828",      // Dark Slate (Text/Contrast)
            Tertiary = "#F0F0F0",       // Light Gray (Backgrounds)
            AppbarBackground = "#FFFFFF",
            Background = "#F9FAFB",     // Very Light Blue-Gray
            Surface = "#FFFFFF",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#344054",
            TextPrimary = "#101828",
            TextSecondary = "#667085",
            ActionDefault = "#667085",
            ActionDisabled = "rgba(0,0,0, 0.26)",
            Divider = "#EAECF0",
            LinesDefault = "#EAECF0"
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = new[] { "Outfit", "Inter", "sans-serif" },
                FontSize = "0.95rem",
                FontWeight = "400",
                LineHeight = "1.5",
                LetterSpacing = "0.01em"
            },
            H1 = new H1Typography { FontFamily = new[] { "Outfit", "sans-serif" }, FontWeight = "700", LineHeight = "1.2" },
            H2 = new H2Typography { FontFamily = new[] { "Outfit", "sans-serif" }, FontWeight = "700", LineHeight = "1.3" },
            H3 = new H3Typography { FontFamily = new[] { "Outfit", "sans-serif" }, FontWeight = "600", LineHeight = "1.3" },
            H4 = new H4Typography { FontFamily = new[] { "Outfit", "sans-serif" }, FontWeight = "600", LineHeight = "1.4" },
            H5 = new H5Typography { FontFamily = new[] { "Outfit", "sans-serif" }, FontWeight = "600" },
            H6 = new H6Typography { FontFamily = new[] { "Outfit", "sans-serif" }, FontWeight = "600" },
            Button = new ButtonTypography { FontFamily = new[] { "Outfit", "sans-serif" }, FontWeight = "600", TextTransform = "none" },
            Body1 = new Body1Typography { FontFamily = new[] { "Inter", "sans-serif" }, FontSize = "1rem", LineHeight = "1.5" },
            Body2 = new Body2Typography { FontFamily = new[] { "Inter", "sans-serif" }, FontSize = "0.875rem", LineHeight = "1.43" }
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "16px",
            DrawerWidthLeft = "280px"
        }
    };
}
