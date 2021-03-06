﻿using CprPrototype.Database;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CprPrototype.ViewModel;

namespace CprPrototype.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LogDetailPage : ContentPage
    {
        #region Properties

        private static DatabaseHelper _databaseHelper = DatabaseHelper.Instance;
        private BaseViewModel _viewModel = BaseViewModel.Instance;

        #endregion

        #region Contructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LogDetailPage"/> Content Page
        /// </summary>
        /// <param name="incomingId">ID received when a list item is pushed and a new page is created.</param>
        public LogDetailPage(int incomingId)
        {
            InitializeComponent();
            switch(Device.RuntimePlatform)
            {
                case Device.iOS:
                    NavigationPage.SetHasNavigationBar(this, true);
                    break;
                case Device.Android:
                    NavigationPage.SetHasNavigationBar(this, false);
                    break;
            }

            BindingContext = _viewModel;
            DataTemplate template = new DataTemplate(typeof(ImageCell));
            template.SetBinding(ImageCell.ImageSourceProperty, "ImageSource");
            template.SetBinding(ImageCell.TextProperty, "Name");
            template.SetBinding(ImageCell.DetailProperty, "Date");
            template.SetValue(ImageCell.TextColorProperty, Color.Black);
            logDetailList.ItemTemplate = template;
            logDetailList.BindingContext = _viewModel;

            GetHistoryEntries(incomingId);
            
        }

        #endregion

        #region Methods

        private async void GetHistoryEntries(int id)
        {
            logDetailList.ItemsSource = await _databaseHelper.GetEntriesConnectedToCPRHistoryAsync(id);
        }

        #endregion
    }
}