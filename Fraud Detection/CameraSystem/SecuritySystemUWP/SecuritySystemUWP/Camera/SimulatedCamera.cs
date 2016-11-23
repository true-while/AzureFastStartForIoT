using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SecuritySystemUWP
{
    class SimulatedCamera : ICamera
    {
        StorageFolder sourceFolder;
        StorageFile sourceFile;
        StorageFolder destFolder;
        StorageFile destFile;

        private bool isEnabled;

        public event EventHandler<PhotoTakenEventArgs> PhotoTaken;

        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }

            set
            {
                isEnabled = value;
            }
        }

        public void Dispose()
        {
        }

        public async Task Initialize()
        {
            // Copying the sample photo to the pictures library.
            sourceFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            sourceFile = await sourceFolder.GetFileAsync(@"Assets\ident1.jpg");
            destFolder = await KnownFolders.PicturesLibrary.GetFolderAsync("securitysystem-cameradrop");
            PhotoTaken = delegate { };
        }

        public async Task TriggerCapture()
        {
            destFile = await destFolder.CreateFileAsync(@"ident1.jpg", CreationCollisionOption.ReplaceExisting);
            await sourceFile.CopyAndReplaceAsync(destFile);
            PhotoTaken(this, new PhotoTakenEventArgs() { Path = destFile.Path });
        }
    }
}



