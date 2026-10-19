using RISKHackTool.src;
using RISKHackTool.src.PlayerCardsForm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;


namespace RISKHackTool
{
    public partial class RISKHack : Form
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        private Dictionary<String, IntPtr> territoryNamesToAdresses = new Dictionary<String, IntPtr>();
        private IntPtr addrOfTerritoriesList = IntPtr.Zero;
        private int sizeOfTerritoriesList = 0;

        private Dictionary<String, IntPtr> playerColorsToAddresses = new Dictionary<String, IntPtr>();
        private Dictionary<String, IntPtr> regionNamesToAddresses = new Dictionary<String, IntPtr>();

        private byte[] buffer = new byte[Constants.MAX_BUFFER_SIZE];
        private IntPtr bytesRead = IntPtr.Zero;

        private Process riskProcess;
        private ProcessModule? gameAssemblyDllProc;
        private bool isLoading = false;

        private IntPtr cachedGameManagerStaticFields = IntPtr.Zero;
        private IntPtr cachedMapManagerStaticFields = IntPtr.Zero;

        private static System.Threading.Mutex mutex = null;

        private BackgroundWorker refreshWorker;


        public RISKHack()
        {
            InitializeComponent();
            EnableDoubleBuffering(this);
            InitializeRefreshWorker();

            bool createdNew;
            mutex = new System.Threading.Mutex(true, "RISKHackToolMutex", out createdNew);

            if (!createdNew)
            {
                ActivateExistingInstance();
                Environment.Exit(0);
                return;
            }

            // License validation
            if (!ValidateLicense())
            {
                Environment.Exit(0);
                return;
            }

            SetupCustomUIEvents();
            // ...

            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;

            isLoading = true;
            backgroundWorker.RunWorkerAsync();
        }

        private bool ValidateLicense()
        {
            if (KeyValidator.Instance.IsValidated())
                return true;

            if (KeyValidator.Instance.TryBootstrap(out _))
                return true;

            using (LicenseActivationForm activationForm = new LicenseActivationForm())
            {
                DialogResult result = activationForm.ShowDialog();
                return result == DialogResult.OK && activationForm.IsActivated();
            }
        }


        // ==========================================
        // REFRESH WORKER METHODS
        // ==========================================

        private void InitializeRefreshWorker()
        {
            refreshWorker = new BackgroundWorker();
            refreshWorker.WorkerReportsProgress = true;
            refreshWorker.WorkerSupportsCancellation = true;

            refreshWorker.DoWork += RefreshWorker_DoWork;
            refreshWorker.RunWorkerCompleted += RefreshWorker_RunWorkerCompleted;

            if (autoRefreshWorker == null)
            {
                autoRefreshWorker = new BackgroundWorker();
            }
            autoRefreshWorker.WorkerReportsProgress = true;
            autoRefreshWorker.WorkerSupportsCancellation = true;
            autoRefreshWorker.DoWork += AutoRefreshWorker_DoWork;
            autoRefreshWorker.RunWorkerCompleted += AutoRefreshWorker_RunWorkerCompleted;
        }

        private void btnRefreshMatchData_Click(object sender, EventArgs e)
        {
            if (refreshWorker != null && refreshWorker.IsBusy)
            {
                return;
            }

            var btn = sender as Button;
            if (btn != null) btn.Enabled = false;

            if (refreshWorker != null)
            {
                refreshWorker.RunWorkerAsync();
            }
        }

        private void RefreshWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                RefreshMatchDataInternal();
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void RefreshWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                var btn = this.Controls.Find("btnRefreshMatchData", true).FirstOrDefault() as Button;
                if (btn != null) btn.Enabled = true;

