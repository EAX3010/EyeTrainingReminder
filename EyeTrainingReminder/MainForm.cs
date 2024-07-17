using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;

namespace EyeTrainingReminder
{
    public partial class MainForm : Form
    {
        private System.Windows.Forms.Timer hourlyTimer;
        private System.Windows.Forms.Timer exerciseTimer;
        private Label instructionLabel;
        private Label countdownLabel;
        private Button nextButton;
        private List<EyeExercise> exercises;
        private int currentExerciseIndex;
        private int countdown;
        private Panel drawingPanel;
        private Bitmap drawingBitmap;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.ComponentModel.IContainer notifyIconcomponents;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private bool contextClose = false;
        public MainForm()
        {
            InitializeExercises();
            InitializeTimers();
            InitializeControls();
            this.Text = "Interactive Eye Training Application";
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Resize += MainForm_Resize;

            // Apply dark theme to the form
            this.BackColor = AppConfig.BackgroundColor;
            this.ForeColor = AppConfig.TextColor;

            SetFormSize();

            this.contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show", null, onClick: Show);
            contextMenu.Items.Add("Hide", null, onClick: Hide);
            contextMenu.Items.Add("Exit", null, onClick: Close);
            notifyIconcomponents = new System.ComponentModel.Container();
            notifyIcon = new System.Windows.Forms.NotifyIcon(this.notifyIconcomponents);
            notifyIcon.Icon = new Icon("app.ico");
            notifyIcon.Text = "EyeTrainingReminder";
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += Show;
            notifyIcon.Click += Show;
            notifyIcon.ContextMenuStrip = contextMenu;
        }
        private void Close(object sender, EventArgs e)
        {
            contextClose = true;
            base.Close();
        }
        private void Hide(object sender, EventArgs e)
        {
            this.Hide();
        }
        private void Show(object sender, EventArgs e)
        {
            this.Show();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.UserClosing && !contextClose)
            {
                e.Cancel = true;
                Hide();
            }
        }
        // Method to set the form size and location based on screen dimensions
        private void SetFormSize()
        {
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
            this.Size = new Size(
                (int)(screenBounds.Width * AppConfig.FormSizeRatio),
                (int)(screenBounds.Height * AppConfig.FormSizeRatio)
            );
            this.Location = new Point(
                (screenBounds.Width - this.Width) / 2,
                (screenBounds.Height - this.Height) / 2
            );
            ResizeControls();
        }

        // Event handler for form resize
        private void MainForm_Resize(object sender, EventArgs e)
        {
            ResizeControls();
        }

        // Method to resize controls based on the form size
        private void ResizeControls()
        {
            if (instructionLabel != null)
                instructionLabel.Height = (int)(this.ClientSize.Height * AppConfig.InstructionLabelHeightRatio);

            if (countdownLabel != null)
                countdownLabel.Height = (int)(this.ClientSize.Height * AppConfig.CountdownLabelHeightRatio);

            if (drawingPanel != null)
            {
                drawingPanel.Size = new Size(
                    (int)(this.ClientSize.Width * AppConfig.DrawingPanelWidthRatio),
                    (int)(this.ClientSize.Height * AppConfig.DrawingPanelHeightRatio)
                );
                drawingPanel.Location = new Point(
                    (this.ClientSize.Width - drawingPanel.Width) / 2,
                    instructionLabel.Bottom + (int)(this.ClientSize.Height * 0.02)
                );
                drawingBitmap = new Bitmap(drawingPanel.Width, drawingPanel.Height);
            }

            if (nextButton != null)
            {
                nextButton.Size = new Size(
                    (int)(this.ClientSize.Width * AppConfig.NextButtonWidthRatio),
                    (int)(this.ClientSize.Height * AppConfig.NextButtonHeightRatio)
                );
                nextButton.Location = new Point(
                    (this.ClientSize.Width - nextButton.Width) / 2,
                    this.ClientSize.Height - nextButton.Height - (int)(this.ClientSize.Height * 0.05)
                );
            }

            this.Invalidate();
        }

        // Method to initialize exercises
        private void InitializeExercises()
        {
            exercises = new List<EyeExercise>
            {
                new EyeExercise("Focus Shift", "Focus on the light blue dot, then on the light green dot. Shift your focus back and forth.", AppConfig.ExerciseDuration, DrawFocusShift),
                new EyeExercise("Figure Eight", "Follow the moving pink dot with your eyes as it traces a figure eight.", AppConfig.ExerciseDuration, DrawFigureEight),
                new EyeExercise("Diagonal Stretch", "Follow the purple dot as it moves from corner to corner.", AppConfig.ExerciseDuration, DrawDiagonalStretch),
                new EyeExercise("Circular Motion", "Follow the orange dot as it moves in a circular pattern.", AppConfig.ExerciseDuration, DrawCircularMotion),
                new EyeExercise("Butterfly Blink", "Blink rapidly for 15 seconds, then close your eyes and relax for 15 seconds.", AppConfig.ExerciseDuration, null)
            };
        }

