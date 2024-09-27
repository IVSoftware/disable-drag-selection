using System.ComponentModel;
using System.Diagnostics;

namespace disable_drag_selection
{
    public partial class MainForm : Form
    {
        public MainForm() => InitializeComponent();
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            dataGridView.DataSource = Records;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            for (int i = 0; i< 10; i++)
            {
                Records.AddNew();
            }
        }
        BindingList<Record> Records { get; } = new BindingList<Record>();
    }
    
    public class Record
    {
        static int _debugCount = 1;
        public Record()
        {
            col1 = col2 = col3 = _debugCount ++;
        }

        public int col1 { get; internal set; }
        public int col2 { get; internal set; }
        public int col3 { get; internal set; }
    }

    class DataGridViewEx : DataGridView
    {
        protected override void OnCellMouseDown(DataGridViewCellMouseEventArgs e)
        {
            var cellToggle = 
                e.ColumnIndex >= 0 && e.RowIndex >= 0 ?
                this[e.ColumnIndex, e.RowIndex] : null;
            if (ModifierKeys == Keys.Control && cellToggle?.Selected == true)
            {
                cellToggle.Selected = false;
            }
            else
            {
                BeginInvoke(() => SingleSelectInColumn(e.ColumnIndex, e.RowIndex));
            }
            // CRITICAL - Do 'not' call base class method!
        }
        private void SingleSelectInColumn(int columnIndex, int rowIndex)
        {
            if (columnIndex >= 0 && rowIndex >= 0)
            {
                var cellsInColumn = 
                    Rows
                    .OfType<DataGridViewRow>()
                    .Select(_ => _.Cells[columnIndex]); 

                foreach (
                    var cell in
                    cellsInColumn )
                {
                    var sbSelected = cell.RowIndex == rowIndex;
                    if(!Equals(cell.Selected, sbSelected))
                    {
                        cell.Selected = sbSelected;
                    }
                }
                AllowedColumn = null;
            }
        }
        public int? AllowedColumn
        {
            get => _allowedColumn;
            set
            {
                if (!Equals(_allowedColumn, value))
                {
                    _allowedColumn = value;
                }
            }
        }

        int? _allowedColumn = null;
        protected override void OnCellMouseEnter(DataGridViewCellEventArgs e)
        {
            base.OnCellMouseEnter(e);
            if (MouseButtons == MouseButtons.Left)
            {
                AllowedColumn = e.ColumnIndex;
                BeginInvoke(()=> SingleSelectInColumn(e.ColumnIndex, e.RowIndex ));
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            // CRITICAL - We have to put this back the way we found it!
            AllowedColumn = null;
        }
        protected override void OnCellMouseLeave(DataGridViewCellEventArgs e)
        {
            base.OnCellMouseLeave(e);
            if (MouseButtons == MouseButtons.Left)
            {
                // CRITICAL - For example if the mouse is dragged outside the control.
                AllowedColumn = int.MinValue;
            }
        }
        protected override void SetSelectedCellCore(int columnIndex, int rowIndex, bool selected)
        {
            if (AllowedColumn is int validColumnIndex)
            {
                if(!Equals(columnIndex, validColumnIndex))
                {
                    return;
                }
            }
            base.SetSelectedCellCore(
                columnIndex,
                rowIndex,
                selected);
        }
    }
}