                if (e.Error != null)
                {
                    MessageBox.Show($"Error during refresh: {e.Error.Message}", "Refresh Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (e.Result is Exception ex)
                {
                    MessageBox.Show($"Refresh failed: {ex.Message}", "Refresh Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show($"UI Update Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshMatchDataInternal()
        {
            if (riskProcess == null || riskProcess.HasExited) return;

            cachedGameManagerStaticFields = IntPtr.Zero;
            cachedMapManagerStaticFields = IntPtr.Zero;
            GetRISKData();
        }

        private void EnableDoubleBuffering(Control control)
        {
            typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(control, true, null);
            foreach (Control c in control.Controls)
            {
                EnableDoubleBuffering(c);
            }
        }

        private void SetupCustomUIEvents()
        {
            SetupCustomTitleBar();
            RemoveTopBarButtons();
            StylePlayerDropdown();

            btnNavTerritories.Click += (s, e) => SwitchView(0);
            btnNavPlayers.Click += (s, e) => SwitchView(1);
            btnNavActions.Click += (s, e) => SwitchView(2);

            AttachSidebarHover(btnNavTerritories);
            AttachSidebarHover(btnNavPlayers);
            AttachSidebarHover(btnNavActions);

            txtSearch.GotFocus += (s, e) => {
                if (txtSearch.Text == "Search territories or players...") txtSearch.Text = "";
            };
            txtSearch.LostFocus += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtSearch.Text)) txtSearch.Text = "Search territories or players...";
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            cmbPlayerFilter.SelectedIndexChanged += CmbPlayerFilter_SelectedIndexChanged;

            var btnShow = this.Controls.Find("btnShowSelectedPlayerCards", true).FirstOrDefault() as Button;
            if (btnShow != null) btnShow.Click += (s, e) => ExecuteOnSelectedPlayer(ShowPlayerCards);

            var btnEdit = this.Controls.Find("btnEditSelectedPlayerCards", true).FirstOrDefault() as Button;
            if (btnEdit != null) btnEdit.Click += (s, e) => ExecuteOnSelectedPlayer(EditPlayerCards);

            SetupActionsTab();
        }

        private void StylePlayerDropdown()
        {
            if (cmbPlayerFilter == null) return;

            // Clean, native flat style matching dark menu bar
            cmbPlayerFilter.FlatStyle = FlatStyle.Flat;
            cmbPlayerFilter.BackColor = Color.FromArgb(35, 35, 35);
            cmbPlayerFilter.ForeColor = Color.White;
            cmbPlayerFilter.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            cmbPlayerFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPlayerFilter.DrawMode = DrawMode.Normal; // Uses standard native rendering
        }

        private void SetupCustomTitleBar()
        {
            this.FormBorderStyle = FormBorderStyle.None;

            Panel pnlTitleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(20, 20, 20),
                Name = "pnlCustomTitleBar"
            };

            Label lblTitle = new Label
            {
                Text = "DefyNation",
                ForeColor = Color.FromArgb(230, 230, 230),
                Font = new Font("Segoe UI", 14.0F, FontStyle.Regular),
                AutoSize = true
            };

            pnlTitleBar.SizeChanged += (s, e) =>
            {
                lblTitle.Location = new Point(
                    (pnlTitleBar.Width - lblTitle.Width) / 2,
                    (pnlTitleBar.Height - lblTitle.Height) / 2
                );
            };

            Panel pnlWindowControls = new Panel
            {
                Dock = DockStyle.Right,
                Width = 135,
                Height = 36
            };

            Button btnMinimize = CreateTitleBarButton("─", Color.FromArgb(60, 60, 60), new Point(0, 0));
            Button btnMaximize = CreateTitleBarButton("🗖", Color.FromArgb(60, 60, 60), new Point(45, 0));
            Button btnClose = CreateTitleBarButton("✕", Color.FromArgb(232, 17, 35), new Point(90, 0));

            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            btnMaximize.Click += (s, e) => {
                this.WindowState = (this.WindowState == FormWindowState.Maximized)
                    ? FormWindowState.Normal
                    : FormWindowState.Maximized;
            };
            btnClose.Click += (s, e) => this.Close();

            pnlWindowControls.Controls.Add(btnMinimize);
            pnlWindowControls.Controls.Add(btnMaximize);
            pnlWindowControls.Controls.Add(btnClose);

            pnlTitleBar.Controls.Add(lblTitle);
            pnlTitleBar.Controls.Add(pnlWindowControls);

            MouseEventHandler dragHandler = (s, e) => {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };

            pnlTitleBar.MouseDown += dragHandler;
            lblTitle.MouseDown += dragHandler;

            this.Controls.Add(pnlTitleBar);
            pnlTitleBar.SendToBack();
        }

        private Button CreateTitleBarButton(string text, Color hoverColor, Point location)
        {
            Button btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Size = new Size(45, 36),
                Location = location,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(20, 20, 20),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(20, 20, 20);
            return btn;
        }

        private void RemoveTopBarButtons()
        {
            List<Control> toRemove = new List<Control>();
            FindHeaderButtons(this, toRemove);

            foreach (Control ctrl in toRemove)
            {
                ctrl.Parent?.Controls.Remove(ctrl);
                ctrl.Dispose();
            }
        }

        private void FindHeaderButtons(Control parent, List<Control> list)
        {
            foreach (Control c in parent.Controls)
            {
                if (c == pnlActionsView) continue;

                if (c is Button btn && (
                    btn.Text.Contains("Recolor") ||
                    btn.Text.Contains("CSV") ||
                    btn.Text.Contains("Toggle Fog") ||
                    btn.Text.Contains("Refresh")))
                {
                    list.Add(btn);
                }
                else if (c.HasChildren)
                {
                    FindHeaderButtons(c, list);
                }
            }
        }

        private void SetupActionsTab()
        {
            if (pnlActionsView == null) return;

            pnlActionsView.Controls.Clear();

            Button btnRecolor = CreateActionButton("🎨 Update Colors ", 12, 20, (s, e) => {
                UpdateTerritoryColors();
            });

            Button btnToggleFog = CreateActionButton("🌫️ Toggle Fog of War", 12, 70, toggleFogButton_Click);
            Button btnRefresh = CreateActionButton("🔄 Refresh Match Data", 12, 120, btnRefreshMatchData_Click);
            btnRefresh.Name = "btnRefreshMatchData";

            pnlActionsView.Controls.Add(btnRecolor);
            pnlActionsView.Controls.Add(btnToggleFog);
            pnlActionsView.Controls.Add(btnRefresh);
        }

        private Button CreateActionButton(string text, int posX, int posY, EventHandler onClick)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btn.Location = new Point(posX, posY);
            btn.Size = new Size(460, 40);
            btn.Click += onClick;
            btn.BackColor = Color.FromArgb(45, 45, 45);
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;

            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(60, 60, 60);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(45, 45, 45);

            return btn;
        }

        private void AttachSidebarHover(Button btn)
        {
            btn.MouseEnter += (s, e) => {
                if (btn.ForeColor != Color.White) btn.BackColor = Color.FromArgb(35, 35, 35);
            };
            btn.MouseLeave += (s, e) => {
                if (btn.ForeColor != Color.White) btn.BackColor = Color.Transparent;
            };
        }

        private void SwitchView(int index)
        {
            pnlTerritoriesView.Visible = (index == 0);
            pnlPlayersView.Visible = (index == 1);
            pnlActionsView.Visible = (index == 2);
            pnlShowCardsView.Visible = (index == 3);

            // Hide search bar ONLY on non-Territories tabs
            pnlSearchHeader.Visible = (index == 0);

            Button[] navBtns = new[] { btnNavTerritories, btnNavPlayers, btnNavActions };
            for (int i = 0; i < navBtns.Length; i++)
            {
                if (i == index)
                {
                    navBtns[i].ForeColor = Color.White;
                    navBtns[i].Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                    navBtns[i].BackColor = Color.FromArgb(35, 35, 35);
                    pnlActiveIndicator.Top = navBtns[i].Top;
                }
                else
                {
                    navBtns[i].ForeColor = Color.Gray;
                    navBtns[i].Font = new Font("Segoe UI", 9.5F);
                    navBtns[i].BackColor = Color.Transparent;
                }
            }
        }

