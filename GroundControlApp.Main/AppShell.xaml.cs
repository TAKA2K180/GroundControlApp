namespace GroundControlApp.Main
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(Views.AdminPanelPage), typeof(Views.AdminPanelPage));
            Routing.RegisterRoute(nameof(Views.MenuManagementPage), typeof(Views.MenuManagementPage));
            Routing.RegisterRoute(nameof(Views.IngredientsPage), typeof(Views.IngredientsPage));
            Routing.RegisterRoute(nameof(Views.StocksPage), typeof(Views.StocksPage));
            Routing.RegisterRoute(nameof(Views.OrdersPage), typeof(Views.OrdersPage));
            Routing.RegisterRoute(nameof(Views.SalesPage), typeof(Views.SalesPage));
            Routing.RegisterRoute(nameof(Views.EmployeeTimePage), typeof(Views.EmployeeTimePage));
            Routing.RegisterRoute(nameof(Views.PayrollPage), typeof(Views.PayrollPage));
            Routing.RegisterRoute(nameof(Views.UsersRolesPage), typeof(Views.UsersRolesPage));
            CurrentItem = LoadingShellContent;
        }
    }
}
