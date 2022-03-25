using System.Collections.Generic;
using System.Windows.Forms;

namespace OpenWallet.WinForm
{

    public enum AcceptedState
    {
        All,
        Visible,
        Selected
    }

    public static class DataGridViewExtension
    {

        public static List<int> GetSelectedRowIndexes(this DataGridView dgv)
        {
            var list = new List<int>();
            foreach (DataGridViewRow row in dgv.SelectedRows)
            {
                if (row.Visible)
                    list.Add(row.Index);
            }
            return list;
        }
        public static void UnselectAllRows(this DataGridView dgv)
        {
            dgv.ClearSelection();
        }


        public static IEnumerable<T> GetValuesAsT<T>(this DataGridView dgv, AcceptedState state, int colIndex = 0) where T: class
        {
            for (int i = 0; i < dgv.RowCount; i++)
            {
                if (state == AcceptedState.Visible && dgv.Rows[i].Visible == false)
                    continue;

                if (state == AcceptedState.Selected && dgv.Rows[i].Selected == false)
                    continue;

                var trade = dgv[colIndex, i].Value as T;

                yield return trade;
            }
        }



    }
}