        // Method to initialize timers
        private void InitializeTimers()
        {
            hourlyTimer = new System.Windows.Forms.Timer
            {
                Interval = AppConfig.HourlyTimerInterval
            };
            hourlyTimer.Tick += OnHourlyTimerElapsed;
            hourlyTimer.Start();

            exerciseTimer = new System.Windows.Forms.Timer
            {
                Interval = AppConfig.ExerciseTimerInterval
            };
            exerciseTimer.Tick += OnExerciseTimerElapsed;
        }

        // Method to initialize controls
        private void InitializeControls()
        {
            instructionLabel = new Label
            {
                Text = "Eye Training Exercises",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Font = AppConfig.InstructionFont,
                ForeColor = AppConfig.TextColor,
                BackColor = AppConfig.BackgroundColor
            };
            this.Controls.Add(instructionLabel);

            countdownLabel = new Label
            {
                Text = "Time left: 0 seconds",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Font = AppConfig.CountdownFont,
                ForeColor = AppConfig.AccentColor,
                BackColor = AppConfig.BackgroundColor
            };
            this.Controls.Add(countdownLabel);

            drawingPanel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = AppConfig.DrawingBackgroundColor
            };
            drawingPanel.Paint += DrawingPanel_Paint;
            this.Controls.Add(drawingPanel);

            nextButton = new Button
            {
                Text = "Start Exercises",
                FlatStyle = FlatStyle.Flat,
                ForeColor = AppConfig.TextColor,
                BackColor = AppConfig.AccentColor
            };
            nextButton.Click += NextButton_Click;
            this.Controls.Add(nextButton);

