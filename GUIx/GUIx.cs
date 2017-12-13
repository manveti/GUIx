ï»¿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GUIx {
    //public class SpinBox : Control...

    class StringDialogWindow : Window {
        public String value = null;

        private TextBox valBox;

        public StringDialogWindow(String title, String prompt, String value = "") {
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.Title = title;
            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition());
            g.ColumnDefinitions.Add(new ColumnDefinition());
            RowDefinition rd = new RowDefinition();
            rd.Height = GridLength.Auto;
            g.RowDefinitions.Add(rd);
            Label promptBox = new Label();
            promptBox.Content = prompt;
            Grid.SetRow(promptBox, 0);
            Grid.SetColumn(promptBox, 0);
            g.Children.Add(promptBox);
            this.valBox = new TextBox();
            this.valBox.Width = 100;
            if (value != null) {
                this.valBox.Text = value;
            }
            Grid.SetRow(this.valBox, 0);
            Grid.SetColumn(this.valBox, 1);
            g.Children.Add(this.valBox);
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
            Grid.SetColumnSpan(butGrid, 2);
            g.Children.Add(butGrid);
            this.Content = g;
            this.Loaded += this.onLoad;
        }

        private void doOk(object sender, RoutedEventArgs e) {
            this.value = this.valBox.Text;
            this.Close();
        }

        private void doCancel(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void onLoad(object sender, RoutedEventArgs e) {
            valBox.Focus();
        }
    }

    static class SimpleDialog {
        public static String askString(String title, String prompt, String value = "") {
            StringDialogWindow dlg = new StringDialogWindow(title, prompt, value);
            dlg.ShowDialog();
            return dlg.value;
        }
    }

#if false
    //ideally, we can generalize StringDialogWindow to something like the following:
    //(we're reading the input from a text box of some kind for everything but bool, so value could almost be a string regardless of type)

    enum ValueType {
        STRING,
        BOOL,
        INT,
        FLOAT,
        LIST
    };

    class Value {
        public String prompt;
        public ValueType type;
        public double min = -Int32.MaxValue, max = Int32.MaxValue, step = 1;
        public String[] options = {};
        public bool readOnly = false;
        private object value;

        public Value(String prompt, ValueType type, object value = null) {
            this.prompt = prompt;
            this.type = type;
            this.value = value;
            if (value == null) {
                switch (type) {
                case ValueType.STRING:
                case ValueType.LIST:
                    this.value = "";
                    break;
                case ValueType.BOOL:
                    this.value = false;
                    break;
                case ValueType.INT:
                case ValueType.FLOAT:
                    this.value = 0;
                    break;
                }
            }
        }

        public Value(String prompt, ValueType type, double min = -Int32.MaxValue, double max = Int32.MaxValue, double step = 1, double value = 0) {
            if ((type != ValueType.INT) && (type != ValueType.FLOAT)) {
                throw new ArgumentException("The min, max, and step parameters are only valid for INT and FLOAT Values");
            }
            this.prompt = prompt;
            this.type = type;
            this.min = min;
            this.max = max;
            this.step = step;
            this.value = value;
        }

        public Value(String prompt, ValueType type, String[] options, bool readOnly = false, String value = "") {
            if (type != ValueType.LIST) {
                throw new ArgumentException("The options and readOnly parameters are only valid for LIST Values");
            }
            this.prompt = prompt;
            this.type = type;
            this.options = options;
            this.readOnly = readOnly;
            this.value = value;
        }

        public String getString() {
            if ((this.type != ValueType.STRING) && (this.type != ValueType.LIST)) {
                throw new InvalidOperationException("The getString method is only valid for STRING and LIST Values");
            }
            return (String)(this.value);
        }

        public bool getBool() {
            if (this.type != ValueType.BOOL) {
                throw new InvalidOperationException("The getBool method is only valid for BOOL Values");
            }
            return (bool)(this.value);
        }

        public int getInt() {
            if (this.type != ValueType.INT) {
                throw new InvalidOperationException("The getInt method is only valid for INT Values");
            }
            return (int)(this.value);
        }

        public double getFloat() {
            if (this.type != ValueType.FLOAT) {
                throw new InvalidOperationException("The getFloat method is only valid for FLOAT Values");
            }
            return (double)(this.value);
        }
    }

    class DialogWindow : Window {
        public Value[] vals;

        public DialogWindow(String title, Value[] vals) {
            RowDefinition rd;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.Title = title;
            this.vals = vals;
            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition());
            g.ColumnDefinitions.Add(new ColumnDefinition());
            for (int i = 0; i < vals.Length; i++) {
                rd = new RowDefinition();
                rd.Height = GridLength.Auto;
                g.RowDefinitions.Add(rd);
                Label prompt = new Label();
                prompt.Content = vals[i].prompt;
                Grid.SetRow(prompt, i);
                Grid.SetColumn(prompt, 0);
                g.Children.Add(prompt);
                //add input box
            }
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
            Grid.SetRow(okBut, 0);
            Grid.SetColumn(okBut, 0);
            butGrid.Children.Add(okBut);
            Button cancelBut = new Button();
            cancelBut.Content = "Cancel";
            Grid.SetRow(cancelBut, 0);
            Grid.SetColumn(cancelBut, 1);
            butGrid.Children.Add(cancelBut);
            Grid.SetRow(butGrid, vals.Length);
            Grid.SetColumn(butGrid, 0);
            Grid.SetColumnSpan(butGrid, 2);
            g.Children.Add(butGrid);
            this.Content = g;
        }
    }

    static class SimpleDialog {
        public static String askString(String title, String prompt, String value = "") {
            Value[] vals = { new Value(prompt, ValueType.STRING, value) };
            DialogWindow dlg = new DialogWindow(title, vals);
            dlg.ShowDialog();
            return vals[0].getString();
        }
    }
#endif
}
