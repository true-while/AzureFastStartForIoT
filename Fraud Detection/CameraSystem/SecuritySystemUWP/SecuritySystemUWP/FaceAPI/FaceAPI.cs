using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Windows.Storage;
using System.Diagnostics;
using System.IO;
using Windows.Storage.Streams;

namespace SecuritySystemUWP
{
    public class FaceClient
    {
        private FaceServiceClient faceServiceClient;
        public bool KnownImagesRegistered { get; private set; }

        // An Id and a name for a group of people.
        const string personGroupId = "authorizedusers";
        const string personGroupName = "Authorized Users";

        public FaceClient(string key)
        {
            //faceServiceClient = new FaceServiceClient(key);
        }

        public async Task RegisterKnownUsersAsync()
        {
            try
            {
                #region Create new Person Group
                //try
                //{
                //    // Delete the person group if it already exists.
                //    PersonGroup group = await faceServiceClient.GetPersonGroupAsync(personGroupId);
                //    Debug.WriteLine("Person Group " + personGroupId + " already exists, deleting it.");
                //    await faceServiceClient.DeletePersonGroupAsync(personGroupId);
                //}
                //catch (Exception)
                //{
                //    // Hmmm, that exception tasted yummy.
                //}

                //// create an empty person group with an Id and a friendly display name.
                //await faceServiceClient.CreatePersonGroupAsync(personGroupId, personGroupName);
                #endregion

                #region Upload images for each person
                ////StorageFolder knownImagesBaseFolder = await KnownFolders.CameraRoll.GetFolderAsync("knownimages");
                //StorageFolder knownImagesBaseFolder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync(@"Assets\knownimages");
                //var knownImageFolders = await knownImagesBaseFolder.GetFoldersAsync();
                //foreach (StorageFolder person in knownImageFolders)
                //{
                //    Debug.WriteLine(person.Name);
                //    // define a person in the group
                //    CreatePersonResult friend = await faceServiceClient.CreatePersonAsync(personGroupId, person.Name);
                //    IReadOnlyList<StorageFile> files = await person.GetFilesAsync();

                //    // add the known images of the person
                //    foreach (StorageFile file in files)
                //    {
                //        using (Stream s = File.OpenRead(file.Path))
                //        {
                //            Debug.WriteLine("Uploading " + file.Path);
                //            // detect faces in the image and add to person
                //            await faceServiceClient.AddPersonFaceAsync(personGroupId, friend.PersonId, s);
                //        }
                //    }
                //}
                #endregion

                #region Train the model.
                //await faceServiceClient.TrainPersonGroupAsync(personGroupId);

                //// wait until the training is complete.
                //TrainingStatus trainingstatus = null;
                //while (true)
                //{
                //    trainingstatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId);

                //    if (trainingstatus.Status == Status.Succeeded)
                //    {
                //        Debug.WriteLine("Training complete.");
                //        break;
                //    }
                //    Debug.WriteLine("Wating for training to complete....");
                //    await Task.Delay(10000);
                //}

                #endregion
            }
            catch (Exception ex)
            {
                #region Track and display errors
                FaceAPIException fex = (FaceAPIException)ex.InnerException;
                Debug.WriteLine(ex.InnerException.Message);
                Debug.WriteLine(fex.ErrorCode);
                Debug.WriteLine(fex.ErrorMessage);
                #endregion
            }

            KnownImagesRegistered = true;

            //App.Controller.Camera.PhotoTaken += Camera_PhotoTaken;
        }

        private void Camera_PhotoTaken(object sender, PhotoTakenEventArgs e)
        {
            Task.Run(async () => {

                string logmessage = String.Empty;

                //StorageFolder folder = await KnownFolders.PicturesLibrary.GetFolderAsync("securitysystem-cameradrop");
                StorageFolder folder = KnownFolders.PicturesLibrary;
                StorageFile securityLogFile = await folder.CreateFileAsync(@"securityaccess.log", CreationCollisionOption.OpenIfExists);

                Debug.WriteLine("Sending photo to Face API for analysis....");
                Person person = await TestUserAsync(e.Path);
                if (person == null)
                {
                    logmessage = "An unsuccessful access attempt was made at " + DateTime.Now + Environment.NewLine;
                }
                else
                {
                    logmessage = person.Name + " was identified at " + DateTime.Now + Environment.NewLine;
                }

                Debug.WriteLine(logmessage);
                await FileIO.AppendTextAsync(securityLogFile, logmessage);

            }).Wait();
        }

        public async Task<Person> TestUserAsync(string testImageFile)
        {
            Person person = null;
           
            try
            {
                using (Stream s = File.OpenRead(testImageFile))
                {
                    #region Send image for face detection
                    // DetectAsync is used to locate any faces in the image. It returns the coordinates of the bounding box for each face.
                    var faces = await faceServiceClient.DetectAsync(s);
                    if (faces.Length == 0) throw new Exception("No face was detected in the photograph.");
                    var faceIds = faces.Select(face => face.FaceId).ToArray();
                    #endregion
                    #region Perform the identification
                    // IdentifyAsync will identify each face in each bounding box.
                    // Returns a list of possible matches (candidates) and a confidence value.
                    var results = await faceServiceClient.IdentifyAsync(personGroupId, faceIds);
                    #endregion
                    #region Process the results
                    foreach (var identifyResult in results)
                    {
                        Debug.WriteLine("Result of face: " + identifyResult.FaceId);
                        if (identifyResult.Candidates.Length == 0)
                        {
                            Debug.WriteLine("No one identified");
                        }
                        else
                        {
                            if (identifyResult.Candidates[0].Confidence > 0.7)
                            {
                                // Get the details of the top candidate.
                                var candidateId = identifyResult.Candidates[0].PersonId;
                                person = await faceServiceClient.GetPersonAsync(personGroupId, candidateId);
                                Debug.WriteLine("Identified as " + person.Name + " - " + (identifyResult.Candidates[0].Confidence) * 100 + "% confident");
                            }
                            else
                            {
                                Debug.WriteLine("A candidate was found but the confidence was not high enough to be considered a true match. Confidence value = " + identifyResult.Candidates[0].Confidence.ToString());
                            }
                        }
                    }
                    #endregion
                }
            }
            catch (FaceAPIException fex)
            {
                #region Print out error message to Output window.
                Debug.WriteLine(fex.Message);
                Debug.WriteLine(fex.ErrorCode);
                Debug.WriteLine(fex.ErrorMessage);
                Debug.WriteLine(fex.InnerException.Message);
                throw fex;
                #endregion
            }

            return person;

        }
    }
}
