using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xamarin.Forms;
using FreshMvvm;
using SLB.iTrackr.PageModels;
using SLB.iTrackr.Configs;
using System.ComponentModel;

namespace SLB.iTrackr.Navigation
{
    public class CustomNavigation : MasterDetailPage, IFreshNavigationService
    {
        FreshNavigationContainer _homeNav, _archiveNav, _reportNav, _settingNav, _aboutNav;

        public CustomNavigation()
        {
            NavigationServiceName = "CustomNavigation";
            SetupAllPagesNav();
            CreateMasterPage("Menu");
            RegisterNavigation();
        }

        public string NavigationServiceName { get; private set; }

        protected void RegisterNavigation()
        {
            FreshIOC.Container.Register<IFreshNavigationService>(this, NavigationServiceName);
        }

        protected void SetupAllPagesNav()
        {
            var homePage = FreshPageModelResolver.ResolvePageModel<HomePageModel>();
            var archivePage = FreshPageModelResolver.ResolvePageModel<ArchivePageModel>();
            var reportPage = FreshPageModelResolver.ResolvePageModel<ReportPageModel>();
            var settingPage = FreshPageModelResolver.ResolvePageModel<SettingPageModel>();
            var aboutPage = FreshPageModelResolver.ResolvePageModel<AboutPageModel>();

            _homeNav = new FreshNavigationContainer(homePage);
            _archiveNav = new FreshNavigationContainer(archivePage);
            _reportNav = new FreshNavigationContainer(reportPage);
            _settingNav = new FreshNavigationContainer(settingPage);
            _aboutNav = new FreshNavigationContainer(aboutPage);
        }

        protected void CreateMasterPage(string title)
        {
            var masterPage = new ContentPage();
            masterPage.Title = title;

            if (Device.OS == TargetPlatform.iOS)
                masterPage.Padding = new Thickness(0, 30, 0, 0);

            var menuListViewItems = new List<MenuItem>()
            {
                new MenuItem("Home", ImageSource.FromResource("SLB.iTrackr.Resources.Menu_Home.png")),
                new MenuItem("Archive", ImageSource.FromResource("SLB.iTrackr.Resources.Menu_Archive.png")),
                new MenuItem("Report", ImageSource.FromResource("SLB.iTrackr.Resources.Menu_Report.png")),
                new MenuItem("Setting", ImageSource.FromResource("SLB.iTrackr.Resources.Menu_Setting.png")),
                new MenuItem("About", ImageSource.FromResource("SLB.iTrackr.Resources.Menu_About.png"))
            };

            #region __Master Page Layout__
            
            var menuListView = new ListView
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                SeparatorVisibility = SeparatorVisibility.None,
                RowHeight = 60,
                ItemsSource = menuListViewItems,
                ItemTemplate = new DataTemplate(() => 
                {
                    var menuIcon = new Image {Aspect = Aspect.AspectFit, VerticalOptions = LayoutOptions.Center};
                    menuIcon.SetBinding(Image.SourceProperty, "IconSource");

                    var rightArrowIcon = new Image 
                    { 
                        Aspect = Aspect.AspectFit, 
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.EndAndExpand,
                        Source = ImageSource.FromResource("SLB.iTrackr.Resources.Right_Arrow.png")
                    };                    

                    var menuLabel = new Label
                    {
                        FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                        FontAttributes = FontAttributes.Bold,
                        VerticalOptions = LayoutOptions.Center
                    };
                    menuLabel.SetBinding(Label.TextProperty, "Title");
                    menuLabel.SetBinding(Label.TextColorProperty, "TextColor");
                    
                    var cellStack = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Padding = new Thickness(20, 10, 0, 10),
                        Spacing = 20,
                        Children = {menuIcon, menuLabel, rightArrowIcon}
                    };
                    
                    return new ViewCell { View = cellStack };
                })
            };

            menuListView.ItemSelected += (async (sender, args) => 
            {                
                //Reset Selection Color
                foreach( var menuItem in menuListViewItems)
                {
                    menuItem.TextColor = Color.FromHex(ColorScheme.PrimaryColor);
                }

                MenuItem selected = (MenuItem)args.SelectedItem;
                selected.TextColor = Color.FromHex(ColorScheme.AccentColor);

                switch((string) selected.Title)
                {
                    case "Home":
                        this.Detail = _homeNav;                        
                        break;
                    case "Archive":
                        this.Detail = _archiveNav;
                        break;
                    case "Report":
                        this.Detail = _reportNav;
                        break;
                    case "Setting":
                        this.Detail = _settingNav;
                        break;
                    case "About":
                        menuListView.SelectedItem = menuListViewItems.Find(Menu => Menu.Title == "Home");
                        var aboutPage = FreshPageModelResolver.ResolvePageModel<AboutPageModel>();
                        await PushPage(aboutPage, null, true);
                        break;
                }

                IsPresented = false;
            });

            menuListView.SelectedItem = menuListViewItems.Find(Menu => Menu.Title == "Home");

            var masterPageHeader = new Image
            {
                HeightRequest = 150,
                Aspect = Aspect.AspectFit,
                Source = ImageSource.FromResource("SLB.iTrackr.Resources.iTrackr_Logo.png"),
                BackgroundColor = Color.FromHex(ColorScheme.PrimaryColor)
            };

            var masterPageFooter = new Image
            {
                HeightRequest = 50,
                Aspect = Aspect.AspectFit,
                VerticalOptions = LayoutOptions.EndAndExpand,
                Source = ImageSource.FromResource("SLB.iTrackr.Resources.Menu_NewTicket.png"),
                BackgroundColor = Color.FromHex(ColorScheme.PrimaryColor)
            };

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += async (s, e) =>
            {
                menuListView.SelectedItem = menuListViewItems.Find(Menu => Menu.Title == "Home");
                var newTicketPage = FreshPageModelResolver.ResolvePageModel<NewTicketPageModel>();
                await PushPage(newTicketPage, null);
                IsPresented = false;               
            };

            masterPageFooter.GestureRecognizers.Add(tapGestureRecognizer);

            var mainStack = new StackLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    masterPageHeader,
                    menuListView,
                    masterPageFooter
                }
            };            

            #endregion

            masterPage.Content = mainStack;       

            Master = masterPage;
        }

        public void NotifyChildrenPageWasPopped()
        {
            //throw new NotImplementedException();
        }

        public virtual async Task PopPage(bool modal = false, bool animate = true)
        {
            if (modal)
                await Navigation.PopModalAsync();
            else
                await ((NavigationPage)this.Detail).PopAsync();
        }

        public virtual async Task PopToRoot(bool animate = true)
        {
            await Navigation.PopToRootAsync(animate);
        }

        public virtual async Task PushPage(Page page, FreshBasePageModel model, bool modal = false, bool animate = true)
        {
            if (modal)
                await Navigation.PushModalAsync(page, animate);
            else
                await ((NavigationPage)this.Detail).PushAsync(page, animate);            
        }

        private class MenuItem : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public string Title { get; set; }            
            public ImageSource IconSource { get; private set; }

            private Color _textColor;
            public Color TextColor 
            {
                get { return _textColor; }
                set 
                {
                    _textColor = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("TextColor"));
                }
            }

            public MenuItem(string title, ImageSource iconSource)
            {
                Title = title;
                IconSource = iconSource;
                TextColor = Color.FromHex(ColorScheme.PrimaryColor);
            }
            
        }
    }
}
