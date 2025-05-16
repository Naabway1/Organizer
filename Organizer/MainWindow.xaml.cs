using Organizer.Core;
using Organizer.ViewModel;
using System.Windows;


namespace Organizer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var navigation = new Navigation(myframe);
            DataContext = new MainVM(navigation);
            navigation.NavigateTo("MainPage", DataContext);
        }
    }
}
