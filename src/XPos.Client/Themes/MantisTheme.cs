using MudBlazor;

namespace XPos.Client.Themes
{
    public static class MantisTheme
    {
        public static MudTheme DefaultTheme = new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = "#1890ff",
                Secondary = "#faaf00",
                AppbarBackground = "#ffffff",
                AppbarText = "#262626",
                Background = "#f0f2f5",
                DrawerBackground = "#ffffff",
                DrawerText = "#262626",
                Surface = "#ffffff",
                TextPrimary = "#262626",
                TextSecondary = "#8c8c8c",
                ActionDefault = "#8c8c8c",
                Divider = "#f0f0f0",
                TableLines = "#f0f0f0",
            },
            /* Typography section commented out due to type issues in MudBlazor v8
            Typography = new Typography()
            {
                Default = new()
                {
                    FontFamily = new[] { "Public Sans", "Roboto", "Helvetica", "Arial", "sans-serif" }
                },
                H1 = new() { FontFamily = new[] { "Public Sans", "Roboto", "Helvetica", "Arial", "sans-serif" }, FontSize = "2.5rem", FontWeight = 600 },
                ...
            },
            */
            LayoutProperties = new LayoutProperties()
            {
                DrawerWidthLeft = "260px",
                AppbarHeight = "64px"
            }
        };
    }
}
