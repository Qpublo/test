using System.Text;
using System.IO;
using Microsoft.Extensions.Configuration;


string? ConnectionString;
string? HSMIP;
string? LogsDirectory;
int? HSMPort;
bool isTestSystem = true;
string dir = "";

Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ". Start converter.");

try
{
    IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true);
    IConfigurationRoot root = builder.Build();
    ConnectionString = root.GetValue<string>("ConnectionString");
    HSMIP = root.GetValue<string>("HSMIP");
    LogsDirectory = root.GetValue<string>("LogsDirectory");
    HSMPort = root.GetValue<int>("HSMPort");
    isTestSystem = root.GetValue<bool>("TestSystem");
    if (ConnectionString == null || ConnectionString == "") throw new Exception("Empty parameter ConnectionString");
    if (HSMIP == null || HSMIP == "") throw new Exception("Empty parameter HSMIP");
    if (HSMIP == null || HSMPort <=0) throw new Exception("Empty or invalid parameter parameter HSMPort");
    if (LogsDirectory == null || LogsDirectory == "")
    {
        dir = Directory.GetCurrentDirectory();
    }
    else
    {
        if (Directory.Exists(LogsDirectory)) dir = LogsDirectory;
        else throw new Exception("Logs directory from settings doesn't exist");
    }
}
catch (Exception e)
{
    throw new IOException("Reading app settings error: " + e.Message);
}

//if (true) return;

// Data for test system

string localKey = "";
string salt = "";
string zmk = "";
string zmkSBlock = "";

if (isTestSystem)
{
    localKey = "BDD20103FDCB43CCAC270EC69F867E8E".ToUpper();
    salt = "B7C8E361C89349DA91FA362262371236".ToUpper();
    zmk = "30037DC91B65CA3FFAE22BBB966FE47B";
    zmkSBlock = "5331303036343532544230304530303030672A7934FD00355CDFC0C25DA85F1C742E028EA38F39AC2C25DCC0312FB6C15432314344383537424245323343394135";
}

if (localKey == "" || salt == "" || zmk == "" || zmkSBlock == "")
{
    Console.WriteLine("Not all required keys are defined");
    return;
}

KeysConversion.DBKeysConverter DBconverter = new KeysConversion.DBKeysConverter(localKey, salt, zmk, zmkSBlock, ConnectionString, HSMIP, HSMPort);
DBconverter.generateCommands();
Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ". Done");
