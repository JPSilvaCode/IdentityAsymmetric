namespace IA.WebAPI.Core.Email
{
    public class EmailSetting
    {
        public string Domain { get; set; }
        public int Port { get; set; }
        public string UserEmailName { get; set; }
        public string UserEmailPassword { get; set; }
        public string UserEmail { get; set; }
        public string UserSMTP { get; set; }
        public string ToEmail { get; set; }
        public string CcEmail { get; set; }
    }
}
