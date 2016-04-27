using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Stability.Model;

namespace Stability
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class PatientControl
    {
        public bool FieldsCanBeNull { get; set; }
        public Brush IfEmptyBorderBrush { get; set; }
        private readonly Brush borderCurrBrush;
        private List<TextBox> _list;

        public PatientControl()
        {
            InitializeComponent();
            FieldsCanBeNull = false;

            borderCurrBrush = text_Name.BorderBrush;
            IfEmptyBorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));

            _list = new List<TextBox>(new[] 
            {
                text_Name,
                text_Surname,
                text_Patronymic,
                text_Height,
                text_Weight,
                text_Street,
                text_House,
                text_Flat
            });

            foreach (var t in _list)
            {
                t.GotFocus += OnGotFocus;
            }
            BirthDatePicker.GotFocus += OnGotFocus;
            BirthDatePicker.DisplayDateEnd = DateTime.Now;
        }

        private void OnGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            ((Control) sender).BorderBrush = borderCurrBrush;
        }

        public PatientControl(cPatient p) : base()
        {
            SetToForm(p);
        }

        public void SetToForm(cPatient p)
        {
            if (p == null) return; 
            ClearAll();
            text_Name.Text = p.Name;
            text_Surname.Text = p.Surname;
            text_Patronymic.Text = p.Patronymic;
            BirthDatePicker.SelectedDate = p.Birthdate;
            if (p.Sex)
                rad_Male.IsChecked = true;
            else
                rad_Female.IsChecked = true;
            text_Height.Text = p.Height.ToString();
            text_Street.Text = p.Address.Street;
            text_House.Text = p.Address.House.ToString();
            text_Flat.Text = p.Address.Flat.ToString();
        }

        private void text_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = true;
            try
            {
                if (Char.IsDigit(e.Text, 0))
                    e.Handled = false;
            }
            catch
            {
                e.Handled = true;
            }
        }

        private void text_Weight_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = true;
            try
            {
                if (((e.Text == ".") && (!((TextBox)sender).Text.Contains("."))) || (Char.IsDigit(e.Text, 0)))
                    e.Handled = false;
            }
            catch
            {
                e.Handled = true;
            }
        }

        public cPatient GetPatient(out string res)
        {
           //var p = new cPatient(){Name = text_Name.Text,Surname = text_Surname.Text,}
            Int16 height;
            double weight;

            res = CheckIfEmpty();
            if (res != "OK")
                return null;

            res = CheckParams(out height, out weight);
            if (res != "OK")
                return null;

            cAddress adr;
            res = CheckAdr(out adr);
            if (res != "OK")
                return null;

            var sex = (bool) rad_Male.IsChecked;
            var pat = new cPatient()
            {
                Address = adr,
                Birthdate = BirthDatePicker.DisplayDate,
                Height = height,
                Name = text_Name.Text,
                Surname = text_Surname.Text,
                Patronymic = text_Patronymic.Text,
                Sex = sex
            };

            if (pat.Age == 0)
            {
                res = "Возраст пациента указан не верно";
                BirthDatePicker.BorderBrush = IfEmptyBorderBrush;
                return null;
            }

            return pat;
        }

        private string CheckParams(out Int16 h, out double w)
        {
            w = 0;

            if (Int16.TryParse(text_Height.Text, out h))
            {
                if (h > 250)
                    return "Рост не может превышать 250 см.";
                if (h == 0)
                    return "Рост не может быть равным нулю";
            }
            else
                return "Значение роста заполнено неверно!";


            if (Double.TryParse(text_Weight.Text, NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-GB"), out w))
            {
                if (w < 0.1)
                    return "Значение веса слишком мало!";
               
                if (w > 150.0)
                    return "Значение веса не может превышать 150 кг.!";
                   
            }
            else
                return "Значение веса введено неверно!";

            return "OK";
        }

        private string CheckAdr(out cAddress adr)
        {
            Int16 h,f;
            adr = null;
            if (Int16.TryParse(text_House.Text, out h))
            {
               if (h == 0)
                    return "Номер дома не может равняться нулю";
            }
            else
                return "Значение заполнено неверно!";

            if (Int16.TryParse(text_Flat.Text, out f))
            {
                if (h == 0)
                    return "Номер квартиры не может равняться нулю";
            }
            else
                return "Значение заполнено неверно!";

            adr= new cAddress(){Street = text_Street.Text,Flat = f,House = h};
            return "OK";
        }

        private void SetCurrBrush()
        {
            _list.ForEach(box => box.BorderBrush = borderCurrBrush);
        }

        public void ClearAll()
        {
            SetCurrBrush();
            _list.ForEach(box => box.Text="");
            BirthDatePicker.SelectedDate = null;
            rad_Male.IsChecked = true;
        }

        private string CheckIfEmpty()
        {
            string res = "OK";
            if (FieldsCanBeNull) 
                return res;
            
           foreach (var t in _list)
            {
                if (t.Text.Equals(""))
                {
                    t.BorderBrush = IfEmptyBorderBrush;
                    res = "Необходимо заполнить все информационные поля";
                }
            }
            return res;
        }
    }
}
