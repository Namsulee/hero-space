using CAMEARA_BASED_DB.ExtraDevice;      // User defined class

namespace CAMEARA_BASED_DB.ExtraDevice
{
    class UserProfileObject
    {
        /// <summary>
        /// An initialized Visitor object
        /// </summary>
        public Visitor Visitor { get; set; }

        /// <summary>
        /// An initialized WebcamHelper 
        /// </summary>
        public WebCam webcam { get; set; }

        /// <summary>
        /// Initializes a new UserProfileObject with relevant information
        /// </summary>
        public UserProfileObject(Visitor visitor, WebCam webcamHelper)
        {
            Visitor = visitor;
            webcam = webcamHelper;
        }
    }
}
