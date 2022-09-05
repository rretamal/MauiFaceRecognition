using Java.IO;
using Java.Nio.Channels;
using Java.Nio;
using Android.Graphics;
using Android.OS;
using Java.Lang;
using static Java.Util.Jar.Attributes;
using Android.Text.Style;
using Android.Util;
using Microsoft.Maui;
using static Android.Provider.MediaStore;

namespace Maui.FaceRecognition.Core;

// All the code in this file is only included on Android.
public class FaceClassifier
{
    int OUTPUT_SIZE = 192;
    private MappedByteBuffer model;
    private Org.Tensorflow.Lite.Interpreter interpreter;
    private int floatSize = 4;
    private int pixelSize = 3;
    private int imageMean = 128;
    private float imageStd = 128.0f;

    public FaceClassifier()
    {
        model = LoadModel();
        interpreter = new Org.Tensorflow.Lite.Interpreter(model);
    }

    private MappedByteBuffer LoadModel()
    {
        var assets = Android.App.Application.Context.Assets;

        var assetDescriptor = assets.OpenFd("mobile_face_net.tflite");
        var inputStream = new FileInputStream(assetDescriptor.FileDescriptor);

        var mappedByteBuffer = inputStream.Channel.Map(FileChannel.MapMode.ReadOnly, assetDescriptor.StartOffset, assetDescriptor.DeclaredLength);
        return mappedByteBuffer;
    }

    public void IdentifyFace(byte[] image)
    {

    }

    public void ExtractEmbeddings(byte[] image)
    {
        var tensor = interpreter.GetInputTensor(0);
        var shape = tensor.Shape();

        var width = shape[1];
        var height = shape[2];

        var byteBuffer = GetPhotoAsByteBuffer(image, width, height);
        Java.Lang.Object[] input = { byteBuffer };

        IDictionary<Integer, Java.Lang.Object> outputs = new Dictionary<Integer, Java.Lang.Object>();

        var outputLocations = new float[1][] { new float[OUTPUT_SIZE] };
        outputs.Add((Integer)0, Java.Lang.Object.FromArray(outputLocations));

        interpreter.RunForMultipleInputsOutputs(input, outputs); //Run model


    }

    private ByteBuffer GetPhotoAsByteBuffer(byte[] image, int width, int height)
    {
        var bitmap = BitmapFactory.DecodeByteArray(image, 0, image.Length);
        var resizedBitmap = Bitmap.CreateScaledBitmap(bitmap, width, height, true);

        var modelInputSize = floatSize * height * width * pixelSize;
        var byteBuffer = ByteBuffer.AllocateDirect(modelInputSize);
        byteBuffer.Order(ByteOrder.NativeOrder());

        var pixels = new int[width * height];
        resizedBitmap.GetPixels(pixels, 0, resizedBitmap.Width, 0, 0, resizedBitmap.Width, resizedBitmap.Height);

        var pixel = 0;

        MemoryStream byteArrayOutputStream = new MemoryStream();
        resizedBitmap.Compress(Bitmap.CompressFormat.Png, 100, byteArrayOutputStream);
        byte[] byteArray = byteArrayOutputStream.ToArray();
        string encoded = Base64.EncodeToString(byteArray, Base64Flags.Default);

        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var pixelVal = pixels[pixel++];

                byteBuffer.PutFloat((((pixelVal >> 16) & 0xFF) - imageMean) / imageStd);
                byteBuffer.PutFloat((((pixelVal >> 8) & 0xFF) - imageMean) / imageStd);
                byteBuffer.PutFloat((((pixelVal) & 0xFF) - imageMean) / imageStd);
            }
        }

        bitmap.Recycle();

        return byteBuffer;
    }
}  