            ResizeControls();
        }

        // Event handler for drawing panel paint event
        private void DrawingPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(drawingBitmap, 0, 0);
        }

        // Event handler for hourly timer elapsed event
        private void OnHourlyTimerElapsed(object sender, EventArgs e)
        {
            this.Show();
            this.TopMost = true;
            ResetExercises();
        }

        // Event handler for exercise timer elapsed event
        private void OnExerciseTimerElapsed(object sender, EventArgs e)
        {
            countdown--;
            countdownLabel.Text = $"Time left: {countdown} seconds";

            if (exercises[currentExerciseIndex].DrawAction != null)
            {
                using (Graphics g = Graphics.FromImage(drawingBitmap))
                {
                    g.Clear(AppConfig.DrawingBackgroundColor);
                    exercises[currentExerciseIndex].DrawAction(g, drawingPanel.ClientRectangle, countdown);
                }
                drawingPanel.Invalidate();
            }

            if (countdown <= 0)
            {
                exerciseTimer.Stop();
                ChangeButtonState();
                currentExerciseIndex++;
                if (currentExerciseIndex < exercises.Count)
                {
                    nextButton.Text = "Next Exercise";
                }
                else
                {
                    nextButton.Text = "Finish";
                    this.Hide();
                }
            }
        }

        // Event handler for next button click event
        private void NextButton_Click(object sender, EventArgs e)
        {
            if (currentExerciseIndex < exercises.Count)
            {
                StartNextExercise();
            }
            else
            {
                ResetExercises();
            }
        }

        // Method to change the state of the next button
        public void ChangeButtonState()
        {
            if (nextButton.Enabled == false)
            {
                nextButton.Enabled = true;
                nextButton.BackColor = AppConfig.AccentColor;
            }
            else
            {
                nextButton.Enabled = false;
                nextButton.BackColor = AppConfig.Disabled;
            }
        }

        // Method to start the next exercise
        private void StartNextExercise()
        {
            var exercise = exercises[currentExerciseIndex];
            instructionLabel.Text = $"{exercise.Name}\n{exercise.Instructions}";
            countdown = exercise.DurationSeconds;
            countdownLabel.Text = $"Time left: {countdown} seconds";
            ChangeButtonState();
            ClearDrawingArea();

            drawingPanel.Invalidate();
            exerciseTimer.Start();
        }

        // Method to reset exercises
        private void ResetExercises()
        {
            currentExerciseIndex = 0;
            nextButton.Text = "Start Exercises";
            nextButton.Enabled = true;
            nextButton.BackColor = AppConfig.AccentColor;
            instructionLabel.Text = "Time for eye exercises!\nClick 'Start Exercises' to begin.";
            countdownLabel.Text = "Time left: 0 seconds";
            ClearDrawingArea();
        }

        // Method to clear the drawing area
        private void ClearDrawingArea()
        {
            using (Graphics g = Graphics.FromImage(drawingBitmap))
            {
                g.Clear(AppConfig.DrawingBackgroundColor);
            }
            drawingPanel.Invalidate();
        }

        // Method to draw focus shift exercise
        private void DrawFocusShift(Graphics g, Rectangle bounds, int timeLeft)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int radius = Math.Min(bounds.Width, bounds.Height) / AppConfig.DrawingElementSizeRatio;

            // Increase the horizontal separation between circles
            int separation = (int)(bounds.Width * 0.7); // 70% of the width
            int centerY = bounds.Height / 2;

            // Calculate x-positions for the circles
            int leftX = (bounds.Width - separation) / 2;
            int rightX = leftX + separation;

            DrawPolishedCircle(g, leftX, centerY, radius, Color.FromArgb(200, 103, 152, 188));
            DrawPolishedCircle(g, rightX, centerY, radius, Color.FromArgb(200, 152, 188, 103));
        }

        // Method to draw figure eight exercise
        private void DrawFigureEight(Graphics g, Rectangle bounds, int timeLeft)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int radius = Math.Min(bounds.Width, bounds.Height) / AppConfig.DrawingElementSizeRatio;
            double angle = (AppConfig.ExerciseDuration - timeLeft) * (2 * Math.PI / AppConfig.ExerciseDuration);
            int x = (int)(bounds.Width / 2 + bounds.Width / 3 * Math.Sin(angle));
            int y = (int)(bounds.Height / 2 + bounds.Height / 4 * Math.Sin(2 * angle));

            DrawPolishedCircle(g, x, y, radius, Color.FromArgb(200, 188, 103, 152));
        }

        // Method to draw diagonal stretch exercise
        private void DrawDiagonalStretch(Graphics g, Rectangle bounds, int timeLeft)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int radius = Math.Min(bounds.Width, bounds.Height) / AppConfig.DrawingElementSizeRatio;
            double progress = (AppConfig.ExerciseDuration - timeLeft) % 8 / 8.0;
            int x, y;

            if (progress < 0.25)
            {
                x = (int)(bounds.Width * progress * 4);
                y = (int)(bounds.Height * progress * 4);
            }
            else if (progress < 0.5)
            {
                x = (int)(bounds.Width * (1 - (progress - 0.25) * 4));
                y = (int)(bounds.Height * (progress - 0.25) * 4);
            }
            else if (progress < 0.75)
            {
                x = (int)(bounds.Width * (progress - 0.5) * 4);
                y = (int)(bounds.Height * (1 - (progress - 0.5) * 4));
            }
            else
            {
                x = (int)(bounds.Width * (1 - (progress - 0.75) * 4));
                y = (int)(bounds.Height * (1 - (progress - 0.75) * 4));
            }

            DrawPolishedCircle(g, x, y, radius, Color.FromArgb(200, 152, 103, 188));
        }

        // Method to draw circular motion exercise
        private void DrawCircularMotion(Graphics g, Rectangle bounds, int timeLeft)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int radius = Math.Min(bounds.Width, bounds.Height) / AppConfig.DrawingElementSizeRatio;
            double angle = (AppConfig.ExerciseDuration - timeLeft) * (2 * Math.PI / AppConfig.ExerciseDuration);
            int x = (int)(bounds.Width / 2 + bounds.Width / 3 * Math.Cos(angle));
            int y = (int)(bounds.Height / 2 + bounds.Height / 3 * Math.Sin(angle));

            DrawPolishedCircle(g, x, y, radius, Color.FromArgb(200, 188, 152, 103));
        }

        // Method to draw a polished circle with gradient and subtle glow
        private void DrawPolishedCircle(Graphics g, int centerX, int centerY, int radius, Color baseColor)
        {
            // Create gradient brush
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(centerX - radius, centerY - radius, radius * 2, radius * 2);
                using (PathGradientBrush brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb(255, baseColor);
                    brush.SurroundColors = new Color[] { Color.FromArgb(100, baseColor) };
                    g.FillEllipse(brush, centerX - radius, centerY - radius, radius * 2, radius * 2);
                }
            }

            // Add subtle glow
            using (GraphicsPath glowPath = new GraphicsPath())
            {
                glowPath.AddEllipse(centerX - radius - 2, centerY - radius - 2, (radius + 2) * 2, (radius + 2) * 2);
                using (PathGradientBrush glowBrush = new PathGradientBrush(glowPath))
                {
                    glowBrush.CenterColor = Color.FromArgb(50, baseColor);
                    glowBrush.SurroundColors = new Color[] { Color.FromArgb(0, baseColor) };
                    g.FillEllipse(glowBrush, centerX - radius - 2, centerY - radius - 2, (radius + 2) * 2, (radius + 2) * 2);
                }
            }
        }
    }
}
