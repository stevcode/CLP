using System.Collections.Generic;
using System.Windows.Media;

namespace Classroom_Learning_Partner.Services
{
    public class TextBoxService
    {
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public bool IsUnderlined { get; set; }

        private readonly List<FontFamily> _availableFontFamilies = new List<FontFamily>(Fonts.SystemFontFamilies);

        public List<FontFamily> AvailableFontFamilies
        {
            get { return _availableFontFamilies; }
        }

        public FontFamily CurrentFontFamily { get; set; }

        private readonly List<double> _availableFontSizes = new List<double>
                                                            {
                                                                3.0,
                                                                4.0,
                                                                5.0,
                                                                6.0,
                                                                6.5,
                                                                7.0,
                                                                7.5,
                                                                8.0,
                                                                8.5,
                                                                9.0,
                                                                9.5,
                                                                10.0,
                                                                10.5,
                                                                11.0,
                                                                11.5,
                                                                12.0,
                                                                12.5,
                                                                13.0,
                                                                13.5,
                                                                14.0,
                                                                15.0,
                                                                16.0,
                                                                17.0,
                                                                18.0,
                                                                19.0,
                                                                20.0,
                                                                22.0,
                                                                24.0,
                                                                26.0,
                                                                28.0,
                                                                30.0,
                                                                32.0,
                                                                34.0,
                                                                36.0,
                                                                38.0,
                                                                40.0,
                                                                44.0,
                                                                48.0,
                                                                52.0,
                                                                56.0,
                                                                60.0,
                                                                64.0,
                                                                68.0,
                                                                72.0,
                                                                76.0,
                                                                80.0,
                                                                88.0,
                                                                96.0,
                                                                104.0,
                                                                112.0,
                                                                120.0,
                                                                128.0,
                                                                136.0,
                                                                144.0
                                                            };

        public List<double> AvailableFontSizes
        {
            get { return _availableFontSizes; }
        }

        public double CurrentFontSize { get; set; }

        private readonly List<Brush> _availableFontColors = new List<Brush>
                                                   {
                                                       new SolidColorBrush(Colors.Black),
                                                       new SolidColorBrush(Colors.Red),
                                                       new SolidColorBrush(Colors.DarkOrange),
                                                       new SolidColorBrush(Colors.Tan),
                                                       new SolidColorBrush(Colors.Gold),
                                                       new SolidColorBrush(Colors.SaddleBrown),
                                                       new SolidColorBrush(Colors.DarkGreen),
                                                       new SolidColorBrush(Colors.MediumSeaGreen),
                                                       new SolidColorBrush(Colors.Blue),
                                                       new SolidColorBrush(Colors.HotPink),
                                                       new SolidColorBrush(Colors.BlueViolet),
                                                       new SolidColorBrush(Colors.Aquamarine),
                                                       new SolidColorBrush(Colors.SlateGray),
                                                       new SolidColorBrush(Colors.SkyBlue),
                                                       new SolidColorBrush(Colors.DeepSkyBlue),
                                                       new SolidColorBrush(Color.FromRgb(0, 152, 247))
                                                   };

        public List<Brush> AvailableFontColors
        {
            get
            {
                return _availableFontColors;
            }
        }

        public Brush CurrentFontColor { get; set; }
    }
}