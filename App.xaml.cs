
namespace MVVM
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
            //Establece la pagina principal como NavigationPage
            MainPage = new NavigationPage(new MVVM.Views.ProductPage());

            
        }
    }
}
