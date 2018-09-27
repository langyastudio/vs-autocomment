namespace HKFY.AutoComment2015
{
    partial class InputBox
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
            this._txtBoxRow = new System.Windows.Forms.TextBox();
            this._txtBoxCol = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnCancle = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _txtBoxRow
            // 
            this._txtBoxRow.Location = new System.Drawing.Point(56, 16);
            this._txtBoxRow.MaxLength = 2;
            this._txtBoxRow.Name = "_txtBoxRow";
            this._txtBoxRow.Size = new System.Drawing.Size(100, 21);
            this._txtBoxRow.TabIndex = 0;
            this._txtBoxRow.Text = "3";
            this._txtBoxRow.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this._txtBoxRow_KeyPress);
            // 
            // _txtBoxCol
            // 
            this._txtBoxCol.Location = new System.Drawing.Point(56, 43);
            this._txtBoxCol.MaxLength = 2;
            this._txtBoxCol.Name = "_txtBoxCol";
            this._txtBoxCol.Size = new System.Drawing.Size(100, 21);
            this._txtBoxCol.TabIndex = 0;
            this._txtBoxCol.Text = "3";
            this._txtBoxCol.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this._txtBoxCol_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "行数:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "列数:";
            // 
            // btnCancle
            // 
            this.btnCancle.Location = new System.Drawing.Point(260, 41);
            this.btnCancle.Name = "btnCancle";
            this.btnCancle.Size = new System.Drawing.Size(75, 23);
            this.btnCancle.TabIndex = 2;
            this.btnCancle.Text = "取消";
            this.btnCancle.UseVisualStyleBackColor = true;
            this.btnCancle.Click += new System.EventHandler(this.btnCancle_Click);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(260, 12);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "确定";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // InputBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(364, 74);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancle);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._txtBoxCol);
            this.Controls.Add(this._txtBoxRow);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputBox";
            this.ShowIcon = false;
            this.Text = "插入表格";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _txtBoxRow;
        private System.Windows.Forms.TextBox _txtBoxCol;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnCancle;
        private System.Windows.Forms.Button btnOk;
    }
}