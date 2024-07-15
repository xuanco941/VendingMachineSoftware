using ActUtlTypeLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestTransferDataPlcMitsu
{
    public partial class Form1 : Form
    {
        public ActUtlType plc;
        private ObservableValue _observableValue;

        public Form1()
        {
            InitializeComponent();
            this.plc = new ActUtlType();
            plc.ActLogicalStationNumber = 1;

            _observableValue = new ObservableValue();
            _observableValue.PropertyChanged += ObservableValue_PropertyChanged;

            textBox1.TextChanged += TextBox1_TextChanged;
            UpdateTextBox();
        }
        public int _value;

        private void UpdateTextBox()
        {
            textBox1.Text = _observableValue.Value.ToString();
        }

        // Sự kiện TextChanged của TextBox
        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox1.Text, out int result))
            {
                _observableValue.Value = result;
            }
        }

        // Sự kiện PropertyChanged của ObservableValue
        private void ObservableValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ObservableValue.Value))
            {
                UpdateTextBox();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            int value = plc.Open();
            if (value == 0)
            {
                button1.Text = "Kết nối thành công";
                plc.GetDevice("", out _value);
            }
        }




        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }
    }
    public class ObservableValue : INotifyPropertyChanged
    {
        private int _value;
        public event PropertyChangedEventHandler PropertyChanged;

        public int Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
