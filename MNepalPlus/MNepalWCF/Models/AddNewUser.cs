namespace MNepalWCF.Models
{
    public class AddNewUser
    {
        public string tid;
        public string sc;
        public string fname;
        public string mname;
        public string lname;
        public string amobile;
        public string umobile;
        public string amount;
        public string dob;
        public string street;
        public string ward;
        public string district;
        public string zone;
        public string photoid;
        public string ivrlang;

        public AddNewUser() { }

        public AddNewUser(string tid,string sc, string fname, string mname, string lname, string amobile, string umobile, string amount,string dob, string street, string ward, string district, string zone, string photoid, string ivrlang)
        {
            this.tid = tid;
            this.sc = sc;
            this.fname = fname;
            this.mname = mname;
            this.lname = lname;
            this.amobile = amobile;
            this.umobile = umobile;
            this.amount = amount;
            this.dob = dob;
            this.street = street;
            this.ward = ward;
            this.district = district;
            this.zone = zone;
            this.photoid = photoid;
            this.ivrlang = ivrlang;
        }
    }
}