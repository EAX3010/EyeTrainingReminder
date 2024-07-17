namespace EyeTrainingReminder
{
    public class AppConfig
    {
        // Colors
        public static Color BackgroundColor = Color.FromArgb(18, 18, 18);
        public static Color Disabled = Color.FromArgb(189, 195, 199);
        public static Color TextColor = Color.FromArgb(220, 220, 220);
        public static Color AccentColor = Color.FromArgb(103, 152, 188);
        public static Color DrawingBackgroundColor = Color.FromArgb(30, 30, 30);

        // Sizes
        public static double FormSizeRatio = 1; // 100% of screen size
        public static double InstructionLabelHeightRatio = 0.12;
        public static double CountdownLabelHeightRatio = 0.06;
        public static double DrawingPanelWidthRatio = 0.95;
        public static double DrawingPanelHeightRatio = 0.6;
        public static double NextButtonWidthRatio = 0.2;
        public static double NextButtonHeightRatio = 0.08;

        // Fonts
        public static Font InstructionFont = new Font("Arial", 12, FontStyle.Regular);
        public static Font CountdownFont = new Font("Arial", 12, FontStyle.Bold);

        // Timers
        public static int HourlyTimerInterval = 36000; // 1 hour in milliseconds
        public static int ExerciseTimerInterval = 1000; // 1 second

#if DEBUG
        // Exercise duration
        public static int ExerciseDuration = 1; // 30 seconds
#else
        // Exercise duration
        public static int ExerciseDuration = 30; // 30 seconds
#endif


        // Drawing
        public static int DrawingElementSizeRatio = 20; // Divide by this to get radius
    }

}
