using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DeafHearingAID
{
    public partial class MainPage : ContentPage
    {

        TaskCompletionSource<int> stopRecognition;

        SpeechRecognizer speechrecognizer;

        SpeechConfig config;

        bool isinprogress = false;
        public MainPage()
        {
            InitializeComponent();

            stopRecognition = new TaskCompletionSource<int>();

        }

        private async void btnstart_Clicked(object sender, EventArgs eventargs)
        {
            try
            {

                isinprogress = true;

                bool micenabled = await GetMicroPhoneEnabledStatusAsync();

                if (config == null)
                {
                    config = SpeechConfig.FromSubscription("67c6d46689c947299cd10be67e63f6e0", "eastus");
                }

                config.SpeechRecognitionLanguage = GetUserSelectedLang(pickerlang.SelectedItem.ToString());

                string audiotext = string.Empty;

                speechrecognizer = new SpeechRecognizer(config);

                speechrecognizer.Recognized += (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizedKeyword)
                    {
                        audiotext = e.Result.Text;
                    }
                    else if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        audiotext = e.Result.Text;
                    }
                    else if (e.Result.Reason == ResultReason.NoMatch)
                    {
                        audiotext = "can not retrive the text from the speech.";
                    }

                    UpdateUI(audiotext);
                };

                speechrecognizer.Canceled += (s, e) =>
                {
                    var cancellation = CancellationDetails.FromResult(e.Result);

                    audiotext = $"CANCELED: Reason={cancellation.Reason} ErrorDetails={cancellation.ErrorDetails}";

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        UpdateUI(audiotext);
                    }

                    stopRecognition.TrySetResult(0);
                };

                speechrecognizer.SessionStarted += (s, e) =>
                {
                    //Debug.WriteLine("\nSession started event.");
                };

                speechrecognizer.SessionStopped += (s, e) =>
                {

                    stopRecognition.TrySetResult(0);
                };

                await speechrecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                Task.WaitAny(new[] { stopRecognition.Task });

                await speechrecognizer.StopKeywordRecognitionAsync().ConfigureAwait(false);

            }
            catch (Exception ex)
            {
                UpdateUI("Exception: " + ex.ToString());
            }
        }


        private string GetUserSelectedLang(string userselectedlang)
        {
            switch (userselectedlang)
            {
                case "English":
                    return "en-IN";

                case "Gujarati":
                    return "gu-IN";

                case "Hindi":
                    return "hi-IN";

                case "Marathi":
                    return "mr-IN";

                case "Tamil":
                    return "ta-IN";

                case "Telugu":
                    return "te-IN";

                //english as default language

                default:
                    return "en-IN";

            }
        }
        private void UpdateUI(string stringidentified)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                editor.Text += stringidentified;
            });
        }

        private async void btnstop_Clicked(object sender, EventArgs e)
        {
            try
            {
                isinprogress = false;

                if (speechrecognizer != null)
                {
                    await speechrecognizer.StopContinuousRecognitionAsync();
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                isinprogress = false;
            }

        }




        public async Task<bool> GetMicroPhoneEnabledStatusAsync()
        {
            bool micAccessGranted = await DependencyService.Get<IMicrophoneService>().GetPermissionsAsync();
            if (!micAccessGranted)
            {
                UpdateUI("Please Provide access to Microphone");
            }

            return micAccessGranted;
        }

        private async void pickerlang_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(editor.Text))
            {
                bool response = await DisplayAlert("Are you sure to change Langugae?", "Changing Language will clear data from recognised text", "Ok", "Cancel", FlowDirection.MatchParent);

                if (response)
                {
                    editor.Text = string.Empty;
                }
            }

        }
    }
}
