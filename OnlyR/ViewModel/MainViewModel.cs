using System;
using System.Windows;
using System.ComponentModel;
using GalaSoft.MvvmLight;
using OnlyR.Model;
using OnlyR.Services.Audio;
using OnlyR.Services.Options;
using OnlyR.Services.RecordingDestination;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Messaging;
using OnlyR.Pages;
using OnlyR.ViewModel.Messages;

namespace OnlyR.ViewModel
{
    /// <summary>
    /// Main View model. The main view is just a container for pages
    /// <para>
    /// See http://www.galasoft.ch/mvvm for details of the mvvm light framework
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly Dictionary<string, FrameworkElement> _pages;
        private string _currentPageName;
        private readonly IOptionsService _optionsService;
        private readonly IAudioService _audioService;

        public MainViewModel(
           IAudioService audioService,
           IOptionsService optionsService,
           IRecordingDestinationService destService)
        {
            // subscriptions...
            Messenger.Default.Register<NavigateMessage>(this, OnNavigate);
            Messenger.Default.Register<AlwaysOnTopChanged>(this, OnAlwaysOnTopChanged);

            _pages = new Dictionary<string, FrameworkElement>();

            _optionsService = optionsService;
            _audioService = audioService;
            
            // set up pages...
            SetupPage(RecordingPageViewModel.PageName, new RecordingPage(),
                new RecordingPageViewModel(audioService, optionsService, destService));

            SetupPage(SettingsPageViewModel.PageName, new SettingsPage(),
                new SettingsPageViewModel(audioService, optionsService));

            var state = new RecordingPageNavigationState
            {
                ShowSplash = true,
                StartRecording = optionsService.Options.StartRecordingOnLaunch
            };

            Messenger.Default.Send(new NavigateMessage(RecordingPageViewModel.PageName, state));
        }

        private void OnAlwaysOnTopChanged(AlwaysOnTopChanged obj)
        {
            RaisePropertyChanged(nameof(AlwaysOnTop));
        }

        public bool AlwaysOnTop => _optionsService.Options.AlwaysOnTop;

        private void SetupPage(string pageName, FrameworkElement page, ViewModelBase pageModel)
        {
            page.DataContext = pageModel;
            _pages.Add(pageName, page);
        }

        private void OnNavigate(NavigateMessage message)
        {
            CurrentPage = _pages[message.TargetPage];
            _currentPageName = message.TargetPage;
            ((IPage)CurrentPage.DataContext).Activated(message.State);
        }

        private FrameworkElement _currentPage;
        public FrameworkElement CurrentPage
        {
            get => _currentPage;
            set
            {
                if (!ReferenceEquals(_currentPage, value))
                {
                    _currentPage = value;
                    RaisePropertyChanged(nameof(CurrentPage));
                }
            }
        }

        public void Closing(object sender, CancelEventArgs e)
        {
            var recordingPageModel = (RecordingPageViewModel)_pages[RecordingPageViewModel.PageName].DataContext;

            if (_optionsService.Options.AllowCloseWhenRecording)
            {
                if (recordingPageModel.IsRecordingOrStopping)
                {
                    e.Cancel = true;
                    _audioService.StoppedEvent += RecordingStoppedDuringAppClose;
                    _audioService.StopRecording(_optionsService.Options.FadeOut);
                }
            }
            else
            {
                // prevent window closing when recording...
                recordingPageModel.Closing(sender, e);

                if (!e.Cancel)
                {
                    Messenger.Default.Send(new BeforeShutDownMessage(_currentPageName));
                    (_audioService as IDisposable)?.Dispose();
                }
            }
        }

        private void RecordingStoppedDuringAppClose(object sender, EventArgs e)
        {
            Messenger.Default.Send(new ShutDownApplicationMessage());
        }
    }
}