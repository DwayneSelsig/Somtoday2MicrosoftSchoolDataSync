using Somtoday2MicrosoftSchoolDataSync.UmService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Somtoday2MicrosoftSchoolDataSync.Models
{
    class VestigingLesgroepModel
    {
        public wisVestiging Vestiging { get; set; }
        public List<wisLesgroep> Lesgroepen { get; set; }
    }
}
