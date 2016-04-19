﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stability.Model
{
    class cHuman
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public bool Sex { get; set; }
        public DateTime Birthdate { get; set; }

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

    class cAddress
    {
        public string Street { get; set; }
        public short House { get; set; }
        public short Flat { get; set; }
    }

    class cPatient:cHuman
    {
        public cAddress Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Profession { get; set; }
    }

    class cDataBase
    {
        public bool  AddPatient(cPatient pat)
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
            t.Addr_ID = adp_addr.InsertGetID(pat.Address.Street, pat.Address.House, pat.Address.Flat);

            if (!adp_pat.Exists(t.Name_ID, t.Surname_ID, t.Patronymic_ID, t.Birthdate, t.Sex, t.Addr_ID))
            {
                adp_pat.Insert(t.Name_ID, t.Surname_ID, t.Patronymic_ID, t.Birthdate, t.Sex, t.Addr_ID);
                return true;
            }

            return false;
        }
    }
}
