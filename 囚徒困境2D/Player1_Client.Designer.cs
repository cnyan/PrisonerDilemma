namespace 囚徒困境2D
{
    partial class Player1_Client
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.uiRichTextBox2 = new Sunny.UI.UIRichTextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Desktop;
            this.panel1.Controls.Add(this.uiRichTextBox2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 450);
            this.panel1.TabIndex = 0;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // uiRichTextBox2
            // 
            this.uiRichTextBox2.BackColor = System.Drawing.SystemColors.Control;
            this.uiRichTextBox2.Cursor = System.Windows.Forms.Cursors.Default;
            this.uiRichTextBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiRichTextBox2.FillColor = System.Drawing.Color.Gray;
            this.uiRichTextBox2.Font = new System.Drawing.Font("宋体", 16F);
            this.uiRichTextBox2.ForeColor = System.Drawing.Color.White;
            this.uiRichTextBox2.Location = new System.Drawing.Point(0, 0);
            this.uiRichTextBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiRichTextBox2.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiRichTextBox2.Name = "uiRichTextBox2";
            this.uiRichTextBox2.Padding = new System.Windows.Forms.Padding(2);
            this.uiRichTextBox2.ReadOnly = true;
            this.uiRichTextBox2.ScrollBarColor = System.Drawing.Color.Gray;
            this.uiRichTextBox2.ShowText = false;
            this.uiRichTextBox2.Size = new System.Drawing.Size(800, 450);
            this.uiRichTextBox2.Style = Sunny.UI.UIStyle.Custom;
            this.uiRichTextBox2.TabIndex = 0;
            this.uiRichTextBox2.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.uiRichTextBox2.ZoomScaleRect = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.uiRichTextBox2.TextChanged += new System.EventHandler(this.uiRichTextBox2_TextChanged);
            // 
            // Player1_Client
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.panel1);
            this.Name = "Player1_Client";
            this.Text = "Player2";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private Sunny.UI.UIRichTextBox uiRichTextBox2;
    }
}