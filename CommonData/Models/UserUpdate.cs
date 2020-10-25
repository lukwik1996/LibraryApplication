using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace CommonData.Models
{
    [Serializable]
    public class UserUpdate
    {
        public string OldPassword { get; set; }

        public string OldPasswordEmail { get; set; }

        public string NewPassword { get; set; }
        
        public string RepeatNewPassword { get; set; }
        
        public string OldEmail { get; set; }
        
        public string NewEmail { get; set; }
    }
}
