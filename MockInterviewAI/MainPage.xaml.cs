﻿using MockInterviewAI.ViewModel;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MockInterviewAI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public InterviewViewModel ViewModel { get; } = new InterviewViewModel();


        public MainPage()
        {
            this.InitializeComponent();
        }

        private void QuestionLimit_Copy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
