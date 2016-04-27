using Stability.Model;


namespace Stability.PatientBaseDataSetTableAdapters
{

    partial class AddressesTableAdapter
    {

        public long InsertGetID(string Street,short House, short Flat)
        {
            var r = GetDataBy(Street,House,Flat);

            if (r.Count == 0)
            {
                Insert(Street,House,Flat);
                r = GetDataBy(Street, House, Flat);
            }
            return r[0].ID;
        }
    }

    partial class SurnamesTableAdapter
    {
        public long InsertGetID(string name)
        {
            var r = GetDataBy(name);

            if (r.Count == 0)
            {
                Insert(name);
                r = GetDataBy(name);
            }
            return r[0].ID;
        }
    }

    partial class PatronymicsTableAdapter
    {
        public long InsertGetID(string name)
        {
            var r = GetDataBy(name);

            if (r.Count == 0)
            {
                Insert(name);
                r = GetDataBy(name);
            }
            return r[0].ID;
        }
    }

    partial class NamesTableAdapter
    {
      public long InsertGetID(string name)
        {
            var r = GetDataBy(name);

            if (r.Count == 0)
            {
                Insert(name);
                r = GetDataBy(name);
            }
            return r[0].ID;
        }
    }

    partial class PatientTableAdapter
    {
        public long InsertGetID(global::System.Nullable<long> Name_ID, global::System.Nullable<long> Surname_ID,
            global::System.Nullable<long> Patronymic_ID, global::System.Nullable<global::System.DateTime> Birthdate,
            global::System.Nullable<bool> Sex, global::System.Nullable<long> Addr_ID, short Height)
        {
            var r = GetDataBy(Name_ID, Surname_ID, Patronymic_ID, Birthdate, Sex, Addr_ID);
            if (r.Count == 0)
            {
                Insert(Name_ID, Surname_ID, Patronymic_ID, Birthdate, Sex, Addr_ID, Height);
                r = GetDataBy(Name_ID, Surname_ID, Patronymic_ID, Birthdate, Sex, Addr_ID);
            }
            return r[0].ID;
        }

        public long _Insert(global::System.Nullable<long> Name_ID, global::System.Nullable<long> Surname_ID,
        global::System.Nullable<long> Patronymic_ID, global::System.Nullable<global::System.DateTime> Birthdate,
        global::System.Nullable<bool> Sex, global::System.Nullable<long> Addr_ID, short Height)
        {
          
            Insert(Name_ID, Surname_ID, Patronymic_ID, Birthdate, Sex, Addr_ID, Height);
            var r = GetDataBy(Name_ID, Surname_ID, Patronymic_ID, Birthdate, Sex, Addr_ID);
            return r[0].ID;
        }

        public bool Exists(global::System.Nullable<long> Name_ID, global::System.Nullable<long> Surname_ID,
            global::System.Nullable<long> Patronymic_ID, global::System.Nullable<global::System.DateTime> Birthdate,
            global::System.Nullable<bool> Sex, global::System.Nullable<long> Addr_ID, ref long ID)
        {
            var r = GetDataBy(Name_ID, Surname_ID, Patronymic_ID, Birthdate, Sex, Addr_ID);
             if (r.Count == 0)
                 return false;
             ID = r[0].ID;
             return true;
        }
    }
}
namespace Stability {
    
    
    public partial class PatientBaseDataSet {
    }
}
