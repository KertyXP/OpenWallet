using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpenWallet.WinForm
{
    public static class DataGridViewExtension
    {

        public static List<int> GetSelectedRowIndexes(this DataGridView dgv)
        {
            var list = new List<int>();
            foreach (DataGridViewRow row in dgv.SelectedRows)
            {
                list.Add(row.Index);
            }
            return list;
        }



    }
}
