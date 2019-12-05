using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HKFY.AutoComment2019
{
    public partial class InputBox : Form
    {
        public InputBox()
        {
            InitializeComponent();
        }

        //控制输入数字
        private void _txtBoxRow_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && (Keys)e.KeyChar != Keys.Back)
            {
                e.Handled = true;
                return;
            }
        }

        //控制输入数字
        private void _txtBoxCol_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && (Keys)e.KeyChar != Keys.Back)
            {
                e.Handled = true;
                return;
            }
        }

        //行数
        public int RowNum
        {
            get 
            {
                int rowNum = 0;
                if (int.TryParse(_txtBoxRow.Text, out rowNum) && rowNum > 0)
                    return (rowNum);
                return (3);
            }
        }

        //列数
        public int ColNum
        {
            get
            {
                int colNum = 0;
                if (int.TryParse(_txtBoxCol.Text, out colNum) && colNum > 0)
                    return (colNum);
                return (3);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnCancle_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
