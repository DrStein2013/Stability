using System;
using Stability.Model.Device;

namespace Stability.Model
{
    public class cHuman
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public bool Sex { get; set; }
        public DateTime Birthdate { get; set; }
        public short Height { get; set; }
        public double Weight { get; set; }

        public int Age
        {
            get
            {
                if (DateTime.Today.DayOfYear >= Birthdate.Date.DayOfYear)
                    return DateTime.Today.Year - Birthdate.Date.Year;
                return DateTime.Today.Year - Birthdate.Date.Year-1;
            } 
        }
    }

    public class cAddress
    {
        public string Street { get; set; }
        public short House { get; set; }
        public short Flat { get; set; }
    }

    public class cPatient:cHuman
    {
        public cAddress Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Profession { get; set; }
       // public long ID { get; private set; }

        public cPatient()
        {
        }

        public cPatient(PatientBaseDataSet.Pat_TabRow t)
        {
            Name = t.Name;
            Surname = t.Surname;
            Patronymic = t.Patronymic;
            Sex = t.Sex;
            Height = t.Height;
            Birthdate = t.Birthdate;
            Address = new cAddress() {Street = t.Street, House = t.House, Flat = t.Flat};
  //          ID = t.ID;
        }
    }

    class cAnamnesisEntry   
    {
        public DateTime MeasureDate { get; set; }
        public double Weight { get; set; }
        public string Info { get; set; }
        public DeviceDataEntry Entry { get; set; }
        public double W_k0 { get; set; }
        public double W_k1 { get; set; }
        public double W_k2 { get; set; }
        public double W_k3 { get; set; }
    }

    class cDataBase
    {
        public bool  AddPatient(cPatient pat, ref long ID)
        {
            var adp_name = new PatientBaseDataSetTableAdapters.NamesTableAdapter();
            var adp_surname = new PatientBaseDataSetTableAdapters.SurnamesTableAdapter();
            var adp_patrname = new PatientBaseDataSetTableAdapters.PatronymicsTableAdapter();
            var adp_addr = new PatientBaseDataSetTableAdapters.AddressesTableAdapter();
            var adp_pat = new PatientBaseDataSetTableAdapters.PatientTableAdapter();

            var t = new PatientBaseDataSet.PatientDataTable().NewPatientRow();

            t.Name_ID = adp_name.InsertGetID(pat.Name);
            t.Surname_ID = adp_surname.InsertGetID(pat.Surname);
            t.Patronymic_ID = adp_patrname.InsertGetID(pat.Patronymic);
            t.Sex = pat.Sex;
            t.Birthdate = pat.Birthdate;
            t.Height = pat.Height;
            t.Addr_ID = adp_addr.InsertGetID(pat.Address.Street, pat.Address.House, pat.Address.Flat);


           if (!adp_pat.Exists(t.Name_ID, t.Surname_ID, t.Patronymic_ID, t.Birthdate, t.Sex, t.Addr_ID,ref ID))
            {
                ID = adp_pat._Insert(t.Name_ID, t.Surname_ID, t.Patronymic_ID, t.Birthdate, t.Sex, t.Addr_ID,t.Height);
                return true;
            }

            return false;
        }

        public cPatient FindPatientBy(long ID)
        {
            var adp = new PatientBaseDataSetTableAdapters.Pat_TabTableAdapter();

            var data = adp.GetDataByID(ID);
            if (data.Count != 0)
            {
              return new cPatient(data[0]);
            }
            return null;
        }

        public cPatient FindPatientBy(long ID,ref PatientBaseDataSet.Pat_TabDataTable patTab)
        {
            var adp = new PatientBaseDataSetTableAdapters.Pat_TabTableAdapter();

            var data = adp.GetDataByID(ID);
            if (data.Count != 0)
            {
                patTab = data;
                return new cPatient(data[0]);
            }
            return null;
        }

        public cPatient FindPatientBy(cHuman FIO)
        {
            var adp = new PatientBaseDataSetTableAdapters.Pat_TabTableAdapter();

            var data = adp.GetDataBy(FIO.Name, FIO.Surname, FIO.Patronymic);
            if (data.Count != 0)
            {
                return new cPatient(data[0]);
            }
            return null;
        }

        public PatientBaseDataSet.Pat_TabDataTable GetPatientBy(long ID)
        {
            var adp = new PatientBaseDataSetTableAdapters.Pat_TabTableAdapter();

            var data = adp.GetDataByID(ID);
            return data;  
        }

        public PatientBaseDataSet.Pat_TabDataTable GetPatientBy(cHuman FIO)
        {
            var adp = new PatientBaseDataSetTableAdapters.Pat_TabTableAdapter();

            var data = adp.GetDataBy(FIO.Name, FIO.Surname, FIO.Patronymic);
            return data;
        }

        public void AddAnamnesis(cAnamnesisEntry entry, long Pat_ID = 0)
        {
            var adp_anam = new PatientBaseDataSetTableAdapters.AnamnesisTableAdapter();
            int i = adp_anam.Insert(Pat_ID, DateTime.Now, entry.Weight, entry.Info,
                                    DeviceDataEntry.Serialize(entry.Entry), entry.W_k0, entry.W_k1, entry.W_k2,
                                    entry.W_k3);
            i += 1;
        }

        public cAnamnesisEntry GetAnamnesisBy(long Pat_ID)
        {
            var adp_anam = new PatientBaseDataSetTableAdapters.AnamnesisTableAdapter();
            var data = adp_anam.GetDataBy(Pat_ID);
            if (data.Count != 0)
            {
                return new cAnamnesisEntry()
                {
                    Entry = DeviceDataEntry.Deserialize(data[0].Entries),
                    Weight = data[0].Weight,
                    Info = data[0].Info,
                    MeasureDate = data[0].Datetime,
                    W_k0 = data[0].W_k0,
                    W_k1 = data[0].W_k1,
                    W_k2 = data[0].W_k2,
                    W_k3 = data[0].W_k3
                };
            }
            return null;
        }

        public PatientBaseDataSet.AnamnesisDataTable GetAnamesisRangeBy(long Pat_ID)
        {
            var adp_anam = new PatientBaseDataSetTableAdapters.AnamnesisTableAdapter();
            return adp_anam.GetDataBy(Pat_ID);
        }

        public PatientBaseDataSet.AnamnesisDataTable GetAnamesisRange(DateTime From, DateTime To)
        {
            var adp_anam = new PatientBaseDataSetTableAdapters.AnamnesisTableAdapter();
            var data = adp_anam.GetDataRange(From, To);
            return data;
        }

        public PatientBaseDataSet.Pat_TabDataTable GetPatients(DateTime From, DateTime To)
        {
            var adp_pat = new PatientBaseDataSetTableAdapters.Pat_TabTableAdapter();
            var data = adp_pat.GetDataRange(From, To);
            return data;
        }
    }
}
