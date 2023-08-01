using System.Collections.Generic;

namespace KernNetzServer.Modules.Lobby
{
    public class AppSecretKey
    {
        public List<string> AppKeys;

        public bool ContainsValue(string value) 
        {
            return AppKeys.Contains(value);
        }
    }
   
}
