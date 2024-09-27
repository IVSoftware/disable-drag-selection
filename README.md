Your post states:

> they should only be able to pick 1 cell per column.

As your image shows, the problem is that dragging over multiple cells with `MultiSelect` enabled is going to break that rule by selecting "multiple cells per column".
___

In other words, we need to be able to preview when the cell selection is changing, and have some criteria for whether to allow the change to occur. The ideal place to do this is in `DataGridView.SetSelectedCallCore` and since this is a protected method that fires no event, it's necessary to make a lightweight extended class for `DataGridView`.

The main feature of `DataGridViewEx` is to be able to `SingleSelectInColumn(int columnIndex, int rowIndex)`. As you do in your code, this needs to happen `OnCellMouseDown`. But to fix your problem with the drag-select, we _also_ need to detect `OnCellMouseEnter`.

1. If the Mouse left button is down when the cell is entered, we know it's a drag-select.
2. In this case, we want to allow selection changes only in the column that the mouse is currently over.
3. Then, using `BeginInvoke`, we post the `SingleSelectInColumn(int columnIndex, int rowIndex)` at the end of the message queue.

I used the minimal test code shown below to verify that this works as intended.

```
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
```

___

_You would then just manually edit your MainForm.Designer.cs file, substituting `DataGridViewEx` for `DataGridView` in two places._
___

#### Test Code

```
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

    public int col1 { get; set; }
    public int col2 { get; set; }
    public int col3 { get; set; }
}
```