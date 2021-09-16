using Newtonsoft.Json;
using System.IO;

namespace VolunteerComputing.Client
{
    class Storage
    {
        static string userDataPath = "userData.json";
        static UserData userData;
        public static int Id
        {
            get
            {
                InitUserData();
                return userData.Id;
            }
            set
            {
                InitUserData();
                if (value != userData?.Id)
                {
                    userData.Id = value;
                    SaveUserData();
                }
            }
        }

        public static bool HasSentInitMeasurements
        {
            get
            {
                InitUserData();
                return userData.HasSentInitMeasurements;
            }
            set
            {
                InitUserData();
                if (value != userData.HasSentInitMeasurements)
                {
                    userData.HasSentInitMeasurements = value;
                    SaveUserData();
                }
            }
        }

        public static string CpuEnergyToolPath
        {
            get
            {
                InitUserData();
                return userData.CpuEnergyToolPath;
            }
            set
            {
                InitUserData();
                if (value != userData.CpuEnergyToolPath)
                {
                    userData.CpuEnergyToolPath = value;
                    SaveUserData();
                }
            }
        }
        public static string GpuEnergyToolPath
        {
            get
            {
                InitUserData();
                return userData.GpuEnergyToolPath;
            }
            set
            {
                InitUserData();
                if (value != userData.GpuEnergyToolPath)
                {
                    userData.GpuEnergyToolPath = value;
                    SaveUserData();
                }
            }
        }

        static void InitUserData()
        {
            if (File.Exists(userDataPath))
            {
                if (userData is null)
                {
                    userData = JsonConvert.DeserializeObject<UserData>(File.ReadAllText(userDataPath));
                }
            }
            else
                userData ??= new UserData();
        }

        static void SaveUserData()
        {
            File.WriteAllText(userDataPath, JsonConvert.SerializeObject(userData));
        }

        public static void Restart()
        {
            File.Delete(userDataPath);
        }

        class UserData
        {
            public int Id { get; set; }
            public bool HasSentInitMeasurements { get; set; }
            public string CpuEnergyToolPath { get; set; }
            public string GpuEnergyToolPath { get; set; }
        }
    }
}
