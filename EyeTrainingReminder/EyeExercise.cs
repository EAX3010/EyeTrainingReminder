using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyeTrainingReminder
{
    public class EyeExercise
    {
        public string Name { get; }
        public string Instructions { get; }
        public int DurationSeconds { get; }
        public Action<Graphics, Rectangle, int> DrawAction { get; }

        public EyeExercise(string name, string instructions, int durationSeconds, Action<Graphics, Rectangle, int> drawAction)
        {
            Name = name;
            Instructions = instructions;
            DurationSeconds = durationSeconds;
            DrawAction = drawAction;
        }
    }
}
