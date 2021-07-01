using Microsoft.AspNetCore.Mvc;

public class CustomOptions
{
    public string Url { get; set; }
    public string PublicKey { get; set; }
    public string SessionKey {get; set;}
    public string PreMasterKey { get; set; }
    public string PrivateKey { get; set; }

}