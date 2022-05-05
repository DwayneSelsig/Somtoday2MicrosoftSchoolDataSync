using Somtoday2MicrosoftSchoolDataSync.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Somtoday2MicrosoftSchoolDataSync.Helpers
{
    class ServiceHelper
    {
        private string umServiceBrinNr;
        private string umServiceUsername;
        private string umServicePassword;
        private string umServiceSchooljaar;
        EventLogHelper eh = Program.eh;

        public ServiceHelper(string umServiceBrinNr, string umServiceUsername, string umServicePassword, string umServiceSchooljaar)
        {
            this.umServiceBrinNr = umServiceBrinNr;
            this.umServiceUsername = umServiceUsername;
            this.umServicePassword = umServicePassword;
            this.umServiceSchooljaar = umServiceSchooljaar;
        }

        internal List<UmService.wisVestiging> GetVestigingen(bool filterLocation, string[] locations)
        {
            List<UmService.wisVestiging> vestigingen = new List<UmService.wisVestiging>();
            using (UmService.UmServiceClient us = new UmService.UmServiceClient())
            {
                try
                {
                    Console.Write(string.Format("Vestigingen opvragen: "));
                    vestigingen = us.getInrichtingVestigingen(umServiceBrinNr, umServiceUsername, umServicePassword).ToList();
                    Console.WriteLine(vestigingen.Count());
                }
                catch (Exception ex)
                {
                    eh.WriteLog(String.Format("Vestiging download mislukt: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error, 500);
                }

                if (filterLocation && vestigingen.Count() > 0)
                {
                    Console.Write(string.Format("Vestigingen filteren: "));
                    vestigingen = vestigingen.Where(v => locations.Any(l => v.afkorting.ToLower() == l.ToLower())).ToList();
                    Console.WriteLine(string.Format("{0} vestigingen overgebleven", vestigingen.Count()));
                }
                Console.WriteLine(string.Format("-------------------------------------"));


            }
            return vestigingen.OrderBy(v => v.afkorting).ToList();
        }

        internal List<VestigingLesgroepModel> GetLesgroepVestiging(List<UmService.wisVestiging> vestigingen)
        {
            List<VestigingLesgroepModel> _lesgroepen = new List<VestigingLesgroepModel>();
            using (UmService.UmServiceClient us = new UmService.UmServiceClient())
            {
                int i = 1;
                foreach (UmService.wisVestiging vestiging in vestigingen)
                {
                    List<UmService.wisLesgroep> lesgroepen = new List<UmService.wisLesgroep>();

                    Console.Write(string.Format("{0}. {1}: ", i, vestiging.naam));
                    try
                    {
                        lesgroepen = us.getInrichtingLesgroepen(umServiceBrinNr, umServiceUsername, umServicePassword, umServiceSchooljaar, vestiging.id).ToList();
                        Console.WriteLine(string.Format("{0} lesgroepen", lesgroepen.Count()));
                        _lesgroepen.Add(new VestigingLesgroepModel { Lesgroepen = lesgroepen, Vestiging = vestiging });
                    }
                    catch (Exception ex)
                    {
                        eh.WriteLog(string.Format("Lesgroep download {0} mislukt: {1}", vestiging.naam, ex.Message), System.Diagnostics.EventLogEntryType.Error, 500);
                    }
                    i++;
                }
            }
            Console.WriteLine(string.Format("-------------------------------------"));
            Console.WriteLine(string.Format("{0} vestigingen en {1} lesgroepen gedownload", _lesgroepen.Count(), _lesgroepen.SelectMany(lg => lg.Lesgroepen).Count()));
            Console.WriteLine();
            return _lesgroepen;
        }

        internal List<UserLesgroepModel> GetStudentInfo(List<VestigingLesgroepModel> vestigingLesgroepModel)
        {
            Console.WriteLine(string.Format("Leerlinggegevens opvragen..."));
            List<UserLesgroepModel> _userLesgroepModel = new List<UserLesgroepModel>();
            using (UmService.UmServiceClient us = new UmService.UmServiceClient())
            {
                foreach (var vestigingLesgroep in vestigingLesgroepModel)
                {
                    Console.Write(string.Format("Leerlingen {0}: ", vestigingLesgroep.Vestiging.afkorting));
                    try
                    {
                        List<UmService.webserviceUmObject> lln = us.getDataLeerlingen(umServiceBrinNr, umServiceUsername, umServicePassword, umServiceSchooljaar, vestigingLesgroep.Vestiging.afkorting, null).ToList(); ;
                        Console.WriteLine(string.Format("{0}", lln.Count()));
                        _userLesgroepModel.Add(new UserLesgroepModel { Users = lln, VestigingLesgroep = vestigingLesgroep });
                    }
                    catch (Exception ex)
                    {
                        eh.WriteLog(String.Format("Leerling download mislukt: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error, 500);
                    }
                }
            }
            Console.WriteLine();
            return _userLesgroepModel;
        }

        internal List<UserLesgroepModel> GetGuardianInfo(List<VestigingLesgroepModel> vestigingLesgroepModel)
        {
            Console.WriteLine(string.Format("Oudergegevens opvragen..."));
            List<UserLesgroepModel> _userLesgroepModel = new List<UserLesgroepModel>();
            using (UmService.UmServiceClient us = new UmService.UmServiceClient())
            {
                foreach (var vestigingLesgroep in vestigingLesgroepModel)
                {
                    Console.Write(string.Format("Ouders {0}: ", vestigingLesgroep.Vestiging.afkorting));
                    try
                    {
                        List<UmService.webserviceUmObject> ouders = us.getDataVerzorgers(umServiceBrinNr, umServiceUsername, umServicePassword, umServiceSchooljaar, vestigingLesgroep.Vestiging.afkorting).ToList(); ;
                        Console.WriteLine(string.Format("{0}", ouders.Count()));
                        _userLesgroepModel.Add(new UserLesgroepModel { Users = ouders, VestigingLesgroep = vestigingLesgroep });
                    }
                    catch (Exception ex)
                    {
                        eh.WriteLog(String.Format("Ouders download mislukt: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error, 500);
                    }
                }
            }
            Console.WriteLine();
            return _userLesgroepModel;
        }

        internal List<UserLesgroepModel> GetTeacherInfo(List<VestigingLesgroepModel> vestigingLesgroepModel)
        {
            Console.WriteLine(string.Format("Medewerkergegevens opvragen..."));
            List<UserLesgroepModel> _userLesgroepModel = new List<UserLesgroepModel>();
            using (UmService.UmServiceClient us = new UmService.UmServiceClient())
            {
                foreach (VestigingLesgroepModel vestigingLesgroep in vestigingLesgroepModel)
                {
                    Console.Write(string.Format("Medewerkers {0}: ", vestigingLesgroep.Vestiging.afkorting));
                    try
                    {
                        List<UmService.webserviceUmObject> medw = us.getDataMedewerkers(umServiceBrinNr, umServiceUsername, umServicePassword, umServiceSchooljaar, vestigingLesgroep.Vestiging.afkorting).ToList();
                        Console.WriteLine(string.Format("{0}", medw.Count()));
                        _userLesgroepModel.Add(new UserLesgroepModel { Users = medw, VestigingLesgroep = vestigingLesgroep });
                    }
                    catch (Exception ex)
                    {
                        eh.WriteLog(String.Format("Medewerker download mislukt: {0}", ex.Message), System.Diagnostics.EventLogEntryType.Error, 500);
                    }
                }
            }
            Console.WriteLine();
            return _userLesgroepModel;
        }
    }
}
