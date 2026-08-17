namespace FFClient
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            scintilla = new ScintillaNET.Scintilla();
            menuStrip1 = new MenuStrip();
            runToolStripMenuItem = new ToolStripMenuItem();
            toggleFreezeF1ToolStripMenuItem = new ToolStripMenuItem();
            stepFrameF2ToolStripMenuItem = new ToolStripMenuItem();
            runToolStripMenuItem1 = new ToolStripMenuItem();
            continueTASF4ToolStripMenuItem = new ToolStripMenuItem();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // scintilla
            // 
            scintilla.AutocompleteListSelectedBackColor = Color.FromArgb(0, 120, 215);
            scintilla.Dock = DockStyle.Fill;
            scintilla.Font = new Font("Verdana", 9F);
            scintilla.LexerName = null;
            scintilla.Location = new Point(0, 24);
            scintilla.Name = "scintilla";
            scintilla.ScrollWidth = 49;
            scintilla.Size = new Size(384, 337);
            scintilla.TabIndex = 0;
            scintilla.Click += scintilla1_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { runToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(384, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // runToolStripMenuItem
            // 
            runToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toggleFreezeF1ToolStripMenuItem, stepFrameF2ToolStripMenuItem, runToolStripMenuItem1, continueTASF4ToolStripMenuItem });
            runToolStripMenuItem.Name = "runToolStripMenuItem";
            runToolStripMenuItem.Size = new Size(40, 20);
            runToolStripMenuItem.Text = "Run";
            // 
            // toggleFreezeF1ToolStripMenuItem
            // 
            toggleFreezeF1ToolStripMenuItem.Name = "toggleFreezeF1ToolStripMenuItem";
            toggleFreezeF1ToolStripMenuItem.Size = new Size(179, 22);
            toggleFreezeF1ToolStripMenuItem.Text = "Toggle On/Off (F1)";
            // 
            // stepFrameF2ToolStripMenuItem
            // 
            stepFrameF2ToolStripMenuItem.Name = "stepFrameF2ToolStripMenuItem";
            stepFrameF2ToolStripMenuItem.Size = new Size(179, 22);
            stepFrameF2ToolStripMenuItem.Text = "Advance Frame (F2)";
            // 
            // runToolStripMenuItem1
            // 
            runToolStripMenuItem1.Name = "runToolStripMenuItem1";
            runToolStripMenuItem1.Size = new Size(179, 22);
            runToolStripMenuItem1.Text = "Run TAS (F3)";
            runToolStripMenuItem1.Click += runToolStripMenuItem1_Click;
            // 
            // continueTASF4ToolStripMenuItem
            // 
            continueTASF4ToolStripMenuItem.Name = "continueTASF4ToolStripMenuItem";
            continueTASF4ToolStripMenuItem.Size = new Size(179, 22);
            continueTASF4ToolStripMenuItem.Text = "Continue TAS (F4)";
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 339);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(384, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(118, 17);
            toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            toolStripStatusLabel1.Click += toolStripStatusLabel1_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 361);
            Controls.Add(statusStrip1);
            Controls.Add(scintilla);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "MainForm";
            Text = "FreezeFrame TAS Editor";
            Load += Form1_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ScintillaNET.Scintilla scintilla;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem runToolStripMenuItem;
        private ToolStripMenuItem toggleFreezeF1ToolStripMenuItem;
        private ToolStripMenuItem stepFrameF2ToolStripMenuItem;
        private ToolStripMenuItem runToolStripMenuItem1;
        private ToolStripMenuItem continueTASF4ToolStripMenuItem;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
    }
}
