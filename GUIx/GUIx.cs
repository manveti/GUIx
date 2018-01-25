using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace GUIx {
    public class SpinBox : Grid {
        public readonly TextBox textBox;
        public readonly ScrollBar scrollBar;

        public SpinBox() {
            this.RowDefinitions.Add(new RowDefinition());
            this.ColumnDefinitions.Add(new ColumnDefinition());
            this.textBox = new TextBox();
            this.textBox.Text = "0";
            this.textBox.TextChanged += this.handleTextChange;
            this.textBox.PreviewTextInput += this.handleInput;
            Grid.SetRow(this.textBox, 0);
            Grid.SetColumn(this.textBox, 0);
            this.Children.Add(this.textBox);
            ColumnDefinition cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            this.ColumnDefinitions.Add(cd);
            this.scrollBar = new ScrollBar();
            this.scrollBar.Minimum = double.MinValue;
            this.scrollBar.Maximum = double.MaxValue;
            this.scrollBar.SmallChange = 1;
            this.scrollBar.Value = 0;
            this.scrollBar.ValueChanged += this.handleScroll;
            Grid.SetRow(this.scrollBar, 0);
            Grid.SetColumn(this.scrollBar, 1);
            this.Children.Add(this.scrollBar);
        }

        // handlers
        public void handleTextChange(object sender, TextChangedEventArgs e) {
            double newValue;
            if (this.textBox.Text == "-") { return; }
            if (!double.TryParse(this.textBox.Text, out newValue)) {
                this.textBox.Text = "" + this.Value;
                return;
            }
            if (newValue == this.Value) { return; }
            this.Value = newValue;
        }

        public void handleInput(object sender, TextCompositionEventArgs e) {
            StringBuilder s = new StringBuilder(this.textBox.Text);
            s.Remove(this.textBox.SelectionStart, this.textBox.SelectionLength);
            s.Insert(this.textBox.SelectionStart, e.Text);
            double newValue;
            if ((this.Minimum < 0) && (s.ToString() == "-")) { return; }
            if (!double.TryParse(s.ToString(), out newValue)) {
                e.Handled = true;
                return;
            }
        }

        public void handleScroll(object sender, RoutedPropertyChangedEventArgs<double> e) {
            String newText = "" + this.Value;
            if (this.textBox.Text == newText) { return; }
            this.textBox.Text = newText;
        }

        // property delegates
        public int CaretIndex {
            get { return this.textBox.CaretIndex; }
            set { this.textBox.CaretIndex = value; }
        }

        public String SelectedText {
            get { return this.textBox.SelectedText; }
            set { this.textBox.SelectedText = value; }
        }

        public int SelectionLength {
            get { return this.textBox.SelectionLength; }
            set { this.textBox.SelectionLength = value; }
        }

        public int SelectionStart {
            get { return this.textBox.SelectionStart; }
            set { this.textBox.SelectionStart = value; }
        }

        public String Text {
            get { return this.textBox.Text; }
            set { this.textBox.Text = value; }
        }

        public double Maximum {
            get { return -this.scrollBar.Minimum; }
            set { this.scrollBar.Minimum = -value; }
        }

        public double Minimum {
            get { return -this.scrollBar.Maximum; }
            set { this.scrollBar.Maximum = -value; }
        }

        public double SmallChange {
            get { return this.scrollBar.SmallChange; }
            set { this.scrollBar.SmallChange = value; }
        }

        public double Value {
            get { return -this.scrollBar.Value; }
            set { this.scrollBar.Value = -value; }
        }

        // handler delegates
        public new event KeyEventHandler KeyDown {
            add { this.textBox.KeyDown += value; }
            remove { this.textBox.KeyDown -= value; }
        }

        public new event KeyEventHandler KeyUp {
            add { this.textBox.KeyUp += value; }
            remove { this.textBox.KeyUp -= value; }
        }

        public new event KeyboardFocusChangedEventHandler LostKeyboardFocus {
            add { this.textBox.LostKeyboardFocus += value; }
            remove { this.textBox.LostKeyboardFocus -= value; }
        }

        public event TextChangedEventHandler TextChanged {
            add { this.textBox.TextChanged += value; }
            remove { this.textBox.TextChanged -= value; }
        }

        public event RoutedPropertyChangedEventHandler<double> ValueChanged {
            add { this.scrollBar.ValueChanged += value; }
            remove { this.scrollBar.ValueChanged -= value; }
        }
    }


    public enum QueryType {
        STRING,
        BOOL,
        INT,
        FLOAT,
        LIST
    }

    public class QueryPrompt {
        public String prompt;
        public QueryType type;
        public object value;
        public double min, max, step;
        public String[] values;
        public bool canEdit;

        public QueryPrompt(String prompt, QueryType type, object value = null, double min = double.MinValue, double max = double.MaxValue, double step = 1,
                            String[] values = null, bool canEdit = false) {
            this.prompt = prompt;
            this.type = type;
            this.value = value;
            this.min = min;
            this.max = max;
            this.step = step;
            this.values = values;
            this.canEdit = canEdit;
        }
    }

    class QueryBox {
        public QueryType type;
        public UIElement box;

        public QueryBox(QueryPrompt prompt) {
            this.type = prompt.type;
            switch (this.type) {
            case QueryType.STRING:
                box = new TextBox();
                ((TextBox)box).MinWidth = 100;
                if (prompt.value != null) {
                    ((TextBox)box).Text = (String)(prompt.value);
                }
                break;
            case QueryType.BOOL:
                box = new CheckBox();
                if (prompt.value != null) {
                    ((CheckBox)box).IsChecked = (bool)(prompt.value);
                }
                break;
            case QueryType.INT:
                if (prompt.min < int.MinValue) { prompt.min = int.MinValue; }
                if (prompt.max > int.MaxValue) { prompt.max = int.MaxValue; }
                if (prompt.value != null) { prompt.value = (double)((int)(prompt.value)); }
                goto case QueryType.FLOAT; // numeric types use the same UI element; fall through to FLOAT
            case QueryType.FLOAT:
                box = new SpinBox();
                ((SpinBox)box).MinWidth = 100;
                ((SpinBox)box).Minimum = prompt.min;
                ((SpinBox)box).Maximum = prompt.max;
                ((SpinBox)box).SmallChange = prompt.step;
                if (prompt.value != null) {
                    ((SpinBox)box).Value = (double)(prompt.value);
                }
                break;
            case QueryType.LIST:
                box = new ComboBox();
                ((ComboBox)box).MinWidth = 100;
                ((ComboBox)box).IsEditable = prompt.canEdit;
                bool gotValue = false;
                for (int i = 0; i < prompt.values.Length; i++) {
                    ((ComboBox)box).Items.Add(prompt.values[i]);
                    if (prompt.values[i] == (String)(prompt.value)) {
                        gotValue = true;
                    }
                }
                if ((gotValue) && (prompt.value != null)) {
                    ((ComboBox)box).Text = (String)(prompt.value);
                }
                else if (prompt.values.Length > 0) {
                    ((ComboBox)box).Text = prompt.values[0];
                }
                break;
            }
        }

        public object getValue() {
            switch (this.type) {
            case QueryType.STRING:
                return ((TextBox)box).Text;
            case QueryType.BOOL:
                return ((CheckBox)box).IsChecked;
            case QueryType.INT:
                return (int)(((SpinBox)box).Value);
            case QueryType.FLOAT:
                return ((SpinBox)box).Value;
            case QueryType.LIST:
                return ((ComboBox)box).Text;
            }
            return null;
        }
    }

    abstract class QueryDialogBase : Window {
        protected Grid queryGrid;
        public bool valid;

        public QueryDialogBase(String title) {
            this.valid = false;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.Title = title;
            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition());
            RowDefinition rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            g.RowDefinitions.Add(rd);
            this.queryGrid = new Grid();
            Grid.SetRow(this.queryGrid, 0);
            Grid.SetColumn(this.queryGrid, 0);
            g.Children.Add(this.queryGrid);
            rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            g.RowDefinitions.Add(rd);
            Grid butGrid = new Grid();
            butGrid.HorizontalAlignment = HorizontalAlignment.Right;
            butGrid.ColumnDefinitions.Add(new ColumnDefinition());
            butGrid.ColumnDefinitions.Add(new ColumnDefinition());
            butGrid.RowDefinitions.Add(new RowDefinition());
            Button okBut = new Button();
            okBut.Content = "OK";
            okBut.Click += this.doOk;
            Grid.SetRow(okBut, 0);
            Grid.SetColumn(okBut, 0);
            butGrid.Children.Add(okBut);
            Button cancelBut = new Button();
            cancelBut.Content = "Cancel";
            cancelBut.Click += this.doCancel;
            Grid.SetRow(cancelBut, 0);
            Grid.SetColumn(cancelBut, 1);
            butGrid.Children.Add(cancelBut);
            Grid.SetRow(butGrid, 1);
            Grid.SetColumn(butGrid, 0);
            g.Children.Add(butGrid);
            this.Content = g;
        }

        private void doOk(object sender, RoutedEventArgs e) {
            this.valid = true;
            this.Close();
        }

        private void doCancel(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }

    class QueryDialogWindow : QueryDialogBase {
        protected QueryBox[] boxes;

        public QueryDialogWindow(String title, QueryPrompt[] prompts) : base(title) {
            ColumnDefinition cd = new ColumnDefinition();
            cd.Width = GridLength.Auto;
            this.queryGrid.ColumnDefinitions.Add(cd);
            this.queryGrid.ColumnDefinitions.Add(new ColumnDefinition());
            this.boxes = new QueryBox[prompts.Length];
            for (int i = 0; i < prompts.Length; i++) {
                RowDefinition rd = new RowDefinition();
                rd.Height = GridLength.Auto;
                this.queryGrid.RowDefinitions.Add(rd);
                Label promptBox = new Label();
                promptBox.Content = prompts[i].prompt;
                Grid.SetRow(promptBox, i);
                Grid.SetColumn(promptBox, 0);
                this.queryGrid.Children.Add(promptBox);
                this.boxes[i] = new QueryBox(prompts[i]);
                Grid.SetRow(this.boxes[i].box, i);
                Grid.SetColumn(this.boxes[i].box, 1);
                this.queryGrid.Children.Add(this.boxes[i].box);
            }
            this.Loaded += this.onLoad;
        }

        private void onLoad(object sender, RoutedEventArgs e) {
            if (this.boxes.Length <= 0) { return; }
            this.boxes[0].box.Focus();
        }

        public object[] getValues() {
            if (!this.valid) { return null; }
            object[] values = new object[this.boxes.Length];
            for (int i = 0; i < this.boxes.Length; i++) {
                values[i] = this.boxes[i].getValue();
            }
            return values;
        }
    }

    static class SimpleDialog {
        public static object[] askCompound(String title, QueryPrompt[] prompts, Window owner = null) {
            QueryDialogWindow dlg = new QueryDialogWindow(title, prompts);
            if (owner != null) {
                dlg.Owner = owner;
            }
            dlg.ShowDialog();
            return dlg.getValues();
        }

        private static object askSingle(String title, QueryPrompt prompt, Window owner = null) {
            object[] values = askCompound(title, new QueryPrompt[] { prompt }, owner);
            if ((values != null) && (values.Length >= 0)) { return values[0]; }
            return null;
        }

        private static object askSingle(String title, String prompt, QueryType type, object value = null, Window owner = null) {
            return askSingle(title, new QueryPrompt(prompt, type, value), owner);
        }

        public static String askString(String title, String prompt, String value = "", Window owner = null) {
            return (String)askSingle(title, prompt, QueryType.STRING, value, owner);
        }

        public static bool? askBool(String title, String prompt, bool value = false, Window owner = null) {
            return (bool?)askSingle(title, prompt, QueryType.BOOL, value, owner);
        }

        public static int? askInt(String title, String prompt, int? value = null, Window owner = null) {
            return (int?)((double?)askSingle(title, prompt, QueryType.INT, (double?)value, owner));
        }

        public static int? askInt(String title, String prompt, int? value = null, int min = int.MinValue, int max = int.MaxValue, int step = 1, Window owner = null) {
            return (int?)((double?)askSingle(title, new QueryPrompt(prompt, QueryType.INT, value, min, max, step), owner));
        }

        public static double? askFloat(String title, String prompt, double? value = null, Window owner = null) {
            return (double?)askSingle(title, prompt, QueryType.FLOAT, value, owner);
        }

        public static double? askFloat(String title, String prompt, int? value = null, int min = int.MinValue, int max = int.MaxValue, int step = 1, Window owner = null) {
            return (double?)askSingle(title, new QueryPrompt(prompt, QueryType.FLOAT, value, min, max, step), owner);
        }

        public static String askList(String title, String prompt, String[] values, bool canEdit = false, String value = null, Window owner = null) {
            return (String)askSingle(title, new QueryPrompt(prompt, QueryType.LIST, value, values: values, canEdit: canEdit), owner);
        }
    }
}
