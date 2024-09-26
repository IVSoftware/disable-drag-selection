If you're willing to extend `DataGridView` you could try detecting mouse drag and disabling cell selection for a brief interval after detection. It worked with the test code shown below.

```
class DataGridViewEx : DataGridView
{
    protected override void SetSelectedCellCore(int columnIndex, int rowIndex, bool selected)
    {
        bool disabledByOneCellPerColumnRule =
            ModifierKeys == Keys.Control &&
            SelectedCells.OfType<DataGridViewCell>().Any(_ => _.ColumnIndex == columnIndex);

        base.SetSelectedCellCore(
            columnIndex, 
            rowIndex,
            selected && !(disabledByOneCellPerColumnRule || _wdtMove.Running));
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (MouseButtons == MouseButtons.Left) _wdtMove.StartOrRestart();
        base.OnMouseMove(e);
    }
    // <PackageReference Include="IVSoftware.Portable.WatchdogTimer" Version="1.2.1" />
    WatchdogTimer _wdtMove = new WatchdogTimer{ Interval = TimeSpan.FromMilliseconds(250)};
}
```

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
