using Newtonsoft.Json;
using System;
using System.Management.Automation;
using System.Threading.Tasks;

namespace LetsEncryptClient.DNS
{
    public class PowerShellWindowsDnsUpdater : IDnsUpdater
    {
        private const string _psScript = @"
param (
    [string]$domain,
    [string]$data,
    [string]$key=""_acme - challenge""
)
try
	{
    $x = Get-DnsServerResourceRecord -Name $key -ZoneName $domain -RRType ""TXT""
    $y = $x.Clone()
    $y.RecordData.DescriptiveText = $data
	Set-DnsServerResourceRecord -NewInputObject $y -OldInputObject $x -ZoneName $domain -PassThru
}
catch
{
    $x = Add-DnsServerResourceRecord -DescriptiveText $data -Name $key -Txt -ZoneName $domain
}
return 1
";

        public async Task CreateOrUpdateTXTRecord(string zone, string host, string text)
        {
            Console.WriteLine($"{host}.{zone}: TXT [{text}]");

            using (PowerShell psi = PowerShell.Create())
            {
                psi.AddScript(_psScript);
                psi.AddParameter("domain", zone);
                psi.AddParameter("data", text);
                psi.AddParameter("key", host);

                var output = psi.Invoke();
                JsonConvert.SerializeObject(output).Dump();
            }

            await Task.Delay(1000);
        }
    }
}
