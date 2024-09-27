
using IVSoftware.Portable;
using System.ComponentModel;

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
            base.OnCellMouseDown(e);
            SingleSelectInColumn(e.ColumnIndex, e.RowIndex);
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
                    cell.Selected = cell.RowIndex == rowIndex;
                }
            }
        }
        int? _allowedColumn = null;
        protected override void OnCellMouseEnter(DataGridViewCellEventArgs e)
        {
            base.OnCellMouseEnter(e);
            if (MouseButtons == MouseButtons.Left)
            {
                _allowedColumn = e.ColumnIndex;
                BeginInvoke(()=> SingleSelectInColumn(e.ColumnIndex, e.RowIndex ));
            }
            else _allowedColumn = null;
        }
        protected override void SetSelectedCellCore(int columnIndex, int rowIndex, bool selected)
        {
            if (_wdtMove.Running && columnIndex != _allowedColumn)
            {
                return;
            }
            base.SetSelectedCellCore(
                columnIndex,
                rowIndex,
                selected);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (MouseButtons == MouseButtons.Left) _wdtMove.StartOrRestart();
            base.OnMouseMove(e);
        }
        // <PackageReference Include="IVSoftware.Portable.WatchdogTimer" Version="1.2.1" />
        WatchdogTimer _wdtMove = new WatchdogTimer { Interval = TimeSpan.FromMilliseconds(250) };
    }
}
