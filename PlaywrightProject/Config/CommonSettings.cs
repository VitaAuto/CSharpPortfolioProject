using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaywrightProject.Config
{
    public class CommonSettings
    {
        public string? Browser { get; set; }
        public bool Headless { get; set; }
        public string? VaultPass { get; set; }
    }
}