        private void CmbPlayerFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ApplyTerritoryFilter();
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            // Search applies exclusively to territories
            ApplyTerritoryFilter();
        }

        private void ApplyTerritoryFilter()
        {
            if (cmbPlayerFilter == null || cmbPlayerFilter.Items.Count == 0) return;

            string query = txtSearch.Text.Trim().ToLower();
            if (query == "search territories or players...") query = "";

            string selectedPlayer = cmbPlayerFilter.SelectedItem?.ToString() ?? "All Players";

            territoriesFlowPanel.SuspendLayout();
            foreach (Control ctrl in territoriesFlowPanel.Controls)
            {
                if (ctrl is Panel itemPanel)
                {
                    string territoryName = itemPanel.Name;
                    bool matchesSearch = string.IsNullOrEmpty(query) || territoryName.ToLower().Contains(query);
                    bool matchesPlayer = true;

                    if (selectedPlayer != "All Players" && territoryNamesToAdresses.ContainsKey(territoryName))
                    {
                        IntPtr territoryAddr = territoryNamesToAdresses[territoryName];
                        IntPtr ownerAddr = GetPlayerAddrFromTerritoryAddr(territoryAddr);

                        string cleanSelectedKey = selectedPlayer.ToLower();
                        if (playerColorsToAddresses.ContainsKey(cleanSelectedKey))
                        {
                            IntPtr targetPlayerAddr = playerColorsToAddresses[cleanSelectedKey];
                            matchesPlayer = (ownerAddr == targetPlayerAddr && ownerAddr != IntPtr.Zero);
                        }
                        else
                        {
                            string ownerColorName = GetColorNameFromPlayer(ownerAddr).Replace("color_", "");
                            matchesPlayer = string.Equals(ownerColorName, cleanSelectedKey, StringComparison.OrdinalIgnoreCase);
                        }
                    }

                    itemPanel.Visible = matchesSearch && matchesPlayer;
                }
            }
            territoriesFlowPanel.ResumeLayout();
        }

        private void ShowPlayerCards(string playerColor)
        {
            OpenPlayerCardsForm(playerColor, isReadOnly: true);
        }

        private void EditPlayerCards(string playerColor)
        {
            OpenPlayerCardsForm(playerColor, isReadOnly: false);
        }

        private void OpenPlayerCardsForm(string playerColor, bool isReadOnly)
        {
            try
            {
                string cleanKey = playerColor.ToLower().Replace("color_", "");

                if (!playerColorsToAddresses.ContainsKey(cleanKey))
                {
                    MessageBox.Show($"Player color '{playerColor}' not found in active match.", "Player Cards Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                IntPtr playerPtr = playerColorsToAddresses[cleanKey];

                pnlShowCardsView.Controls.Clear();
                pnlShowCardsView.BackColor = Color.FromArgb(30, 30, 30);

                PlayerCardsForm cardsForm = new PlayerCardsForm(cleanKey, playerPtr, riskProcess)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill,
                    Visible = true,
                    BackColor = Color.FromArgb(30, 30, 30)
                };

                if (isReadOnly)
                {
                    foreach (Control c in cardsForm.Controls)
                    {
                        if (c is Button b && b.DialogResult != DialogResult.Cancel && b.Text.ToLower().Contains("save"))
                        {
                            b.Enabled = false;
                        }
                    }
                }

                pnlShowCardsView.Controls.Add(cardsForm);
                SwitchView(3);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error launching Player Cards menu: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetTerritoryTroopsCount(string territoryName)
        {
            try
            {
                if (riskProcess == null || riskProcess.HasExited) return 1;

                if (territoryNamesToAdresses.TryGetValue(territoryName, out IntPtr baseAddress) && baseAddress != IntPtr.Zero)
                {
                    byte[] buffer = new byte[8];
                    int[] troopOffsets = new int[] { 0x90, 0x38, 0x40, 0x48, 0x50, 0x58 };

                    foreach (int offset in troopOffsets)
                    {
                        if (HelperFunctions.ReadProcessMemory(riskProcess.Handle, IntPtr.Add(baseAddress, offset), buffer, 4, out bytesRead))
                        {
                            int val = BitConverter.ToInt32(buffer, 0);
                            if (val > 0 && val < 10000) return val;
                        }
                    }

                    int[] ptrOffsets = new int[] { 0x10, 0x18, 0x20 };
                    foreach (int ptrOffset in ptrOffsets)
                    {
                        if (HelperFunctions.ReadProcessMemory(riskProcess.Handle, IntPtr.Add(baseAddress, ptrOffset), buffer, 8, out bytesRead))
                        {
                            IntPtr derefPtr = (IntPtr)BitConverter.ToInt64(buffer, 0);
                            if (derefPtr != IntPtr.Zero && derefPtr.ToInt64() > 0x10000)
                            {
                                foreach (int offset in troopOffsets)
                                {
                                    if (HelperFunctions.ReadProcessMemory(riskProcess.Handle, IntPtr.Add(derefPtr, offset), buffer, 4, out bytesRead))
                                    {
                                        int val = BitConverter.ToInt32(buffer, 0);
                                        if (val > 0 && val < 10000) return val;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            return 1;
        }

        private void ExecuteOnSelectedPlayer(Action<string> cardAction)
        {
            string selectedPlayer = cmbPlayerFilter.SelectedItem?.ToString() ?? "";
            if (string.IsNullOrEmpty(selectedPlayer) || selectedPlayer == "All Players")
            {
                MessageBox.Show("Please select a specific player from the player dropdown menu first.", "Select Player", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            cardAction(selectedPlayer.ToLower());
        }

        private IntPtr GetGameManagerStaticFields()
        {
            if (cachedGameManagerStaticFields != IntPtr.Zero)
            {
                byte[] verifyBuffer = new byte[4];
                bool verifySuccess = HelperFunctions.ReadProcessMemory(riskProcess.Handle,
                    IntPtr.Add(cachedGameManagerStaticFields, 0), verifyBuffer, 4, out bytesRead);
                if (verifySuccess)
                {
                    int gameMode = BitConverter.ToInt32(verifyBuffer, 0);
                    if (gameMode >= 0 && gameMode <= 5)
                    {
                        return cachedGameManagerStaticFields;
                    }
                }
                cachedGameManagerStaticFields = IntPtr.Zero;
            }

            IntPtr result = GetStaticFieldsForType(BaseOffsets.GAME_MANAGER_TYPE_INFO_OFFSET);
            if (result != IntPtr.Zero)
            {
                cachedGameManagerStaticFields = result;
            }
            return result;
        }

        private IntPtr GetMapManagerStaticFields()
        {
            if (cachedMapManagerStaticFields != IntPtr.Zero)
            {
                byte[] verifyBuffer = new byte[8];
                bool verifySuccess = HelperFunctions.ReadProcessMemory(riskProcess.Handle,
                    IntPtr.Add(cachedMapManagerStaticFields, 0), verifyBuffer, 8, out bytesRead);
                if (verifySuccess)
                {
                    IntPtr mapPtr = new IntPtr(BitConverter.ToInt64(verifyBuffer, 0));
                    if (mapPtr != IntPtr.Zero && mapPtr.ToInt64() > 0x10000)
                    {
                        return cachedMapManagerStaticFields;
                    }
                }
                cachedMapManagerStaticFields = IntPtr.Zero;
            }

            IntPtr result = GetStaticFieldsForType(BaseOffsets.MAP_MANAGER_TYPE_INFO_OFFSET);
            if (result != IntPtr.Zero)
            {
                cachedMapManagerStaticFields = result;
            }
            return result;
        }

        private IntPtr GetStaticFieldsForType(int typeInfoOffset)
        {
            try
            {
                if (gameAssemblyDllProc == null) return IntPtr.Zero;

                IntPtr typeInfoSlotAddr = IntPtr.Add(gameAssemblyDllProc.BaseAddress, typeInfoOffset);

                byte[] buffer = new byte[8];
                HelperFunctions.ReadProcessMemory(riskProcess.Handle, typeInfoSlotAddr, buffer, 8, out bytesRead);
                IntPtr classPtr = new IntPtr(BitConverter.ToInt64(buffer, 0));

                if (classPtr == IntPtr.Zero) return IntPtr.Zero;

                int[] possibleOffsets = new int[] { 0xB8, 0xC0, 0xC8, 0xD0, 0xD8, 0xE0, 0xE8, 0xF0 };

                foreach (int offset in possibleOffsets)
                {
                    IntPtr staticFieldsAddr = IntPtr.Add(classPtr, offset);
                    byte[] ptrBuffer = new byte[8];

                    bool success = HelperFunctions.ReadProcessMemory(riskProcess.Handle,
                        staticFieldsAddr, ptrBuffer, 8, out bytesRead);

                    if (success)
                    {
                        IntPtr staticFields = new IntPtr(BitConverter.ToInt64(ptrBuffer, 0));
                        if (staticFields != IntPtr.Zero)
                        {
                            byte[] verifyBuffer = new byte[8];
                            bool verifySuccess = HelperFunctions.ReadProcessMemory(riskProcess.Handle,
                                staticFields, verifyBuffer, 8, out bytesRead);

                            if (verifySuccess)
                            {
                                IntPtr verifyPtr = new IntPtr(BitConverter.ToInt64(verifyBuffer, 0));
                                if (verifyPtr != IntPtr.Zero && verifyPtr.ToInt64() > 0x10000)
                                {
                                    return staticFields;
                                }
                            }
                        }
                    }
                }

                return IntPtr.Zero;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        private IntPtr GetMapInstance()
        {
            IntPtr mapManagerStaticFields = GetMapManagerStaticFields();
            if (mapManagerStaticFields == IntPtr.Zero) return IntPtr.Zero;

            byte[] buffer = new byte[8];
            HelperFunctions.ReadProcessMemory(riskProcess.Handle,
                IntPtr.Add(mapManagerStaticFields, 0),
                buffer, 8, out bytesRead);
            return new IntPtr(BitConverter.ToInt64(buffer, 0));
        }

        private void ActivateExistingInstance()
        {
            try
            {
                Process currentProcess = Process.GetCurrentProcess();
                Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);

                foreach (Process process in processes)
                {
                    if (process.Id != currentProcess.Id)
                    {
                        IntPtr hWnd = process.MainWindowHandle;
                        if (hWnd != IntPtr.Zero)
                        {
                            ShowWindow(hWnd, 9);
                            SetForegroundWindow(hWnd);
                            break;
                        }
                    }
                }
            }
            catch { }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (backgroundWorker != null && backgroundWorker.IsBusy)
                {
                    backgroundWorker.CancelAsync();
                }

                if (mutex != null)
                {
                    mutex.ReleaseMutex();
                    mutex.Close();
                }
            }
            catch { }
            base.OnFormClosing(e);
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                GetRISKData();
                (sender as BackgroundWorker).ReportProgress(50);
                SetRISKDataInComponent();
                (sender as BackgroundWorker).ReportProgress(100);
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            try
            {
                if (e.ProgressPercentage == 100)
                {
                    isLoading = false;
                    statusLabel.Text = $"Loaded {territoryNamesToAdresses.Count} territories, {playerColorsToAddresses.Count} players";
                    statusLabel.ForeColor = Color.LightGreen;
                    lblProcessState.Text = "Risk : Connected";
                    lblProcessState.ForeColor = Color.LightGreen;
                }
                else if (e.ProgressPercentage == 50)
                {
                    statusLabel.Text = "Waiting...";
                    statusLabel.ForeColor = Color.Orange;
                }
            }
            catch { }
        }

        private void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                isLoading = false;

                if (e.Error != null)
                {
                    statusLabel.Text = "Error no game running";
                    statusLabel.ForeColor = Color.Salmon;
                    lblProcessState.Text = "IL2CPP Target: Error";
                    lblProcessState.ForeColor = Color.Salmon;
                    return;
                }

                if (!backgroundWorker.CancellationPending && !isLoading)
                {
                    if (autoRefreshWorker != null && !autoRefreshWorker.IsBusy)
                    {
                        autoRefreshWorker.RunWorkerAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in completion: {ex.Message}");
            }
        }

        private void AutoRefreshWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            bool flag = false;
            while (!worker.CancellationPending && !isLoading)
            {
                Thread.Sleep(100);
                worker.ReportProgress(Convert.ToInt32(flag));
                flag = !flag;
            }
            e.Cancel = true;
        }

        private void AutoRefreshWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled && !isLoading)
            {
                if (autoRefreshWorker != null && !autoRefreshWorker.IsBusy)
                {
                    autoRefreshWorker.RunWorkerAsync();
                }
            }
        }

        void GetRISKData()
        {
            Process[] riskProcs = Process.GetProcessesByName("Risk");
            if (riskProcs.Length != 1)
            {
                throw new Exception("Risk process not detected or multiple instances found.");
            }
            riskProcess = riskProcs[0];

            gameAssemblyDllProc = null;
            foreach (ProcessModule module in riskProcess.Modules)
            {
                if (module.FileName != null && module.FileName.Contains("GameAssembly.dll"))
                {
                    gameAssemblyDllProc = module;
                    break;
                }
            }

            if (gameAssemblyDllProc == null)
            {
                throw new Exception("GameAssembly.dll not detected.");
            }

            territoryNamesToAdresses.Clear();
            playerColorsToAddresses.Clear();
            regionNamesToAddresses.Clear();

            IntPtr gameManagerStaticFields = GetGameManagerStaticFields();

            if (gameManagerStaticFields == IntPtr.Zero)
            {
                throw new Exception("GameManager not found.");
            }

            byte[] playerArrayBuffer = new byte[8];
            HelperFunctions.ReadProcessMemory(riskProcess.Handle,
                IntPtr.Add(gameManagerStaticFields, GameManagerOffsets.PLAYERS_OFFSET),
                playerArrayBuffer, 8, out bytesRead);
            IntPtr playerArrayPtr = new IntPtr(BitConverter.ToInt64(playerArrayBuffer, 0));

            if (playerArrayPtr != IntPtr.Zero && playerArrayPtr.ToInt64() > 0x10000)
            {
                byte[] playerCountBuffer = new byte[4];
                HelperFunctions.ReadProcessMemory(riskProcess.Handle,
                    IntPtr.Add(playerArrayPtr, 0x18),
                    playerCountBuffer, 4, out bytesRead);
                int numPlayers = BitConverter.ToInt32(playerCountBuffer, 0);

                for (int playerIdx = 0; playerIdx < numPlayers && playerIdx < 20; playerIdx++)
                {
                    byte[] singlePlayerBuffer = new byte[8];
                    HelperFunctions.ReadProcessMemory(riskProcess.Handle,
                        IntPtr.Add(playerArrayPtr, 0x20 + (playerIdx * 8)),
                        singlePlayerBuffer, 8, out bytesRead);
                    IntPtr playerPtr = new IntPtr(BitConverter.ToInt64(singlePlayerBuffer, 0));

                    if (playerPtr != IntPtr.Zero && playerPtr.ToInt64() > 0x10000)
                    {
                        string colorName = GetColorNameFromPlayer(playerPtr);
                        string playerKey = (!string.IsNullOrEmpty(colorName) && colorName != "color_unknown")
                            ? colorName.Replace("color_", "")
                            : new string[] { "blue", "red", "orange", "yellow", "pink", "black", "white" }[playerIdx % 7];

                        if (!playerColorsToAddresses.ContainsKey(playerKey))
                        {
                            playerColorsToAddresses.Add(playerKey, playerPtr);
                        }
                    }
                }
            }

            IntPtr mapInstance = GetMapInstance();

            if (mapInstance == IntPtr.Zero || mapInstance.ToInt64() <= 0x10000)
            {
                throw new Exception("Map not found.");
            }

            byte[] territoriesPtrBuffer = new byte[8];
            HelperFunctions.ReadProcessMemory(riskProcess.Handle,
                IntPtr.Add(mapInstance, 0x38),
                territoriesPtrBuffer, 8, out bytesRead);
            addrOfTerritoriesList = new IntPtr(BitConverter.ToInt64(territoriesPtrBuffer, 0));

            if (addrOfTerritoriesList == IntPtr.Zero || addrOfTerritoriesList.ToInt64() <= 0x10000)
            {
                throw new Exception("Territories list missing.");
            }

            byte[] territorySizeBuffer = new byte[4];
            HelperFunctions.ReadProcessMemory(riskProcess.Handle,
                IntPtr.Add(addrOfTerritoriesList, 0x18),
                territorySizeBuffer, 4, out bytesRead);
            sizeOfTerritoriesList = BitConverter.ToInt32(territorySizeBuffer, 0);

            for (int terrIdx = 0; terrIdx < sizeOfTerritoriesList && terrIdx < 200; terrIdx++)
            {
                try
                {
                    byte[] terrPtrBuffer = new byte[8];
                    HelperFunctions.ReadProcessMemory(riskProcess.Handle,
                        IntPtr.Add(addrOfTerritoriesList, 0x20 + (terrIdx * 8)),
                        terrPtrBuffer, 8, out bytesRead);
                    IntPtr territoryAddress = new IntPtr(BitConverter.ToInt64(terrPtrBuffer, 0));

                    if (territoryAddress == IntPtr.Zero || territoryAddress.ToInt64() <= 0x10000) continue;

                    byte[] namePtrBuffer = new byte[8];
                    HelperFunctions.ReadProcessMemory(riskProcess.Handle,
                        IntPtr.Add(territoryAddress, TerritoryOffsets.NAME_OFFSET),
                        namePtrBuffer, 8, out bytesRead);
                    IntPtr territoryNameAddress = new IntPtr(BitConverter.ToInt64(namePtrBuffer, 0));

                    if (territoryNameAddress == IntPtr.Zero || territoryNameAddress.ToInt64() <= 0x10000) continue;

                    byte[] lenBuffer = new byte[4];
                    HelperFunctions.ReadProcessMemory(riskProcess.Handle,
                        IntPtr.Add(territoryNameAddress, StringOffsets.SIZE_OFFSET),
                        lenBuffer, 4, out bytesRead);
                    int nameLength = BitConverter.ToInt32(lenBuffer, 0);

                    if (nameLength > 0 && nameLength < 100)
                    {
                        byte[] nameBuffer = new byte[nameLength * 2];
                        HelperFunctions.ReadProcessMemory(riskProcess.Handle,
                            IntPtr.Add(territoryNameAddress, StringOffsets.FIRST_CHAR_OFFSET),
                            nameBuffer, nameLength * 2, out bytesRead);
                        string territoryName = System.Text.Encoding.ASCII.GetString(nameBuffer.Where(x => x != 0x00).ToArray()).ToLower();

                        if (!string.IsNullOrEmpty(territoryName) && !territoryNamesToAdresses.ContainsKey(territoryName))
                        {
                            territoryNamesToAdresses.Add(territoryName, territoryAddress);
                        }
                    }
                }
                catch { }
            }
        }

        void SetRISKDataInComponent()
        {
            try
            {
                if (territoriesFlowPanel.InvokeRequired)
                {
                    territoriesFlowPanel.Invoke(new Action(SetRISKDataInComponent));
                    return;
                }

                territoriesFlowPanel.SuspendLayout();
                playersFlowPanel.SuspendLayout();

                territoriesFlowPanel.Controls.Clear();
                playersFlowPanel.Controls.Clear();

                List<String> territoryNames = territoryNamesToAdresses.Keys.ToList();
                List<String> playerColors = playerColorsToAddresses.Keys.ToList();

                string selectedBefore = cmbPlayerFilter.SelectedItem?.ToString();
                cmbPlayerFilter.Items.Clear();
                cmbPlayerFilter.Items.Add("All Players");

                for (int i = 0; i < playerColors.Count; ++i)
                {
                    cmbPlayerFilter.Items.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(playerColors[i]));
                    AddPlayerInTool(playerColors[i], i);
                }

                if (!string.IsNullOrEmpty(selectedBefore) && cmbPlayerFilter.Items.Contains(selectedBefore))
                {
                    cmbPlayerFilter.SelectedItem = selectedBefore;
                }
                else
                {
                    cmbPlayerFilter.SelectedIndex = 0;
                }

                for (int i = 0; i < territoryNames.Count && i < 200; ++i)
                {
                    AddTerritoryInTool(territoryNames[i], i);
                }

                territoriesFlowPanel.ResumeLayout();
                playersFlowPanel.ResumeLayout();

                UpdateTerritoryColors();
                ApplyTerritoryFilter();
            }
            catch { }
        }

        private void AddTerritoryInTool(string territoryName, int index)
        {
            try
            {
                Panel card = new Panel();
                card.Name = territoryName;
                card.Size = new Size(territoriesFlowPanel.Width - 30, 42);
                card.Margin = new Padding(0, 0, 0, 6);

                card.BackColor = Color.FromArgb(35, 35, 35);
                card.Padding = new Padding(2);

                Panel colorPanel = new Panel();
                colorPanel.Name = territoryName + "_ColorPanel";
                colorPanel.Dock = DockStyle.Left;
                colorPanel.Size = new Size(180, 42);
                colorPanel.BackColor = GetColorForTerritoryTextBox(territoryName);

                Label lblTerritoryName = new Label();
                lblTerritoryName.Name = territoryName + "_Label";
                lblTerritoryName.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(territoryName);
                lblTerritoryName.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                lblTerritoryName.ForeColor = Color.Black;
                lblTerritoryName.Dock = DockStyle.Fill;
                lblTerritoryName.TextAlign = ContentAlignment.MiddleCenter;

                Color bgColor = colorPanel.BackColor;
                float brightness = (bgColor.R * 0.299f + bgColor.G * 0.587f + bgColor.B * 0.114f) / 255f;
                lblTerritoryName.ForeColor = brightness < 0.5f ? Color.White : Color.Black;

                colorPanel.Controls.Add(lblTerritoryName);

                Label lblTroopCounter = new Label();
                lblTroopCounter.Name = territoryName + "_TroopCounter";
                lblTroopCounter.AutoSize = true;
                lblTroopCounter.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                lblTroopCounter.ForeColor = Color.White;
                lblTroopCounter.Location = new Point(190, 12);
                lblTroopCounter.Text = "";

                Button btnTroops = CreateCardButton("👷‍♂️  Edit", 190, (s, e) => SetTroopsButton_Click(s, e, territoryName));
                Button btnCapital = CreateCardButton("👑 Capital", 275, (s, e) => SetCapital(territoryName));
                Button btnBlizzard = CreateCardButton("❄️ Blizzard", 360, (s, e) => SetBlizzard(territoryName));

                card.Controls.Add(colorPanel);
                card.Controls.Add(lblTroopCounter);
                card.Controls.Add(btnTroops);
                card.Controls.Add(btnCapital);
                card.Controls.Add(btnBlizzard);

                territoriesFlowPanel.Controls.Add(card);
            }
            catch { }
        }

        private void AddPlayerInTool(string playerColor, int index)
        {
            try
            {
                Panel card = new Panel
                {
                    Name = playerColor,
                    Size = new Size(Math.Max(playersFlowPanel.ClientSize.Width - 25, 460), 42),
                    Margin = new Padding(0, 0, 0, 6),
                    BackColor = Color.FromArgb(35, 35, 35),
                    Padding = new Padding(2)
                };

                Color playerColorValue = GetColorForPlayerName(playerColor);
                Panel colorPanel = new Panel
                {
                    Name = playerColor + "_ColorPanel",
                    Dock = DockStyle.Left,
                    Size = new Size(180, 42),
                    BackColor = playerColorValue
                };

                float brightness = (playerColorValue.R * 0.299f + playerColorValue.G * 0.587f + playerColorValue.B * 0.114f) / 255f;
                Label lblPlayerName = new Label
                {
                    Name = playerColor + "_Label",
                    Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(playerColor),
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = brightness < 0.5f ? Color.White : Color.Black,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                colorPanel.Controls.Add(lblPlayerName);

                Button btnAddTroops = CreateCardButton("⚔️ Set Troops", 190, (s, e) => SetPlayerTroopsButton_Click(s, e, playerColor));
                Button btnShowCards = CreateCardButton("🃏 Show Cards", 280, (s, e) => ShowPlayerCards(playerColor));

                card.Controls.Add(colorPanel);
                card.Controls.Add(btnAddTroops);
                card.Controls.Add(btnShowCards);

                playersFlowPanel.Controls.Add(card);
            }
            catch { }
        }

        private Button CreateCardButton(string text, int posX, EventHandler onClick)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 8.5F);
            btn.Location = new Point(posX, 5);
            btn.Size = new Size(80, 32);
            btn.Click += onClick;
            btn.BackColor = Color.FromArgb(50, 50, 50);
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;

            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(70, 70, 70);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(50, 50, 50);

            return btn;
        }

        private string GetColorNameFromPlayer(IntPtr playerPtr)
        {
            try
            {
                if (playerPtr == IntPtr.Zero) return "color_unknown";

                byte[] gamePlayerColorPtrBuffer = new byte[8];
                HelperFunctions.ReadProcessMemory(riskProcess.Handle, IntPtr.Add(playerPtr, 0xD0), gamePlayerColorPtrBuffer, 8, out bytesRead);
                IntPtr gamePlayerColorPtr = new IntPtr(BitConverter.ToInt64(gamePlayerColorPtrBuffer, 0));

                if (gamePlayerColorPtr == IntPtr.Zero) return "color_unknown";

                byte[] colorIdPtrBuffer = new byte[8];
                HelperFunctions.ReadProcessMemory(riskProcess.Handle, IntPtr.Add(gamePlayerColorPtr, 0x10), colorIdPtrBuffer, 8, out bytesRead);
                IntPtr colorIdPtr = new IntPtr(BitConverter.ToInt64(colorIdPtrBuffer, 0));

                if (colorIdPtr == IntPtr.Zero) return "color_unknown";

                byte[] lenBuffer = new byte[4];
                HelperFunctions.ReadProcessMemory(riskProcess.Handle, IntPtr.Add(colorIdPtr, StringOffsets.SIZE_OFFSET), lenBuffer, 4, out bytesRead);
                int len = BitConverter.ToInt32(lenBuffer, 0);

                if (len <= 0 || len > 50) return "color_unknown";

                byte[] strBuffer = new byte[len * 2];
                HelperFunctions.ReadProcessMemory(riskProcess.Handle, IntPtr.Add(colorIdPtr, StringOffsets.FIRST_CHAR_OFFSET), strBuffer, len * 2, out bytesRead);
                string colorName = System.Text.Encoding.ASCII.GetString(strBuffer.Where(x => x != 0x00).ToArray()).ToLower();

                return !string.IsNullOrEmpty(colorName) ? colorName : "color_unknown";
            }
            catch { return "color_unknown"; }
        }

        private Color GetActualPlayerColor(IntPtr playerPtr)
        {
            try
            {
                if (playerPtr == IntPtr.Zero) return Color.Empty;

                byte[] gamePlayerColorPtrBuffer = new byte[8];
                HelperFunctions.ReadProcessMemory(riskProcess.Handle, IntPtr.Add(playerPtr, 0xD0), gamePlayerColorPtrBuffer, 8, out bytesRead);
                IntPtr gamePlayerColorPtr = new IntPtr(BitConverter.ToInt64(gamePlayerColorPtrBuffer, 0));

                if (gamePlayerColorPtr == IntPtr.Zero) return Color.Empty;

                byte[] colorData = new byte[16];
                HelperFunctions.ReadProcessMemory(riskProcess.Handle, IntPtr.Add(gamePlayerColorPtr, 0x40), colorData, 16, out bytesRead);

                if (bytesRead.ToInt32() == 16)
                {
                    float r = BitConverter.ToSingle(colorData, 0);
                    float g = BitConverter.ToSingle(colorData, 4);
                    float b = BitConverter.ToSingle(colorData, 8);

                    if (r >= 0 && r <= 1 && g >= 0 && g <= 1 && b >= 0 && b <= 1)
                    {
                        return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
                    }
                }
                return Color.Empty;
            }
            catch { return Color.Empty; }
        }

        private Color GetColorForPlayerName(string playerName)
        {
            if (playerColorsToAddresses.ContainsKey(playerName))
            {
                Color actualColor = GetActualPlayerColor(playerColorsToAddresses[playerName]);
                if (actualColor != Color.Empty) return actualColor;
            }

            string key = playerName.ToLower().Replace("color_", "");
            switch (key)
            {
                case "blue": return Color.FromArgb(0, 114, 198);
                case "red": return Color.FromArgb(204, 0, 0);
                case "orange": return Color.FromArgb(230, 120, 0);
                case "yellow": return Color.FromArgb(230, 200, 0);
                case "pink": return Color.FromArgb(230, 80, 140);
                case "green": return Color.FromArgb(0, 170, 80);
                case "purple": return Color.FromArgb(128, 0, 128);
                case "black": return Color.FromArgb(30, 30, 30);
                case "white": return Color.FromArgb(220, 220, 220);
                default: return Color.FromArgb(100, 100, 100);
            }
        }

        private Color GetColorForTerritoryTextBox(String territoryName)
        {
            try
            {
                if (!territoryNamesToAdresses.ContainsKey(territoryName)) return Color.FromArgb(50, 50, 50);
                IntPtr territoryPtr = territoryNamesToAdresses[territoryName];
                IntPtr playerPtr = GetPlayerAddrFromTerritoryAddr(territoryPtr);
                if (playerPtr == IntPtr.Zero) return Color.FromArgb(50, 50, 50);

                Color actualColor = GetActualPlayerColor(playerPtr);
                return actualColor != Color.Empty ? actualColor : Color.FromArgb(50, 50, 50);
            }
            catch { return Color.FromArgb(50, 50, 50); }
        }

        private void UpdateTerritoryColors()
        {
            try
            {
                foreach (String key in territoryNamesToAdresses.Keys)
                {
                    var controls = territoriesFlowPanel.Controls.Find(key + "_ColorPanel", true);
                    if (controls != null && controls.Length > 0)
                    {
                        Panel colorPanel = (Panel)controls[0];
                        Color newColor = GetColorForTerritoryTextBox(key);
                        if (colorPanel.BackColor != newColor)
                        {
                            colorPanel.BackColor = newColor;

                            var labelControls = colorPanel.Controls.Find(key + "_Label", true);
                            if (labelControls != null && labelControls.Length > 0)
                            {
                                Label lbl = (Label)labelControls[0];
                                float brightness = (newColor.R * 0.299f + newColor.G * 0.587f + newColor.B * 0.114f) / 255f;
                                lbl.ForeColor = brightness < 0.5f ? Color.White : Color.Black;
                            }
                        }
                    }
                }
                statusLabel.Text = "Colors updated successfully";
            }
            catch { }
        }

        private void SetTroops(String territoryName, int troopCount)
        {
            if (!territoryNamesToAdresses.ContainsKey(territoryName)) return;
            IntPtr territoryPtr = territoryNamesToAdresses[territoryName];
            int[] offsetsToTry = new int[] { 0x90, 0x94, 0x98, 0x9C, 0xA0, 0xA4, 0xA8 };

            foreach (int offset in offsetsToTry)
            {
                HelperFunctions.WriteProcessMemory(riskProcess.Handle, IntPtr.Add(territoryPtr, offset), BitConverter.GetBytes(troopCount), MemoryConstants.INT_BYTES, out bytesRead);
            }
            statusLabel.Text = $"Set {territoryName} troops to {troopCount}";
        }

        private void SetPlayerTroops(String playerColor, int troopCount)
        {
            if (!playerColorsToAddresses.ContainsKey(playerColor)) return;
            IntPtr playerPtr = playerColorsToAddresses[playerColor];

            HelperFunctions.WriteProcessMemory(riskProcess.Handle, IntPtr.Add(playerPtr, PlayerOffsets.PLACEABLE_TROOPS_OFFSET), BitConverter.GetBytes(troopCount), MemoryConstants.INT_BYTES, out bytesRead);

            int[] offsetsToTry = new int[] { 0xD8, 0xDC, 0xE0, 0xE4, 0xE8, 0xEC, 0xF0, 0xF4, 0xF8 };
            foreach (int offset in offsetsToTry)
            {
                HelperFunctions.WriteProcessMemory(riskProcess.Handle, IntPtr.Add(playerPtr, offset), BitConverter.GetBytes(troopCount), MemoryConstants.INT_BYTES, out bytesRead);
            }
            statusLabel.Text = $"Set {playerColor} player troops to {troopCount}";
        }

        private void SetCapital(String territoryName)
        {
            if (!territoryNamesToAdresses.ContainsKey(territoryName)) return;
            IntPtr territoryPtr = territoryNamesToAdresses[territoryName];
            HelperFunctions.WriteProcessMemory(riskProcess.Handle, IntPtr.Add(territoryPtr, TerritoryOffsets.IS_CAPITAL_OFFSET),
                BitConverter.GetBytes((int)TerritoryType.Capital), MemoryConstants.INT_BYTES, out bytesRead);
            statusLabel.Text = $"Set {territoryName} as Capital";
        }

        private void SetBlizzard(String territoryName)
        {
            if (!territoryNamesToAdresses.ContainsKey(territoryName)) return;
            IntPtr territoryPtr = territoryNamesToAdresses[territoryName];
            HelperFunctions.WriteProcessMemory(riskProcess.Handle, IntPtr.Add(territoryPtr, TerritoryOffsets.ENCRYPTED_UNITS_OFFSET), BitConverter.GetBytes(IntPtr.Zero.ToInt64()), MemoryConstants.POINTER_BYTES, out bytesRead);
            HelperFunctions.WriteProcessMemory(riskProcess.Handle, IntPtr.Add(territoryPtr, TerritoryOffsets.PLAYER_OFFSET), BitConverter.GetBytes(IntPtr.Zero.ToInt64()), MemoryConstants.POINTER_BYTES, out bytesRead);
            HelperFunctions.WriteProcessMemory(riskProcess.Handle, IntPtr.Add(territoryPtr, TerritoryOffsets.IS_CAPITAL_OFFSET), BitConverter.GetBytes((int)TerritoryType.Blizzard), MemoryConstants.INT_BYTES, out bytesRead);
            statusLabel.Text = $"Set {territoryName} as Blizzard";
        }

        private void SetTroopsButton_Click(object sender, EventArgs e, string territoryName)
        {
            String troopCountStr = Microsoft.VisualBasic.Interaction.InputBox("Troop count for " + territoryName + ":", "Set Troops", "10");
            if (int.TryParse(troopCountStr, out int troopCount))
            {
                SetTroops(territoryName, troopCount);
            }
        }

        private void SetPlayerTroopsButton_Click(object sender, EventArgs e, string playerColor)
        {
            String troopCountStr = Microsoft.VisualBasic.Interaction.InputBox("Troop count for " + playerColor + " player:", "Set Player Troops", "50");
            if (int.TryParse(troopCountStr, out int troopCount))
            {
                SetPlayerTroops(playerColor, troopCount);
            }
        }

        private void PlayerCardsButton_Click(object sender, EventArgs args, string playerColor)
        {
            OpenPlayerCardsForm(playerColor, isReadOnly: true);
        }

        private IntPtr GetPlayerAddrFromTerritoryAddr(IntPtr territoryAddr)
        {
            try
            {
                buffer = new byte[Constants.MAX_BUFFER_SIZE];
                HelperFunctions.ReadProcessMemory(riskProcess.Handle, IntPtr.Add(territoryAddr, TerritoryOffsets.PLAYER_OFFSET), buffer, MemoryConstants.POINTER_BYTES, out bytesRead);
                return new IntPtr(BitConverter.ToInt64(buffer, 0));
            }
            catch { return IntPtr.Zero; }
        }

        private void toggleFogButton_Click(object sender, EventArgs e)
        {
            try
            {
                IntPtr gameManagerStaticFields = GetGameManagerStaticFields();
                if (gameManagerStaticFields == IntPtr.Zero) return;

                IntPtr fogAddr = IntPtr.Add(gameManagerStaticFields, GameManagerOffsets.IS_FOG_OF_WAR_OFFSET);
                buffer = new byte[Constants.MAX_BUFFER_SIZE];
                HelperFunctions.ReadProcessMemory(riskProcess.Handle, fogAddr, buffer, MemoryConstants.BOOLEAN_BYTES, out bytesRead);

                bool enableFog = BitConverter.ToBoolean(buffer);
                HelperFunctions.WriteProcessMemory(riskProcess.Handle, fogAddr, BitConverter.GetBytes(!enableFog), MemoryConstants.BOOLEAN_BYTES, out bytesRead);

                statusLabel.Text = $"Fog of War {(enableFog ? "Disabled" : "Enabled")}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fog toggle error: {ex.Message}");
            }
        }

        private void loadCsvButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "CSV files|*.csv;" };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                parseCsv(openFileDialog.FileName);
                statusLabel.Text = $"Parsed CSV: {Path.GetFileName(openFileDialog.FileName)}";
            }
        }

        private void parseCsv(String fileName)
        {
            var lines = File.ReadLines(fileName);
            foreach (var line in lines)
            {
                var fields = line.ToLower().Split(',');
                if (fields.Length != CSVConstants.NUM_COLS || !territoryNamesToAdresses.ContainsKey(fields[CSVConstants.TERRITORY_NAME]))
                    continue;

                SetTroops(fields[CSVConstants.TERRITORY_NAME], int.Parse(fields[CSVConstants.TROOP_COUNT]));
            }
            UpdateTerritoryColors();
        }
    }
}