namespace UniversityCompanyAppointmentSystem.Common
{
    // Central place for the string keys used to read/write ASP.NET Core Session.
    // Keeping them as constants avoids typos like "CompanyId" vs "companyId" in different files.
    public static class SessionKeys
    {
        public const string CompanyId = "CompanyId";           // set when a Company logs in
        public const string UniversityId = "UniversityId";     // set when a University logs in
        public const string AccountType = "AccountType";       // "Company" or "University"
        public const string DisplayName = "DisplayName";       // name shown in the top navigation bar
    }
}
