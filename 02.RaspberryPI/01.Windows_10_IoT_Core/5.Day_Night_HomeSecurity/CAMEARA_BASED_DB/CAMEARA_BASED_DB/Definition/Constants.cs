namespace CAMEARA_BASED_DB.Definition
{
    public static class OpenAPIConstants
    {
        // This variable should be set to false for devices, unlike the Raspberry Pi, that have GPU support
        public const bool DisableLiveCameraFeed = false;
        // You can obtain a subscription key for Face API by following the instructions here: https://www.microsoft.com/cognitive-services/en-us/sign-up
        public const string OxfordFaceAPIKey = "<own-Key>";

        // Name of the folder in which all Whitelist data is stored
        public const string WhiteListFolderName = "UserList";

        public const double precision = 0.70;

        public const int MaxUserCnt = 1;
    }

    public static class GpioConstants
    {
        public const int PirSensorPin = 13; // 센서 입력용 GPIO 핀
        //public const int CheckLedPin = 12; // 동작 확인용 ACTLED    
        public const int LED_PIN_17 = 17;
        public const int LED_PIN_27 = 27;

        public const int LED_PIN_22 = 22;
        public const int LED_PIN_10 = 10;

        public const int LED_PIN_9 = 9;
        public const int LED_PIN_11 = 11;

        public const int LED_PIN_5 = 5;
        public const int LED_PIN_6 = 6;
        
    }
}
