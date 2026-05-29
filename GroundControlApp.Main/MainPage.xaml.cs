namespace GroundControlApp.Main
{
    public partial class MainPage : ContentPage
    {
        private readonly ViewModels.MainPageViewModel viewModel;
        private bool hasLoaded;
        private bool clockStarted;

        public MainPage()
            : this(AppServices.GetRequiredService<ViewModels.MainPageViewModel>())
        {
        }

        public MainPage(ViewModels.MainPageViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            BindingContext = viewModel;
            SizeChanged += OnSizeChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (hasLoaded)
            {
                return;
            }

            hasLoaded = true;
            await viewModel.LoadAsync();

            if (!clockStarted)
            {
                clockStarted = true;
                Dispatcher.StartTimer(TimeSpan.FromSeconds(30), () =>
                {
                    viewModel.RefreshStatusClock();
                    return true;
                });
            }

            viewModel.RefreshStatusClock();
        }

        private void OnSizeChanged(object? sender, EventArgs e)
        {
            ApplyResponsiveLayout(Width, Height);
        }

        private void ApplyResponsiveLayout(double width, double height)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            var isLandscape = width > height;
            var isCompactHeight = height < 620;

            if (width < 720)
            {
                ApplyPhoneLayout();
                return;
            }

            if (isLandscape && isCompactHeight)
            {
                ApplyCompactLandscapeLayout(width);
                return;
            }

            if (width < 1100)
            {
                ApplyTabletLayout();
                return;
            }

            ApplyDesktopLayout();
        }

        private void ApplyDesktopLayout()
        {
            RootGrid.Padding = new Thickness(18);
            RootGrid.RowDefinitions.Clear();
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            RootGrid.ColumnDefinitions.Clear();
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1.45, GridUnitType.Star)));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(360)));

            HeaderPanel.SetValue(Grid.RowProperty, 0);
            HeaderPanel.SetValue(Grid.ColumnProperty, 0);
            HeaderPanel.SetValue(Grid.ColumnSpanProperty, 3);
            ProductPanel.SetValue(Grid.RowProperty, 1);
            ProductPanel.SetValue(Grid.ColumnProperty, 0);
            ProductPanel.SetValue(Grid.ColumnSpanProperty, 1);
            CartPanel.SetValue(Grid.RowProperty, 1);
            CartPanel.SetValue(Grid.ColumnProperty, 1);
            CartPanel.SetValue(Grid.ColumnSpanProperty, 1);
            PaymentPanel.SetValue(Grid.RowProperty, 1);
            PaymentPanel.SetValue(Grid.ColumnProperty, 2);
            PaymentPanel.SetValue(Grid.ColumnSpanProperty, 1);

            HeaderGrid.ColumnDefinitions.Clear();
            HeaderGrid.RowDefinitions.Clear();
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            HeaderGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            StatusBar.SetValue(Grid.RowProperty, 0);
            StatusBar.SetValue(Grid.ColumnProperty, 1);
            StatusBar.SetValue(Grid.ColumnSpanProperty, 1);
            HeaderActions.SetValue(Grid.RowProperty, 0);
            HeaderActions.SetValue(Grid.ColumnProperty, 2);

            ProductItemsLayout.Span = 3;
            ApplyPanelHeights(width: Width, height: Height, productRatio: 0.64, cartRatio: 0.48);
        }

        private void ApplyCompactLandscapeLayout(double width)
        {
            var height = Height;
            RootGrid.Padding = new Thickness(10);
            RootGrid.RowDefinitions.Clear();
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.ColumnDefinitions.Clear();
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1.3, GridUnitType.Star)));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(width < 920 ? 300 : 340)));

            HeaderPanel.SetValue(Grid.RowProperty, 0);
            HeaderPanel.SetValue(Grid.ColumnProperty, 0);
            HeaderPanel.SetValue(Grid.ColumnSpanProperty, 3);
            ProductPanel.SetValue(Grid.RowProperty, 1);
            ProductPanel.SetValue(Grid.ColumnProperty, 0);
            ProductPanel.SetValue(Grid.ColumnSpanProperty, 1);
            CartPanel.SetValue(Grid.RowProperty, 1);
            CartPanel.SetValue(Grid.ColumnProperty, 1);
            CartPanel.SetValue(Grid.ColumnSpanProperty, 1);
            PaymentPanel.SetValue(Grid.RowProperty, 1);
            PaymentPanel.SetValue(Grid.ColumnProperty, 2);
            PaymentPanel.SetValue(Grid.ColumnSpanProperty, 1);

            HeaderGrid.ColumnDefinitions.Clear();
            HeaderGrid.RowDefinitions.Clear();
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            HeaderGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            StatusBar.SetValue(Grid.RowProperty, 0);
            StatusBar.SetValue(Grid.ColumnProperty, 1);
            StatusBar.SetValue(Grid.ColumnSpanProperty, 1);
            HeaderActions.SetValue(Grid.RowProperty, 0);
            HeaderActions.SetValue(Grid.ColumnProperty, 2);

            ProductItemsLayout.Span = width < 920 ? 2 : 3;
            ApplyPanelHeights(width, height, productRatio: 0.60, cartRatio: 0.46);
        }

        private void ApplyTabletLayout()
        {
            RootGrid.Padding = new Thickness(14);
            RootGrid.RowDefinitions.Clear();
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.ColumnDefinitions.Clear();
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1.2, GridUnitType.Star)));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            HeaderPanel.SetValue(Grid.RowProperty, 0);
            HeaderPanel.SetValue(Grid.ColumnProperty, 0);
            HeaderPanel.SetValue(Grid.ColumnSpanProperty, 2);
            ProductPanel.SetValue(Grid.RowProperty, 1);
            ProductPanel.SetValue(Grid.ColumnProperty, 0);
            ProductPanel.SetValue(Grid.ColumnSpanProperty, 1);
            CartPanel.SetValue(Grid.RowProperty, 1);
            CartPanel.SetValue(Grid.ColumnProperty, 1);
            CartPanel.SetValue(Grid.ColumnSpanProperty, 1);
            PaymentPanel.SetValue(Grid.RowProperty, 2);
            PaymentPanel.SetValue(Grid.ColumnProperty, 0);
            PaymentPanel.SetValue(Grid.ColumnSpanProperty, 2);

            HeaderGrid.ColumnDefinitions.Clear();
            HeaderGrid.RowDefinitions.Clear();
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            HeaderGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            HeaderGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            StatusBar.SetValue(Grid.RowProperty, 1);
            StatusBar.SetValue(Grid.ColumnProperty, 0);
            StatusBar.SetValue(Grid.ColumnSpanProperty, 3);
            HeaderActions.SetValue(Grid.RowProperty, 0);
            HeaderActions.SetValue(Grid.ColumnProperty, 2);

            ProductItemsLayout.Span = 2;
            ApplyPanelHeights(width: Width, height: Height, productRatio: 0.58, cartRatio: 0.44);
        }

        private void ApplyPhoneLayout()
        {
            RootGrid.Padding = new Thickness(10);
            RootGrid.RowDefinitions.Clear();
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            RootGrid.ColumnDefinitions.Clear();
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            HeaderPanel.SetValue(Grid.RowProperty, 0);
            HeaderPanel.SetValue(Grid.ColumnProperty, 0);
            HeaderPanel.SetValue(Grid.ColumnSpanProperty, 1);
            ProductPanel.SetValue(Grid.RowProperty, 1);
            ProductPanel.SetValue(Grid.ColumnProperty, 0);
            ProductPanel.SetValue(Grid.ColumnSpanProperty, 1);
            CartPanel.SetValue(Grid.RowProperty, 2);
            CartPanel.SetValue(Grid.ColumnProperty, 0);
            CartPanel.SetValue(Grid.ColumnSpanProperty, 1);
            PaymentPanel.SetValue(Grid.RowProperty, 3);
            PaymentPanel.SetValue(Grid.ColumnProperty, 0);
            PaymentPanel.SetValue(Grid.ColumnSpanProperty, 1);

            HeaderGrid.ColumnDefinitions.Clear();
            HeaderGrid.RowDefinitions.Clear();
            HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            HeaderGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            HeaderGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            HeaderGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            StatusBar.SetValue(Grid.RowProperty, 1);
            StatusBar.SetValue(Grid.ColumnProperty, 0);
            StatusBar.SetValue(Grid.ColumnSpanProperty, 1);
            HeaderActions.SetValue(Grid.RowProperty, 2);
            HeaderActions.SetValue(Grid.ColumnProperty, 0);

            ProductItemsLayout.Span = 1;
            ApplyPhonePanelHeights();
        }

        private void ApplyPanelHeights(double width, double height, double productRatio, double cartRatio)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            var headerAllowance = width < 1100 ? 190 : 110;
            var usableHeight = Math.Max(360, height - headerAllowance);

            ProductList.HeightRequest = Math.Clamp(usableHeight * productRatio, 342, 620);
            CartList.HeightRequest = -1;
        }

        private void ApplyPhonePanelHeights()
        {
            ProductList.HeightRequest = Math.Max(342, Math.Min(Height * 0.56, 520));
            CartList.HeightRequest = -1;
        }
    }
}
