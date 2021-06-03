using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolunteerComputing.Client
{
    class Storage
    {
        static string userDataPath = "userData.txt";
        static UserData userData;
        public static int? Id
        {
            get
            {
                InitUserData();
                return userData?.Id;
            }
            set
            {
                if(value.HasValue && value.Value != userData?.Id)
                {
                    userData.Id = value.Value;
                    SaveUserData();
                }
            }
        }

        static void InitUserData()
        {
            if (userData is null && File.Exists(userDataPath))
            {
                userData = JsonConvert.DeserializeObject<UserData>(File.ReadAllText(userDataPath));
            }
        }

        static void SaveUserData()
        {
            File.WriteAllText(userDataPath, JsonConvert.SerializeObject(userData));
        }

        class UserData
        {
            public int Id { get; set; }
        }
    }
}
