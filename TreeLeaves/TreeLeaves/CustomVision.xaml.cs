using Newtonsoft.Json;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TreeLeaves.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TreeLeaves
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomVision : ContentPage
    {
        private string currentType;
        private double currentProbability;

        public CustomVision()
        {
            InitializeComponent();
        }

        private async void loadCamera(object sender, EventArgs args)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", "No camera available.", "OK");
                return;
            }

            MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                PhotoSize = PhotoSize.Medium,
                Directory = "Sample",
                Name = $"{DateTime.UtcNow}.jpg"
            });
            
            if (file == null)
                return;

            image.Source = ImageSource.FromStream(() =>
            {
                return file.GetStream();
            });

            TagLabel.Text = "Analysing...";
            PredictionLabel.Text = "";

            await MakePredictionRequest(file);

            await postResultsAsync();
        }

        static byte[] GetImageAsByteArray(MediaFile file)
        {
            var stream = file.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            return binaryReader.ReadBytes((int)stream.Length);
        }

        async Task MakePredictionRequest(MediaFile file)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Prediction-Key", "6a42d857aa574219aa5a2c6ef6e32282");

            string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/5f73d24b-1153-490c-a7f8-da544c4fd659/image";

            HttpResponseMessage response;

            byte[] byteData = GetImageAsByteArray(file);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    EvaluationModel responseModel = JsonConvert.DeserializeObject<EvaluationModel>(responseString);
                    
                    string tag = "No data on this type of leaf";
                    double max = 0.0;
                    foreach (Prediction prediction in responseModel.Predictions)
                    {
                        if (prediction.Probability > max && prediction.Probability >= 0.35)
                        {
                            this.currentType = prediction.Tag;
                            this.currentProbability = prediction.Probability;

                            max = prediction.Probability;
                            tag = prediction.Tag;
                        }
                    }

                    TagLabel.Text = "Type: " + tag;

                    if (tag != "No data on this type of leaf")
                    {
                        this.currentProbability = max;
                        PredictionLabel.Text = "Probability: " + max;
                    }
                }

                file.Dispose();
            }
        }

        async Task postResultsAsync()
        {
            TreeLeavesModel model = new TreeLeavesModel
            {
                Type = this.currentType,
                Probability = this.currentProbability
            };

            await AzureManager.AzureManagerInstance.PostTreeLeavesInformation(model);
        }
    }
}