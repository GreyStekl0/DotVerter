using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVerter.UI;

public partial class Converter : ContentPage
{
    public Converter()
    {
        BindingContext = new ConverterViewModel();
        InitializeComponent();
    }
}