using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace vr_test
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        // Grammer File
        private const string SRGS_FILE = "input11.xml";
        // Speech Recognizer
        private SpeechRecognizer recognizer;
        private SpeechSynthesizer synth;
        private MediaElement myMedia;
        private SpeechSynthesisStream sStream;

        public MainPage()
        {
            this.InitializeComponent();

            Unloaded += MainPage_Unloaded;

            initializeSpeechRecognizer();
            initSynthsizer();

        }


        private async void MainPage_Unloaded(object sender, object args)
        {
            // Stop recognizing
            await recognizer.ContinuousRecognitionSession.StopAsync();

            // Release pins
            recognizer.Dispose();
            synth.Dispose();

            recognizer = null;
            synth = null;
        }

        private async void initializeSpeechRecognizer()
        {
            try
            {
                // Initialize recognizer

                recognizer = new SpeechRecognizer();

                recognizer.StateChanged += RecognizerStateChanged;
                recognizer.ContinuousRecognitionSession.ResultGenerated += RecognizerResultGenerated;

                Debug.WriteLine("Result: 1. Event Handler Created.");
                
                // Load Grammer file constraint
                string fileName = String.Format(SRGS_FILE);
                StorageFile grammarContentFile = await Package.Current.InstalledLocation.GetFileAsync(fileName);
                Debug.WriteLine("Result: 2. Storage file Created "+ grammarContentFile.ToString());

                SpeechRecognitionGrammarFileConstraint grammarConstraint = new SpeechRecognitionGrammarFileConstraint(grammarContentFile);
                Debug.WriteLine("Result: 3. Grammar File Contraint " + grammarConstraint.ToString());

                // Add to grammer constraint
                recognizer.Constraints.Add(grammarConstraint);

                // Compile grammer
                SpeechRecognitionCompilationResult compilationResult = await recognizer.CompileConstraintsAsync();

                Debug.WriteLine("Status: 4. compilation Status " + compilationResult.Status.ToString());
                // If successful, display the recognition result.
                if (compilationResult.Status == SpeechRecognitionResultStatus.Success)
                {
                    await recognizer.ContinuousRecognitionSession.StartAsync();
                    Debug.WriteLine("Result: 5. compilation Result Success " + compilationResult.ToString());

                   
                }
                else
                {
                    Debug.WriteLine("Status: 6. Compilation Result not success. " + compilationResult.Status);
                }

                // Send the stream to the media object.
                //mediaElement.SetSource(stream, stream.ContentType);
                //mediaElement.Play();

            }
            catch (Exception e)
            {
                Debug.WriteLine("Recognition Error: " + e.ToString());
            }

        }
        // Recognizer generated results

        private async void initSynthsizer()
        {
            try
            {
                synth = new SpeechSynthesizer();

                // Generate the audio stream from plain text.
                sStream = await synth.SynthesizeTextToStreamAsync("");

                var mediaElement = new MediaElement();
                mediaElement.SetSource(sStream, sStream.ContentType);
                mediaElement.Play();

                sStream.Seek(0);
            }
            catch (Exception e)
            {
                Debug.WriteLine("SpeechSynthesizer Error: " + e.ToString());
            }
        }
        private void RecognizerResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            // Output debug strings
            Debug.WriteLine("Recognition:  " + args.Result.Status);
            Debug.WriteLine("Recognition:  " + args.Result.Text);

            int count = args.Result.SemanticInterpretation.Properties.Count;

            Debug.WriteLine("Count: " + count);
            Debug.WriteLine("Tag: " + args.Result.Constraint.Tag);
                   

    }

        // Recognizer state changed
        private void RecognizerStateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            Debug.WriteLine("Speech recognizer state: " + args.State.ToString());
        }

        private async void tts(String a)
        {
            try
            {
                // Generate the audio stream from plain text.
                sStream = await synth.SynthesizeTextToStreamAsync(a);

                var mediaElement = new MediaElement();
                mediaElement.SetSource(sStream, sStream.ContentType);
                mediaElement.Play();


                sStream.Seek(0);



                // Send the stream to the media object.
                //mediaElement.SetSource(stream, stream.ContentType);
                //mediaElement.Play();

            }
            catch (Exception e)
            {
                Debug.WriteLine("Error: " + e.ToString());
            }
        }

        private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            tts("Click first Button and waiting for your command");
        }

        private void Button_Click_1(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            tts("Test for foreign language");
        }

        private void Button_Click_2(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            tts("click third button");
        }
    }
}
