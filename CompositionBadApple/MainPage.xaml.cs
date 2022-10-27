using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CompositionBadApple
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Color[][] frames;
        CompositionColorBrush[] pixelBrushes;
        int frame;

        public MainPage()
        {
            this.InitializeComponent();     
        }

        private async void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///badapple.rgb"));
            var reader = new BinaryReader(await file.OpenStreamForReadAsync());

            frames = new Color[5259][];

            for (int i = 0; i < 5259; i++)
            {
                Color[] pixels = new Color[128*128];

                for (int j = 0; j < 128 * 128; j++)
                {
                    pixels[j].R = reader.ReadByte();
                    pixels[j].G = reader.ReadByte();
                    pixels[j].B = reader.ReadByte();
                    pixels[j].A = 255;
                }

                frames[i] = pixels;
            }

            reader.Dispose();
            file = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Compositor compositor = Window.Current.Compositor;
            ContainerVisual visual = compositor.CreateContainerVisual();
            visual.Size = new System.Numerics.Vector2(128, 128);

            pixelBrushes = new CompositionColorBrush[128 * 128];

            int index = 0;

            for (int i = 0; i < 128; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    SpriteVisual sprite = compositor.CreateSpriteVisual();
                    CompositionColorBrush brush = compositor.CreateColorBrush();

                    sprite.Size = new System.Numerics.Vector2(1, 1);
                    sprite.Offset = new System.Numerics.Vector3((float)j, (float)i, 0);
                    sprite.Brush = brush;

                    visual.Children.InsertAtTop(sprite);
                    pixelBrushes[index] = brush;

                    index++;
                }
            }

            ElementCompositionPreview.SetElementChildVisual(DrawingRect, visual);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(24);
            timer.Tick += (s, o) =>
            {
                var f = frames[frame];

                for (int i = 0; i < 128 * 128; i++)
                {
                    var c = f[i];
                    pixelBrushes[i].Color = c;                      
                }

                frame = frame == 5258 ? 0 : ++frame;
            };

            timer.Start();
        }
    }
}
