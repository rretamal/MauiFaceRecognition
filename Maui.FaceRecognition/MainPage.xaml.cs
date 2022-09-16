using Maui.FaceRecognition.Core;

namespace Maui.FaceRecognition;

public partial class MainPage : ContentPage
{
	int count = 0;
    FaceClassifier classifier;
    byte[] frame;

	public MainPage()
	{
		InitializeComponent();

        classifier = new FaceClassifier();

        btnTakePicture.Clicked += BtnTakePicture_Clicked;
        cameraView.FrameReady += CameraView_FrameReady;
	}

    private void CameraView_FrameReady(object sender, ZXing.Net.Maui.CameraFrameBufferEventArgs e)
    {
        var bytes = new Byte[e.Data.Data.Remaining()];
        e.Data.Data.Get(bytes, 0, bytes.Length);

        frame = bytes;
    }

    private void BtnTakePicture_Clicked(object sender, EventArgs e)
    {
        //classifier.ExtractEmbeddings(frame);
    }
}


