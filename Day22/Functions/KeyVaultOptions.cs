using System.Collections;
using System.Collections.Generic;

namespace Day22
{
    public class KeyVaultOptions
    {
        public string KeyVaultName { get; set; }
        public IEnumerable<string> Secrets { get; set; }
    }
}