﻿namespace IA.Identity.API.Extensions
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public int Expiracao { get; set; }
        public string Emissor { get; set; }
        public string ValidoEm { get; set; }
    }
}