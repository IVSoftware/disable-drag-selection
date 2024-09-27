Your post states:

> they should only be able to pick 1 cell per column.

As your image shows, the problem is that dragging over multiple cells with `MultiSelect` enabled is going to break that rule by selecting "multiple cells per column".
___
The general approach you've taken in your code is to deselect after the fact. It might be even better to prevent the cells that would break the rule from being selected in the first place, and it's straightforward to do this by making a lightweight extended class for `DataGridView`, and then detecting mouse drag and disabling cell selection for a brief interval after detection. I used the minimal test code shown below to verify that this works as intended.

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

___

#### Variant - Drag-Select follows "One Cell per Column" rule

Here's a slight variation. See if it's more intuitive this way or the other.

[![variant][1]][1]

```
class DataGridViewEx : DataGridView
{
    protected override void SetSelectedCellCore(int columnIndex, int rowIndex, bool selected)
    {
        bool disabledByOneCellPerColumnRule =
            ((ModifierKeys == Keys.Control) || _wdtMove.Running) &&
            SelectedCells.OfType<DataGridViewCell>().Any(_ => _.ColumnIndex == columnIndex);

        base.SetSelectedCellCore(
            columnIndex,
            rowIndex,
            selected && !disabledByOneCellPerColumnRule);
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


  [1]: https://i.sstatic.net/7ABRz2qe.png