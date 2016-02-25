namespace DrawingEngine
{
    partial class Grouping
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
            this.cbObjects = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.lblErrors = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cbObjects
            // 
            this.cbObjects.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbObjects.FormattingEnabled = true;
            this.cbObjects.Location = new System.Drawing.Point(12, 12);
            this.cbObjects.Name = "cbObjects";
            this.cbObjects.Size = new System.Drawing.Size(195, 21);
            this.cbObjects.TabIndex = 41;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(110, 144);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(97, 23);
            this.button1.TabIndex = 42;
            this.button1.Text = "Выбрать";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lblErrors
            // 
            this.lblErrors.AutoSize = true;
            this.lblErrors.Location = new System.Drawing.Point(12, 49);
            this.lblErrors.Name = "lblErrors";
            this.lblErrors.Size = new System.Drawing.Size(0, 13);
            this.lblErrors.TabIndex = 43;
            // 
            // Grouping
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(219, 179);
            this.Controls.Add(this.lblErrors);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cbObjects);
            this.Name = "Grouping";
            this.Text = "Группировка";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbObjects;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblErrors;
    }
}