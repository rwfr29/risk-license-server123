using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RISKHackTool
{
    /// <summary>
    /// A modern, rounded-corner license activation dialog.
    /// Fully integrated with KeyValidator system.
    /// </summary>
    public partial class LicenseActivationForm : Form
    {
        // ---- Palette
        private static readonly Color BgColor = Color.FromArgb(20, 20, 20);
        private static readonly Color CardColor = Color.FromArgb(35, 35, 35);
        private static readonly Color FieldColor = Color.FromArgb(35, 35, 35);
        private static readonly Color FieldFocusColor = Color.FromArgb(45, 45, 45);
        private static readonly Color AccentColor = Color.FromArgb(45, 45, 45);
        private static readonly Color AccentHover = Color.FromArgb(60, 60, 60);
        private static readonly Color SuccessColor = Color.FromArgb(46, 196, 122);
        private static readonly Color ErrorColor = Color.FromArgb(235, 87, 87);
        private static readonly Color WarnColor = Color.FromArgb(240, 178, 80);
        private static readonly Color TextPrimary = Color.White;
        private static readonly Color TextSecondary = Color.FromArgb(180, 180, 180);
        private static readonly Color TextMuted = Color.FromArgb(110, 110, 110);
        private static readonly Color BorderColor = Color.FromArgb(50, 50, 50);

        private const int CornerRadius = 14;

        private bool _activated = false;
        private KeyData _activationData = null;
        private bool _isDragging = false;
        private Point _dragStartPoint = Point.Empty;

        private TextBox _keyTextBox;
        private RoundedButton _activateBtn;
        private Label _statusIcon;
        private Label _statusText;
        private Panel _statusBanner;
        private Panel _fieldContainer;
        private System.Windows.Forms.Timer _dotsTimer;
        private int _dotsFrame = 0;

        public LicenseActivationForm()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
            BuildUi();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            ClientSize = new Size(400, 380);
            Name = "LicenseActivationForm";
            Text = "Activation";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = BgColor;
            DoubleBuffered = true;
            KeyPreview = true;
            ResumeLayout(false);
        }

        private void BuildUi()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(28, 22, 28, 20),
                ColumnCount = 1,
                RowCount = 7
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 14));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            root.Controls.Add(BuildHeader(), 0, 0);
            root.Controls.Add(BuildKeyField(), 0, 3);
            root.Controls.Add(BuildStatusBanner(), 0, 4);
            root.Controls.Add(BuildFooterArea(), 0, 6);

            Controls.Add(root);

            EnableDragging(root);
            foreach (Control c in root.Controls) EnableDragging(c);
        }

        private Control BuildHeader()
        {
            var panel = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = Color.Transparent };

            var badge = new Panel
            {
                Size = new Size(46, 46),
                Location = new Point(0, 2),
                BackColor = Color.Transparent
            };
            badge.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var surfaceBrush = new SolidBrush(BgColor))
                    e.Graphics.FillRectangle(surfaceBrush, badge.ClientRectangle);
                using (var path = RoundedRect(new Rectangle(0, 0, badge.Width - 1, badge.Height - 1), 12))
                using (var brush = new LinearGradientBrush(badge.ClientRectangle, AccentColor, Color.FromArgb(43, 139, 255), 45f))
                {
                    e.Graphics.FillPath(brush, path);
                }
                TextRenderer.DrawText(e.Graphics, "🔑", new Font("Segoe UI Emoji", 18F), badge.ClientRectangle,
                    Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            var title = new Label
            {
                Text = "Software Activation",
                Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold),
                ForeColor = TextPrimary,
                AutoSize = true,
                Location = new Point(60, 2)
            };
            var subtitle = new Label
            {
                Text = "Enter your license key to unlock this app",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = TextSecondary,
                AutoSize = true,
                Location = new Point(60, 27)
            };

            var closeBtn = new Label
            {
                Text = "✕",
                Font = new Font("Segoe UI", 12F),
                ForeColor = TextMuted,
                Size = new Size(28, 28),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(panel.Width - 30, 4)
            };
            closeBtn.MouseEnter += (s, e) => closeBtn.ForeColor = TextPrimary;
            closeBtn.MouseLeave += (s, e) => closeBtn.ForeColor = TextMuted;
            closeBtn.Click += (s, e) => Close();

            panel.Controls.Add(badge);
            panel.Controls.Add(title);
            panel.Controls.Add(subtitle);
            panel.Controls.Add(closeBtn);
            panel.Resize += (s, e) => closeBtn.Location = new Point(panel.Width - 30, 4);
            return panel;
        }

        private Control BuildKeyField()
        {
            var wrap = new Panel { Dock = DockStyle.Top, Height = 78, BackColor = Color.Transparent };

            var keyLabel = new Label
            {
                Text = "License Key",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = TextPrimary,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            _fieldContainer = new RoundedPanel(10)
            {
                Location = new Point(0, 22),
                Size = new Size(wrap.Width, 42),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                FillColor = FieldColor,
                BorderColor = BorderColor,
                SurfaceColor = BgColor
            };

            _keyTextBox = new TextBox
            {
                Font = new Font("Consolas", 11F),
                ForeColor = TextPrimary,
                BackColor = FieldColor,
                BorderStyle = BorderStyle.None,
                Location = new Point(14, 11),
                Size = new Size(_fieldContainer.Width - 100, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                PlaceholderText = "XXXX-XXXX-XXXX-XXXX"
            };
            _keyTextBox.GotFocus += (s, e) =>
            {
                _keyTextBox.BackColor = FieldFocusColor;
                _fieldContainer.Invalidate();
            };
            _keyTextBox.LostFocus += (s, e) =>
            {
                _keyTextBox.BackColor = FieldColor;
                _fieldContainer.Invalidate();
            };

            var pasteBtn = new LinkLabel
            {
                Text = "Paste",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                LinkColor = AccentHover,
                ActiveLinkColor = AccentColor,
                LinkBehavior = LinkBehavior.NeverUnderline,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(_fieldContainer.Width - 55, 13)
            };
            pasteBtn.Click += (s, e) =>
            {
                if (Clipboard.ContainsText())
                {
                    _keyTextBox.Text = Clipboard.GetText().Trim();
                    _keyTextBox.SelectAll();
                    _keyTextBox.Focus();
                }
            };

            _fieldContainer.Controls.Add(_keyTextBox);
            _fieldContainer.Controls.Add(pasteBtn);
            wrap.Controls.Add(_fieldContainer);
            wrap.Controls.Add(keyLabel);
            return wrap;
        }

        private Control BuildStatusBanner()
        {
            _statusBanner = new RoundedPanel(8)
            {
                Dock = DockStyle.Top,
                Height = 0,
                Visible = false,
                FillColor = Color.FromArgb(40, WarnColor),
                BorderColor = Color.Transparent,
                SurfaceColor = BgColor,
                Margin = new Padding(0, 8, 0, 0)
            };
            _statusIcon = new Label
            {
                Text = "!",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(12, 8)
            };
            _statusText = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9F),
                AutoSize = true,
                Location = new Point(32, 8)
            };
            _statusBanner.Controls.Add(_statusIcon);
            _statusBanner.Controls.Add(_statusText);
            return _statusBanner;
        }

        private Control BuildFooterArea()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 3

            };

            var infoLabel = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8.25F),
                ForeColor = TextMuted,
                AutoSize = true,
                Margin = new Padding(0, 6, 0, 12)
            };

            var buttonRow = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0)
            };

            _activateBtn = new RoundedButton
            {
                Text = "Activate",
                Size = new Size(120, 42),
                FillColor = AccentColor,
                HoverColor = AccentHover,
                PressedColor = Color.FromArgb(50, 110, 210),
                TextColor = Color.White,
                SurfaceColor = BgColor,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Margin = new Padding(0, 14, 0, 0)
            };
            _activateBtn.Click += (s, e) => TryActivate();

            buttonRow.Controls.Add(_activateBtn);
            buttonRow.Anchor = AnchorStyles.None;
            buttonRow.Dock = DockStyle.None;
            buttonRow.AutoSize = true;
            var copyright = new Label
            {
                Text = "© 2026 All rights reserved",
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = Color.FromArgb(70, 70, 90),
                AutoSize = true,
                Margin = new Padding(0, 14, 0, 0),
                Anchor = AnchorStyles.None
            };

            panel.Controls.Add(infoLabel, 0, 0);
            panel.Controls.Add(buttonRow, 0, 1);
            panel.Controls.Add(copyright, 0, 2);
            return panel;
        }

        private void EnableDragging(Control c)
        {
            c.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    _isDragging = true;
                    _dragStartPoint = e.Location;
                }
            };
            c.MouseMove += (s, e) =>
            {
                if (_isDragging)
                {
                    var screenPt = ((Control)s).PointToScreen(e.Location);
                    Location = new Point(screenPt.X - _dragStartPoint.X, screenPt.Y - _dragStartPoint.Y);
                }
            };
            c.MouseUp += (s, e) => _isDragging = false;
        }

        private void ShowStatus(string message, Color color, string icon)
        {
            _statusIcon.ForeColor = color;
            _statusText.ForeColor = color;
            _statusIcon.Text = icon;
            _statusText.Text = message;

            _statusBanner.Height = 34;
            _statusBanner.Visible = true;
            _statusBanner.Invalidate();
        }

        private void HideStatus()
        {
            _statusBanner.Visible = false;
            _statusBanner.Height = 0;
        }

        private async void TryActivate()
        {
            string key = _keyTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(key))
            {
                ShowStatus("Please enter a license key", WarnColor, "!");
                _keyTextBox.Focus();
                return;
            }

            HideStatus();
            _activateBtn.Enabled = false;
            _activateBtn.FillColor = Color.FromArgb(60, 60, 78);
            StartCheckingAnimation();

            KeyStatus status;
            try
            {
                status = await KeyValidator.Instance.ValidateKeyAsync(key);
            }
            finally
            {
                StopCheckingAnimation();
            }

            _activateBtn.Text = "Activate";
            _activateBtn.FillColor = AccentColor;
            _activateBtn.Enabled = true;

            if (!status.IsValid)
            {
                ShowStatus(status.Message ?? "Activation failed", ErrorColor, "✕");
                _keyTextBox.SelectAll();
                _keyTextBox.Focus();
                return;
            }

            if (status.IsActivated && status.IsExpired)
            {
                ShowStatus($"Expired on {status.ExpirationDate?.ToShortDateString()}", ErrorColor, "⏰");
                return;
            }

            if (status.IsActivated)
            {
                _activated = true;
                _activationData = KeyValidator.Instance.GetCurrentActivation();

                string remaining = status.RemainingDays >= 9999
                    ? "Lifetime"
                    : $"{Math.Floor(status.RemainingDays)} days remaining";
                ShowStatus($"Activated! {remaining}", SuccessColor, "✓");

                _activateBtn.Text = "Activated ✓";
                _activateBtn.FillColor = SuccessColor;
                _activateBtn.Enabled = false;
                _keyTextBox.Enabled = false;

                await Task.Delay(800);
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            ShowStatus("Activation failed. Please try again.", ErrorColor, "✕");
        }

        private void StartCheckingAnimation()
        {
            _dotsFrame = 0;
            _dotsTimer = new System.Windows.Forms.Timer { Interval = 250 };
            _dotsTimer.Tick += (s, e) =>
            {
                _dotsFrame = (_dotsFrame + 1) % 4;
                _activateBtn.Text = "Checking" + new string('.', _dotsFrame);
            };
            _dotsTimer.Start();
        }

        private void StopCheckingAnimation()
        {
            _dotsTimer?.Stop();
            _dotsTimer?.Dispose();
            _dotsTimer = null;
        }

        protected override void OnPaint(PaintEventArgs e)
        { base.OnPaint(e); }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            using (var path = RoundedRect(new Rectangle(0, 0, ClientSize.Width, ClientSize.Height), CornerRadius))
            {
                Region = new Region(path);
            }
        }

        public bool IsActivated() => _activated;
        public KeyData GetActivationData() => _activationData;

        internal static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    internal class RoundedPanel : Panel
    {
        public int Radius { get; set; }
        public Color FillColor { get; set; } = Color.FromArgb(40, 40, 54);
        public Color BorderColor { get; set; } = Color.Transparent;
        public Color SurfaceColor { get; set; } = Color.FromArgb(24, 24, 32);

        public RoundedPanel(int radius)
        {
            Radius = radius;
            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                      ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var surfaceBrush = new SolidBrush(SurfaceColor))
                e.Graphics.FillRectangle(surfaceBrush, ClientRectangle);

            var bounds = new Rectangle(0, 0, Width - 1, Math.Max(Height - 1, 1));
            using (var path = LicenseActivationForm.RoundedRect(bounds, Radius))
            {
                using (var brush = new SolidBrush(FillColor)) e.Graphics.FillPath(brush, path);
                if (BorderColor != Color.Transparent)
                    using (var pen = new Pen(BorderColor)) e.Graphics.DrawPath(pen, path);
            }
        }
    }

    internal class RoundedButton : Control, IButtonControl
    {
        public Color FillColor { get; set; } = Color.FromArgb(66, 135, 245);
        public Color HoverColor { get; set; } = Color.FromArgb(90, 155, 255);
        public Color PressedColor { get; set; } = Color.FromArgb(50, 110, 210);
        public Color TextColor { get; set; } = Color.White;
        public Color DisabledFillColor { get; set; } = Color.FromArgb(60, 60, 78);
        public Color DisabledTextColor { get; set; } = Color.FromArgb(150, 150, 165);
        public Color SurfaceColor { get; set; } = Color.FromArgb(24, 24, 32);
        public int CornerRadius { get; set; } = 10;

        public DialogResult DialogResult { get; set; } = DialogResult.None;

        private bool _hover = false;
        private bool _pressed = false;

        public RoundedButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                      ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                      ControlStyles.SupportsTransparentBackColor, true);
            Cursor = Cursors.Hand;
            TabStop = true;
            Size = new Size(140, 40);

            MouseEnter += (s, e) => { _hover = true; Invalidate(); };
            MouseLeave += (s, e) => { _hover = false; _pressed = false; Invalidate(); };
            MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { _pressed = true; Invalidate(); } };
            MouseUp += (s, e) => { _pressed = false; Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var surfaceBrush = new SolidBrush(SurfaceColor))
                g.FillRectangle(surfaceBrush, ClientRectangle);

            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            Color fill = !Enabled ? DisabledFillColor : (_pressed ? PressedColor : (_hover ? HoverColor : FillColor));
            Color text = !Enabled ? DisabledTextColor : TextColor;

            using (var path = LicenseActivationForm.RoundedRect(bounds, CornerRadius))
            {
                if (fill != Color.Transparent)
                {
                    using (var brush = new SolidBrush(fill)) g.FillPath(brush, path);
                }
                else if (_hover)
                {
                    using (var brush = new SolidBrush(HoverColor)) g.FillPath(brush, path);
                }
            }

            TextRenderer.DrawText(g, Text, Font, bounds, text,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        protected override void OnClick(EventArgs e)
        {
            if (!Enabled) return;
            base.OnClick(e);
        }

        public void NotifyDefault(bool value) { }
        public void PerformClick() => OnClick(EventArgs.Empty);
    }
}