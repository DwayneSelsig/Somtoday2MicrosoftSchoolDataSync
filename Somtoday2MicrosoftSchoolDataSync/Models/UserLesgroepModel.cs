using Somtoday2MicrosoftSchoolDataSync.UmService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Somtoday2MicrosoftSchoolDataSync.Models
{
    class UserLesgroepModel
    {
        public VestigingLesgroepModel VestigingLesgroep { get; set; }
        public List<webserviceUmObject> Users { get; set; }
    }
}
