using System;
using FirstFloor.ModernUI.Presentation;

namespace SignalCompressionMUI.Models
{
    public static class ZedGraphModel
    {
        private static Uri _themeType = AppearanceManager.LightThemeSource;
        public static Uri ThemeType
        {
            get { return _themeType; }
            set
            {
                _themeType = value;
                OnThemeChanged?.Invoke();
            }
        }

        public delegate void ThemeChangedHandler();
        public static event ThemeChangedHandler OnThemeChanged;
    }
}
