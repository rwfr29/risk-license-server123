namespace RISKHackTool
{
    partial class RISKHack
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.autoRefreshWorker = new System.ComponentModel.BackgroundWorker();

            // Main Layout Panels
            this.topToolbarPanel = new System.Windows.Forms.Panel();
            this.sidebarPanel = new System.Windows.Forms.Panel();
            this.mainContainerPanel = new System.Windows.Forms.Panel();

            this.bottomStatusBarPanel = new System.Windows.Forms.Panel();

            // Toolbar Controls
            this.lblAppTitle = new System.Windows.Forms.Label();
            this.btnToolbarRefresh = new System.Windows.Forms.Button();
            this.btnToolbarFog = new System.Windows.Forms.Button();
            this.btnToolbarUpdateColors = new System.Windows.Forms.Button();

            // Sidebar Controls
            this.btnNavTerritories = new System.Windows.Forms.Button();
            this.btnNavPlayers = new System.Windows.Forms.Button();
            this.btnNavActions = new System.Windows.Forms.Button();
            this.pnlActiveIndicator = new System.Windows.Forms.Panel();

            // Content Views
            this.pnlTerritoriesView = new System.Windows.Forms.Panel();
            this.pnlPlayersView = new System.Windows.Forms.Panel();
            this.pnlActionsView = new System.Windows.Forms.Panel();
            this.pnlShowCardsView = new System.Windows.Forms.Panel();

            // Search Bar & Controls
            this.pnlSearchHeader = new System.Windows.Forms.Panel();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearchIcon = new System.Windows.Forms.Label();

            // Player Filter ComboBox
            this.cmbPlayerFilter = new System.Windows.Forms.ComboBox();

            // Flow Panels
            this.territoriesFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.playersFlowPanel = new System.Windows.Forms.FlowLayoutPanel();

            // Status Bar Controls
            this.statusLabel = new System.Windows.Forms.Label();
            this.lblProcessState = new System.Windows.Forms.Label();

            this.topToolbarPanel.SuspendLayout();
            this.sidebarPanel.SuspendLayout();
            this.mainContainerPanel.SuspendLayout();
            this.pnlSearchHeader.SuspendLayout();
            this.pnlTerritoriesView.SuspendLayout();
            this.pnlPlayersView.SuspendLayout();
            this.pnlActionsView.SuspendLayout();
            this.pnlShowCardsView.SuspendLayout();

            this.bottomStatusBarPanel.SuspendLayout();
            this.SuspendLayout();

            // Colors setup
            Color bgDark = Color.FromArgb(20, 20, 20);
            Color bgSurface = Color.FromArgb(30, 30, 30);
            Color sidebarBg = Color.FromArgb(24, 24, 24);
            Color cardBg = Color.FromArgb(40, 40, 40);
            Color textLight = Color.FromArgb(230, 230, 230);
            Color accentBlue = Color.FromArgb(0, 122, 204);

            // ==========================================
            // RISKHack Form
            // ==========================================
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = bgDark;
            this.ClientSize = new System.Drawing.Size(680, 750);
            this.MinimumSize = new System.Drawing.Size(680, 550);
            this.Name = "";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "";

            // ==========================================
            // Top Toolbar Panel
            // ==========================================
            this.topToolbarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topToolbarPanel.Height = 50;
            this.topToolbarPanel.BackColor = sidebarBg;
            this.topToolbarPanel.Padding = new System.Windows.Forms.Padding(15, 8, 15, 8);

            this.lblAppTitle.Text = "discord.gg/defyy";
            this.lblAppTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblAppTitle.ForeColor = accentBlue;
            this.lblAppTitle.AutoSize = true;
            this.lblAppTitle.Location = new System.Drawing.Point(12, 14);

            this.btnToolbarRefresh.Text = "🔄 Refresh";
            this.btnToolbarRefresh.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnToolbarRefresh.Width = 100;
            this.btnToolbarRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToolbarRefresh.FlatAppearance.BorderSize = 0;
            this.btnToolbarRefresh.BackColor = accentBlue;
            this.btnToolbarRefresh.ForeColor = Color.White;
            this.btnToolbarRefresh.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);


            this.btnToolbarFog.Text = "🌫️ Toggle Fog";
            this.btnToolbarFog.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnToolbarFog.Width = 110;
            this.btnToolbarFog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToolbarFog.FlatAppearance.BorderSize = 0;
            this.btnToolbarFog.BackColor = Color.FromArgb(210, 110, 0);
            this.btnToolbarFog.ForeColor = Color.White;
            this.btnToolbarFog.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnToolbarFog.Click += new System.EventHandler(this.toggleFogButton_Click);

            this.btnToolbarUpdateColors.Text = "🎨 Recolor";
            this.btnToolbarUpdateColors.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnToolbarUpdateColors.Width = 95;
            this.btnToolbarUpdateColors.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToolbarUpdateColors.FlatAppearance.BorderSize = 0;
            this.btnToolbarUpdateColors.BackColor = Color.FromArgb(130, 80, 190);
            this.btnToolbarUpdateColors.ForeColor = Color.White;
            this.btnToolbarUpdateColors.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnToolbarUpdateColors.Click += (s, e) => this.UpdateTerritoryColors();

            this.topToolbarPanel.Controls.Add(this.lblAppTitle);
            this.topToolbarPanel.Controls.Add(this.btnToolbarUpdateColors);
            this.topToolbarPanel.Controls.Add(this.btnToolbarFog);
            this.topToolbarPanel.Controls.Add(this.btnToolbarRefresh);

            // ==========================================
            // Sidebar Panel
            // ==========================================
            this.sidebarPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.sidebarPanel.Width = 170;
            this.sidebarPanel.BackColor = sidebarBg;

            this.pnlActiveIndicator.Size = new System.Drawing.Size(4, 45);
            this.pnlActiveIndicator.BackColor = accentBlue;
            this.pnlActiveIndicator.Location = new System.Drawing.Point(0, 10);

            this.btnNavTerritories.Text = " 🗺️  Territories";
            this.btnNavTerritories.Location = new System.Drawing.Point(4, 10);
            this.btnNavTerritories.Size = new System.Drawing.Size(166, 45);
            this.btnNavTerritories.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavTerritories.FlatAppearance.BorderSize = 0;
            this.btnNavTerritories.ForeColor = Color.White;
            this.btnNavTerritories.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnNavTerritories.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavTerritories.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);

            this.btnNavPlayers.Text = " 👥  Players";
            this.btnNavPlayers.Location = new System.Drawing.Point(4, 60);
            this.btnNavPlayers.Size = new System.Drawing.Size(166, 45);
            this.btnNavPlayers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavPlayers.FlatAppearance.BorderSize = 0;
            this.btnNavPlayers.ForeColor = Color.Gray;
            this.btnNavPlayers.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnNavPlayers.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavPlayers.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);

            this.btnNavActions.Text = " ⚙️  Quick Actions";
            this.btnNavActions.Location = new System.Drawing.Point(4, 110);
            this.btnNavActions.Size = new System.Drawing.Size(166, 45);
            this.btnNavActions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavActions.FlatAppearance.BorderSize = 0;
            this.btnNavActions.ForeColor = Color.Gray;
            this.btnNavActions.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.btnNavActions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavActions.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);

            this.sidebarPanel.Controls.Add(this.pnlActiveIndicator);
            this.sidebarPanel.Controls.Add(this.btnNavTerritories);
            this.sidebarPanel.Controls.Add(this.btnNavPlayers);
            this.sidebarPanel.Controls.Add(this.btnNavActions);

            // Main Container - Adjust padding to create space between search and territories
            this.mainContainerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainContainerPanel.BackColor = bgDark;
            this.mainContainerPanel.Padding = new System.Windows.Forms.Padding(12, 12, 12, 12); // Top padding added

            // Search Header Panel - Make it taller and properly aligned
            this.pnlSearchHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSearchHeader.Height = 50; // Increased height for better centering
            this.pnlSearchHeader.BackColor = cardBg;
            this.pnlSearchHeader.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8); // More padding

            // Create a TableLayoutPanel for perfect alignment
            TableLayoutPanel searchLayout = new TableLayoutPanel();
            searchLayout.Dock = DockStyle.Fill;
            searchLayout.ColumnCount = 3;
            searchLayout.RowCount = 1;
            searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 35)); // Icon
            searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Search
            searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 185)); // ComboBox
            searchLayout.BackColor = Color.Transparent;
            searchLayout.Margin = new Padding(0);

            // Search Icon - centered vertically
            this.lblSearchIcon.Text = "🔍";
            this.lblSearchIcon.Dock = DockStyle.Fill;
            this.lblSearchIcon.ForeColor = Color.Gray;
            this.lblSearchIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblSearchIcon.Margin = new Padding(0);
            this.lblSearchIcon.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

            // Search TextBox - properly centered with internal padding
            this.txtSearch.Anchor = AnchorStyles.Left | AnchorStyles.Right; // was: Dock = DockStyle.Fill
            this.txtSearch.BackColor = cardBg;
            this.txtSearch.ForeColor = textLight;
            this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtSearch.Text = "Search territories or players...";
            this.txtSearch.Margin = new Padding(5, 0, 5, 0); // horizontal only now, vertical handled by centering below
            this.txtSearch.TextAlign = HorizontalAlignment.Left; // Keep text left-aligned
            this.txtSearch.Padding = new Padding(5, 0, 0, 0); // Add some left padding

            // Player Filter ComboBox - properly centered
            this.cmbPlayerFilter.Dock = DockStyle.Fill;
            this.cmbPlayerFilter.BackColor = bgSurface;
            this.cmbPlayerFilter.ForeColor = textLight;
            this.cmbPlayerFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbPlayerFilter.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cmbPlayerFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPlayerFilter.Margin = new Padding(0, 5, 0, 5); // Vertical margin for centering
            this.cmbPlayerFilter.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

            // Add controls to TableLayoutPanel
            searchLayout.Controls.Add(this.lblSearchIcon, 0, 0);
            searchLayout.Controls.Add(this.txtSearch, 1, 0);
            searchLayout.Controls.Add(this.cmbPlayerFilter, 2, 0);

            // Vertically center txtSearch within its TableLayoutPanel cell
            // (TextBox ignores Dock.Fill vertical centering, so we position it manually)
            searchLayout.Layout += (s, e) =>
            {
                int cellHeight = searchLayout.GetRowHeights()[0];
                txtSearch.Width = searchLayout.GetColumnWidths()[1] - txtSearch.Margin.Horizontal;
                txtSearch.Top = (cellHeight - txtSearch.Height) / 2;
            };

            // Clear existing controls and add the TableLayoutPanel
            this.pnlSearchHeader.Controls.Clear();
            this.pnlSearchHeader.Controls.Add(searchLayout);

            // Territories FlowPanel - Add top margin to separate from search
            this.pnlTerritoriesView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTerritoriesView.Padding = new Padding(0, 8, 0, 0); // Add top padding

            this.territoriesFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.territoriesFlowPanel.AutoScroll = true;
            this.territoriesFlowPanel.WrapContents = false;
            this.territoriesFlowPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.territoriesFlowPanel.Padding = new Padding(0, 5, 5, 5); // Add some padding

            // Make sure the territories view contains the flow panel
            this.pnlTerritoriesView.Controls.Clear();
            this.pnlTerritoriesView.Controls.Add(this.territoriesFlowPanel);


            // Territories Panel View
            this.pnlTerritoriesView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.territoriesFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.territoriesFlowPanel.AutoScroll = true;
            this.territoriesFlowPanel.WrapContents = false;
            this.territoriesFlowPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlTerritoriesView.Controls.Add(this.territoriesFlowPanel);

            // Players View Panel - Add padding to separate from search
            this.pnlPlayersView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPlayersView.Visible = false;
            this.pnlPlayersView.Padding = new Padding(0, 8, 0, 0); // Top padding to separate from search

            // Players FlowPanel - with proper spacing
            this.playersFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.playersFlowPanel.AutoScroll = true;
            this.playersFlowPanel.WrapContents = false;
            this.playersFlowPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.playersFlowPanel.Padding = new Padding(0, 5, 5, 5); // Add padding
            this.playersFlowPanel.Margin = new Padding(0, 5, 0, 0);

            // Make sure players view contains the flow panel
            this.pnlPlayersView.Controls.Clear();
            this.pnlPlayersView.Controls.Add(this.playersFlowPanel);

            // Actions Panel View
            this.pnlActionsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlActionsView.Visible = false;

            // Integrated Show Cards Panel View
            this.pnlShowCardsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlShowCardsView.Visible = false;
            this.pnlShowCardsView.AutoScroll = true;

            this.mainContainerPanel.Controls.Add(this.pnlTerritoriesView);
            this.mainContainerPanel.Controls.Add(this.pnlPlayersView);
            this.mainContainerPanel.Controls.Add(this.pnlActionsView);
            this.mainContainerPanel.Controls.Add(this.pnlShowCardsView);
            this.mainContainerPanel.Controls.Add(this.pnlSearchHeader);

            // ==========================================
            // Status Bar Panel
            // ==========================================
            this.bottomStatusBarPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomStatusBarPanel.Height = 28;
            this.bottomStatusBarPanel.BackColor = sidebarBg;
            this.bottomStatusBarPanel.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);

            this.statusLabel.Text = "Ready - Please Start Risk...";
            this.statusLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.statusLabel.AutoSize = true;
            this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.statusLabel.ForeColor = textLight;

            this.lblProcessState.Text = "Risk : Disconnected";
            this.lblProcessState.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblProcessState.AutoSize = true;
            this.lblProcessState.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblProcessState.ForeColor = Color.Orange;

            this.bottomStatusBarPanel.Controls.Add(this.statusLabel);
            this.bottomStatusBarPanel.Controls.Add(this.lblProcessState);

            this.Controls.Add(this.mainContainerPanel);
            this.Controls.Add(this.sidebarPanel);
            this.Controls.Add(this.topToolbarPanel);
            this.Controls.Add(this.bottomStatusBarPanel);

            this.topToolbarPanel.ResumeLayout(false);
            this.topToolbarPanel.PerformLayout();
            this.sidebarPanel.ResumeLayout(false);
            this.mainContainerPanel.ResumeLayout(false);
            this.pnlSearchHeader.ResumeLayout(false);
            this.pnlSearchHeader.PerformLayout();
            this.pnlTerritoriesView.ResumeLayout(false);
            this.pnlPlayersView.ResumeLayout(false);
            this.pnlActionsView.ResumeLayout(false);
            this.pnlShowCardsView.ResumeLayout(false);

            this.bottomStatusBarPanel.ResumeLayout(false);
            this.bottomStatusBarPanel.PerformLayout();

            this.ResumeLayout(false);
        }

        #endregion

        private System.ComponentModel.BackgroundWorker backgroundWorker;
        private System.ComponentModel.BackgroundWorker autoRefreshWorker;

        private System.Windows.Forms.Panel topToolbarPanel;
        private System.Windows.Forms.Panel sidebarPanel;
        private System.Windows.Forms.Panel mainContainerPanel;
        private System.Windows.Forms.Panel bottomStatusBarPanel;

        private System.Windows.Forms.Label lblAppTitle;
        private System.Windows.Forms.Button btnToolbarRefresh;
        private System.Windows.Forms.Button btnToolbarFog;
        private System.Windows.Forms.Button btnToolbarUpdateColors;

        private System.Windows.Forms.Button btnNavTerritories;
        private System.Windows.Forms.Button btnNavPlayers;
        private System.Windows.Forms.Button btnNavActions;
        private System.Windows.Forms.Panel pnlActiveIndicator;

        private System.Windows.Forms.Panel pnlSearchHeader;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearchIcon;
        private System.Windows.Forms.ComboBox cmbPlayerFilter;

        private System.Windows.Forms.Panel pnlTerritoriesView;
        private System.Windows.Forms.Panel pnlPlayersView;
        private System.Windows.Forms.Panel pnlActionsView;
        private System.Windows.Forms.Panel pnlShowCardsView;

        private System.Windows.Forms.FlowLayoutPanel territoriesFlowPanel;
        private System.Windows.Forms.FlowLayoutPanel playersFlowPanel;

        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Label lblProcessState;
    }
}