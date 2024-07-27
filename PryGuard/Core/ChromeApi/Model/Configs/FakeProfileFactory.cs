using System;
using System.Text;
using System.Linq;
using System.Globalization;
using System.Diagnostics; //Debug
using System.ComponentModel;
using PryGuard.Services.Helpers;
using System.Collections.Generic;
using System.Security.Cryptography;
using PryGuard.Core.ChromeApi.Settings;

namespace PryGuard.Core.ChromeApi.Model.Configs;
public class FakeProfileFactory
{
    private static readonly Dictionary<EOSVersion, string> OsVersions = new Dictionary<EOSVersion, string>()
        {
            {EOSVersion.Win7, "Windows NT 6.1"}, {EOSVersion.Win8, "Windows NT 6.2"},
            {EOSVersion.Win81, "Windows NT 6.3"}, {EOSVersion.Win10, "Windows NT 10.0"},
        };

    private static readonly List<string> ChromeBuildVersion = new List<string>()
        {
            "117.0.5938.130",
            "117.0.5938.127",
            "117.0.5938.128",
            "117.0.5938.129",
            "117.0.5938.126",
            "117.0.5938.125",
            "117.0.5938.123",
            "117.0.5938.124",
            "117.0.5938.122"
        };


    public static List<ScreenSize> ScreenSizes { get; } = new List<ScreenSize>()
        {
            new ScreenSize(1280, 768),
            new ScreenSize(1366, 768),
            new ScreenSize(1440, 900),
            new ScreenSize(1920, 1080),
            new ScreenSize(1280, 1024)
        };

    public static List<int> CpuConcurrency { get; } = new List<int>() { 2, 4, 6, 8, 12 };
    public static List<int> MemoryAvailable { get; } = new List<int>() { 2, 4, 6, 8, 16 };

    private static HashSet<string> _allFonts = new HashSet<string>()
        {
            "AIGDT", "AMGDT", "AcadEref", "Adobe Arabic", "Adobe Caslon Pro", "Adobe Caslon Pro Bold",
            "Adobe Devanagari", "Adobe Fan Heiti Std B", "Adobe Fangsong Std R", "Adobe Garamond Pro",
            "Adobe Garamond Pro Bold", "Adobe Gothic Std B", "Adobe Hebrew", "Adobe Heiti Std R", "Adobe Kaiti Std R",
            "Adobe Ming Std L", "Adobe Myungjo Std M", "Adobe Naskh Medium", "Adobe Song Std L", "Agency FB", "Aharoni",
            "Alexandra Script", "Algerian", "Amadeus", "AmdtSymbols", "AnastasiaScript", "Andalus", "Angsana New",
            "AngsanaUPC", "Annabelle", "Aparajita", "Arabic Transparent", "Arabic Typesetting", "Arial", "Arial Baltic",
            "Arial Black", "Arial CE", "Arial CYR", "Arial Cyr", "Arial Greek", "Arial Narrow", "Arial Rounded MT Bold",
            "Arial TUR", "Arial Unicode MS", "Ariston", "Arno Pro", "Arno Pro Caption", "Arno Pro Display",
            "Arno Pro Light Display", "Arno Pro SmText", "Arno Pro Smbd", "Arno Pro Smbd Caption",
            "Arno Pro Smbd Display", "Arno Pro Smbd SmText", "Arno Pro Smbd Subhead", "Arno Pro Subhead",
            "BankGothic Lt BT", "BankGothic Md BT", "Baskerville Old Face", "Batang", "BatangChe", "Bauhaus 93",
            "Bell Gothic Std Black", "Bell Gothic Std Light", "Bell MT", "Berlin Sans FB", "Berlin Sans FB Demi",
            "Bernard MT Condensed", "Bickham Script One", "Bickham Script Pro Regular", "Bickham Script Pro Semibold",
            "Bickham Script Two", "Birch Std", "Blackadder ITC", "Blackoak Std", "Bodoni MT", "Bodoni MT Black",
            "Bodoni MT Condensed", "Bodoni MT Poster Compressed", "Book Antiqua", "Bookman Old Style",
            "Bookshelf Symbol 7", "Bradley Hand ITC", "Britannic Bold", "Broadway", "Browallia New", "BrowalliaUPC",
            "Brush Script MT", "Brush Script Std", "Calibri", "Calibri Light", "Californian FB", "Calisto MT",
            "Calligraph", "Cambria", "Cambria Math", "Candara", "Carolina", "Castellar", "Centaur", "Century",
            "Century Gothic", "Century Schoolbook", "Ceremonious Two", "Chaparral Pro", "Chaparral Pro Light",
            "Charlemagne Std", "Chiller", "CityBlueprint", "Clarendon BT", "Clarendon Blk BT", "Clarendon Lt BT",
            "Colonna MT", "Comic Sans MS", "CommercialPi BT", "CommercialScript BT", "Complex", "Consolas",
            "Constantia", "Cooper Black", "Cooper Std Black", "Copperplate Gothic Bold", "Copperplate Gothic Light",
            "Copyist", "Corbel", "Cordia New", "CordiaUPC", "CountryBlueprint", "Courier", "Courier New",
            "Courier New Baltic", "Courier New CE", "Courier New CYR", "Courier New Cyr", "Courier New Greek",
            "Courier New TUR", "Curlz MT", "DFKai-SB", "DaunPenh", "David", "Decor", "DejaVu Sans",
            "DejaVu Sans Condensed", "DejaVu Sans Light", "DejaVu Sans Mono", "DejaVu Serif", "DejaVu Serif Condensed",
            "DilleniaUPC", "DokChampa", "Dotum", "DotumChe", "Dutch801 Rm BT", "Dutch801 XBd BT", "Ebrima",
            "Eccentric Std", "Edwardian Script ITC", "Elephant", "Engravers MT", "Eras Bold ITC", "Eras Demi ITC",
            "Eras Light ITC", "Eras Medium ITC", "Estrangelo Edessa", "EucrosiaUPC", "Euphemia", "EuroRoman",
            "Eurostile", "FangSong", "Felix Titling", "Fixedsys", "Footlight MT Light", "Forte", "FrankRuehl",
            "Franklin Gothic Book", "Franklin Gothic Demi", "Franklin Gothic Demi Cond", "Franklin Gothic Heavy",
            "Franklin Gothic Medium", "Franklin Gothic Medium Cond", "Freehand521 BT", "FreesiaUPC", "Freestyle Script",
            "French Script MT", "Futura Md BT", "GDT", "GENISO", "Gabriola", "Gadugi", "Garamond", "Garamond Premr Pro",
            "Garamond Premr Pro Smbd", "Gautami", "Gentium Basic", "Gentium Book Basic", "Georgia", "Giddyup Std",
            "Gigi", "Gill Sans MT", "Gill Sans MT Condensed", "Gill Sans MT Ext Condensed Bold", "Gill Sans Ultra Bold",
            "Gill Sans Ultra Bold Condensed", "Gisha", "Gloucester MT Extra Condensed", "GothicE", "GothicG", "GothicI",
            "Goudy Old Style", "Goudy Stout", "GreekC", "GreekS", "Gulim", "GulimChe", "Gungsuh", "GungsuhChe",
            "Haettenschweiler", "Harlow Solid Italic", "Harrington", "Heather Script One", "Helvetica",
            "High Tower Text", "Hobo Std", "ISOCP", "ISOCP2", "ISOCP3", "ISOCPEUR", "ISOCT", "ISOCT2", "ISOCT3",
            "ISOCTEUR", "Impact", "Imprint MT Shadow", "Informal Roman", "IrisUPC", "Iskoola Pota", "Italic", "ItalicC",
            "ItalicT", "JasmineUPC", "Jokerman", "Juice ITC", "KaiTi", "Kalinga", "Kartika", "Khmer UI", "KodchiangUPC",
            "Kokila", "Kozuka Gothic Pr6N B", "Kozuka Gothic Pr6N EL", "Kozuka Gothic Pr6N H", "Kozuka Gothic Pr6N L",
            "Kozuka Gothic Pr6N M", "Kozuka Gothic Pr6N R", "Kozuka Gothic Pro B", "Kozuka Gothic Pro EL",
            "Kozuka Gothic Pro H", "Kozuka Gothic Pro L", "Kozuka Gothic Pro M", "Kozuka Gothic Pro R",
            "Kozuka Mincho Pr6N B", "Kozuka Mincho Pr6N EL", "Kozuka Mincho Pr6N H", "Kozuka Mincho Pr6N L",
            "Kozuka Mincho Pr6N M", "Kozuka Mincho Pr6N R", "Kozuka Mincho Pro B", "Kozuka Mincho Pro EL",
            "Kozuka Mincho Pro H", "Kozuka Mincho Pro L", "Kozuka Mincho Pro M", "Kozuka Mincho Pro R", "Kristen ITC",
            "Kunstler Script", "Lao UI", "Latha", "Leelawadee", "Letter Gothic Std", "Levenim MT",
            "Liberation Sans Narrow", "LilyUPC", "Lithos Pro Regular", "Lucida Bright", "Lucida Calligraphy",
            "Lucida Console", "Lucida Fax", "Lucida Handwriting", "Lucida Sans", "Lucida Sans Typewriter",
            "Lucida Sans Unicode", "MS Gothic", "MS Mincho", "MS Outlook", "MS PGothic", "MS PMincho",
            "MS Reference Sans Serif", "MS Reference Specialty", "MS Sans Serif", "MS Serif", "MS UI Gothic",
            "MT Extra", "MV Boli", "Magneto", "Maiandra GD", "Malgun Gothic", "Mangal", "Marlett",
            "Matura MT Script Capitals", "Meiryo", "Meiryo UI", "Mesquite Std", "Microsoft Himalaya",
            "Microsoft JhengHei", "Microsoft JhengHei UI", "Microsoft New Tai Lue", "Microsoft PhagsPa",
            "Microsoft Sans Serif", "Microsoft Tai Le", "Microsoft Uighur", "Microsoft YaHei", "Microsoft YaHei UI",
            "Microsoft Yi Baiti", "MingLiU", "MingLiU-ExtB", "MingLiU_HKSCS", "MingLiU_HKSCS-ExtB", "Minion Pro",
            "Minion Pro Cond", "Minion Pro Med", "Minion Pro SmBd", "Miriam", "Miriam Fixed", "Mistral", "Modern",
            "Modern No. 20", "Mongolian Baiti", "Monospac821 BT", "Monotxt", "Monotype Corsiva", "MoolBoran",
            "Myriad Arabic", "Myriad Hebrew", "Myriad Pro", "Myriad Pro Cond", "Myriad Pro Light", "Myriad Web Pro",
            "NSimSun", "Narkisim", "Niagara Engraved", "Niagara Solid", "Nirmala UI", "Nueva Std", "Nueva Std Cond",
            "Nyala", "OCR A Extended", "OCR A Std", "OCR-A BT", "OCR-B 10 BT", "Old English Text MT", "Onyx",
            "OpenSymbol", "Orator Std", "Ouverture script", "PMingLiU", "PMingLiU-ExtB", "Palace Script MT",
            "Palatino Linotype", "PanRoman", "Papyrus", "Parchment", "Perpetua", "Perpetua Titling MT",
            "Plantagenet Cherokee", "Playbill", "Poor Richard", "Poplar Std", "Prestige Elite Std", "Pristina",
            "Proxy 1", "Proxy 2", "Proxy 3", "Proxy 4", "Proxy 5", "Proxy 6", "Proxy 7", "Proxy 8", "Proxy 9", "Raavi",
            "Rage Italic", "Ravie", "Rockwell", "Rockwell Condensed", "Rockwell Extra Bold", "Rod", "Roman", "RomanC",
            "RomanD", "RomanS", "RomanT", "Romantic", "Rosewood Std Regular", "Sakkal Majalla", "SansSerif", "Script",
            "Script MT Bold", "ScriptC", "ScriptS", "Segoe Print", "Segoe Script", "Segoe UI", "Segoe UI Light",
            "Segoe UI Semibold", "Segoe UI Semilight", "Segoe UI Symbol", "Shonar Bangla", "Showcard Gothic", "Shruti",
            "SimHei", "SimSun", "SimSun-ExtB", "Simplex", "Simplified Arabic", "Simplified Arabic Fixed", "Small Fonts",
            "Snap ITC", "Square721 BT", "Stencil", "Stencil Std", "Stylus BT", "SuperFrench", "Swis721 BT",
            "Swis721 BdCnOul BT", "Swis721 BdOul BT", "Swis721 Blk BT", "Swis721 BlkCn BT", "Swis721 BlkEx BT",
            "Swis721 BlkOul BT", "Swis721 Cn BT", "Swis721 Ex BT", "Swis721 Hv BT", "Swis721 Lt BT", "Swis721 LtCn BT",
            "Swis721 LtEx BT", "Syastro", "Sylfaen", "Symap", "Symath", "Symbol", "Symeteo", "Symusic", "System",
            "Tahoma", "TeamViewer8", "Technic", "TechnicBold", "TechnicLite", "Tekton Pro", "Tekton Pro Cond",
            "Tekton Pro Ext", "Tempus Sans ITC", "Terminal", "Times New Roman", "Times New Roman Baltic",
            "Times New Roman CE", "Times New Roman CYR", "Times New Roman Cyr", "Times New Roman Greek",
            "Times New Roman TUR", "Traditional Arabic", "Trajan Pro", "Trebuchet MS", "Tunga", "Tw Cen MT",
            "Tw Cen MT Condensed", "Tw Cen MT Condensed Extra Bold", "Txt", "UniversalMath1 BT", "Utsaah", "Vani",
            "Verdana", "Vijaya", "Viner Hand ITC", "Vineta BT", "Vivaldi", "Vladimir Script", "Vrinda",
            "WP Arabic Sihafa", "WP ArabicScript Sihafa", "WP CyrillicA", "WP CyrillicB", "WP Greek Century",
            "WP Greek Courier", "WP Greek Helve", "WP Hebrew David", "WP MultinationalA Courier",
            "WP MultinationalA Helve", "WP MultinationalA Roman", "WP MultinationalB Courier",
            "WP MultinationalB Helve", "WP MultinationalB Roman", "WST_Czec", "WST_Engl", "WST_Fren", "WST_Germ",
            "WST_Ital", "WST_Span", "WST_Swed", "Webdings", "Wide Latin", "Wingdings", "Wingdings 2", "Wingdings 3",
            "ZWAdobeF"
        };

    private static HashSet<string> _fontsWin10 = new HashSet<string>()
        {
            "Arial", "Calibri", "Cambria", "Cambria Math", "Candara", "Comic Sans MS", "Comic Sans MS Bold",
            "Comic Sans", "Consolas", "Constantia", "Corbel", "Courier New", "Caurier Regular", "Ebrima",
            "Fixedsys Regular", "Franklin Gothic", "Gabriola Regular", "Gadugi", "Georgia",
            "HoloLens MDL2 Assets Regular", "Impact Regular", "Javanese Text Regular", "Leelawadee UI",
            "Lucida Console Regular", "Lucida Sans Unicode Regular", "Malgun Gothic", "Microsoft Himalaya Regular",
            "Microsoft JhengHei", "Microsoft JhengHei UI", "Microsoft PhangsPa", "Microsoft Sans Serif Regular",
            "Microsoft Tai Le", "Microsoft YaHei", "Microsoft YaHei UI", "Microsoft Yi Baiti Regular",
            "MingLiU_HKSCS-ExtB Regular", "MingLiu-ExtB Regular", "Modern Regular", "Mongolia Baiti Regular",
            "MS Gothic Regular", "MS PGothic Regular", "MS Sans Serif Regular", "MS Serif Regular",
            "MS UI Gothic Regular", "MV Boli Regular", "Myanmar Text", "Nimarla UI", "MV Boli Regular", "Myanmar Tet",
            "Nirmala UI", "NSimSun Regular", "Palatino Linotype", "PMingLiU-ExtB Regular", "Roman Regular",
            "Script Regular", "Segoe MDL2 Assets Regular", "Segoe Print", "Segoe Script", "Segoe UI",
            "Segoe UI Emoji Regular", "Segoe UI Historic Regular", "Segoe UI Symbol Regular", "SimSun Regular",
            "SimSun-ExtB Regular", "Sitka Banner", "Sitka Display", "Sitka Heading", "Sitka Small", "Sitka Subheading",
            "Sitka Text", "Small Fonts Regular", "Sylfaen Regular", "Symbol Regular", "System Bold", "Tahoma",
            "Terminal", "Times New Roman", "Trebuchet MS", "Verdana", "Webdings Regular", "Wingdings Regular",
            "Yu Gothic", "Yu Gothic UI", "Arial", "Arial Black", "Calibri", "Calibri Light", "Cambria", "Cambria Math",
            "Candara", "Comic Sans MS", "Consolas", "Constantia", "Corbel", "Courier", "Courier New", "Ebrima",
            "Fixedsys", "Franklin Gothic Medium", "Gabriola", "Gadugi", "Georgia", "HoloLens MDL2 Assets", "Impact",
            "Javanese Text", "Leelawadee UI", "Leelawadee UI Semilight", "Lucida Console", "Lucida Sans Unicode",
            "MS Gothic", "MS PGothic", "MS Sans Serif", "MS Serif", "MS UI Gothic", "MV Boli", "Malgun Gothic",
            "Malgun Gothic Semilight", "Marlett", "Microsoft Himalaya", "Microsoft JhengHei",
            "Microsoft JhengHei Light", "Microsoft JhengHei UI", "Microsoft JhengHei UI Light", "Microsoft New Tai Lue",
            "Microsoft PhagsPa", "Microsoft Sans Serif", "Microsoft Tai Le", "Microsoft YaHei", "Microsoft YaHei Light",
            "Microsoft YaHei UI", "Microsoft YaHei UI Light", "Microsoft Yi Baiti", "MingLiU-ExtB",
            "MingLiU_HKSCS-ExtB", "Modern", "Mongolian Baiti", "Myanmar Text", "NSimSun", "Nirmala UI",
            "Nirmala UI Semilight", "PMingLiU-ExtB", "Palatino Linotype", "Roman", "Script", "Segoe MDL2 Assets",
            "Segoe Print", "Segoe Script", "Segoe UI", "Segoe UI Black", "Segoe UI Emoji", "Segoe UI Historic",
            "Segoe UI Light", "Segoe UI Semibold", "Segoe UI Semilight", "Segoe UI Symbol", "SimSun", "SimSun-ExtB",
            "Sitka Banner", "Sitka Display", "Sitka Heading", "Sitka Small", "Sitka Subheading", "Sitka Text",
            "Small Fonts", "Sylfaen", "Symbol", "System", "Tahoma", "Terminal", "Times New Roman", "Trebuchet MS",
            "Verdana", "Webdings", "Wingdings", "Yu Gothic", "Yu Gothic Light", "Yu Gothic Medium", "Yu Gothic UI",
            "Yu Gothic UI Light", "Yu Gothic UI Semibold", "Yu Gothic UI Semilight"
        };

    private static HashSet<string> _fontsWin7 = new HashSet<string>()
        {
            "Aharoni Bold", "Andalus Regular", "Angsana New", "Angsana New Bold", "Angsana New Italic",
            "Angsana New Bold Italic", "AngsanaUPC", "AngsanaUPC Bold", "AngsanaUPC Italic", "AngsanaUPC Bold Italic",
            "Aparajita", "Aparajita Bold", "Aparajita Italic", "Aparajita Bold Italic", "Arabic Typesetting Regular",
            "Arial Unicode MS Regular", "Arial", "Arial Bold", "Arial Black", "Arial Italic", "Arial Bold Italic",
            "Batang", "BatangChe", "Browallia New", "Browallia New Bold", "Browallia New Italic",
            "Browallia New Bold Italic", "BrowalliaUPC", "BrowalliaUPC Bold", "BrowalliaUPC Italic",
            "BrowalliaUPC Bold Italic", "Calibri", "Calibri Bold", "Calibri Italic", "Calibri Bold Italic",
            "Cambria Math", "Cambria", "Cambria Bold", "Cambria Italic", "Cambria Bold Italic", "Candara",
            "Candara Bold", "Candara Italic", "Candara Bold Italic", "Comic Sans MS", "Comic Sans MS Bold", "Consolas",
            "Consolas Bold", "Consolas Italic", "Consolas Bold Italic", "Constantia", "Constantia Bold",
            "Constantia Italic", "Constantia Bold Italic", "Corbel", "Corbel Bold", "Corbel Italic",
            "Corbel Bold Italic", "Cordia New", "Cordia New Bold", "Cordia New Italic", "Cordia New Bold Italic",
            "CordiaUPC", "CordiaUPC Bold", "CordiaUPC Italic", "CordiaUPC Bold Italic", "Courier New",
            "Courier New Bold", "Courier New Italic", "Courier New Bold Italic", "DFKai-SB", "DaunPenh", "David",
            "David Bold", "DilleniaUPC", "DilleniaUPC Bold", "DilleniaUPC Italic", "DilleniaUPC Bold Italic",
            "DokChampa", "Dotum", "DotumChe", "Ebrima", "Ebrima Bold", "Estrangelo Edessa", "EucrosiaUPC",
            "EucrosiaUPC Bold", "EucrosiaUPC Italic", "EucrosiaUPC Bold Italic", "Euphemia", "FangSong", "FrankRuehl",
            "Franklin Gothic Medium", "Franklin Gothic Medium Italic", "FreesiaUPC", "FreesiaUPC Bold",
            "FreesiaUPC Italic", "FreesiaUPC Bold Italic", "Gabriola", "Gautami", "Gautami Bold", "Georgia",
            "Georgia Bold", "Georgia Italic", "& Georgia Bold Italic", "Gisha", "Gisha Bold", "Gulim", "GulimChe",
            "Gungsuh", "GungsuhChe", "Impact", "IrisUPC", "IrisUPC Bold", "IrisUPC Italic", "IrisUPC Bold Italic",
            "Iskoola Pota", "IskoolaPota Bold", "JasmineUPC", "JasmineUPC Bold", "JasmineUPC Italic",
            "JasmineUPC Bold Italic", "KaiTi", "Kalinga", "Kalinga Bold", "Kartika", "Kartika Bold", "Khmer UI",
            "Khmer UI Bold", "KodchiangUPC", "KodchiangUPC Bold", "KodchiangUPC Italic", "KodchiangUPC Bold Italic",
            "Kokila", "Kokila Bold", "Kokila Italic", "Kokila Bold Italic", "Lao UI", "Lao UI Bold", "Latha",
            "Latha Bold", "Leelawadee", "Leelawadee Bold", "Levenim MT", "Levenim MT Bold", "LilyUPC", "LilyUPC Bold",
            "LilyUPC Italic", "LilyUPC Bold Italic", "Lucida Console", "Lucida Sans Unicode", "MS Gothic", "MS Mincho",
            "MS PGothic", "MS PMincho", "MS UI Gothic", "MV Boli", "Malgun Gothic", "Malgun Gothic Bold", "Mangal",
            "Mangal Bold", "Meiryo UI", "Meiryo UI Bold", "Meiryo UI Italic", "Meiryo UI Bold Italic", "Meiryo",
            "Meiryo Bold", "Meiryo Italic", "Meiryo Bold Italic", "Microsoft Himalaya", "Microsoft JhengHei",
            "Microsoft JhengHei Bold", "Microsoft New Tai Lue", "Microsoft New Tai Lue Bold", "Microsoft PhagsPa",
            "Microsoft PhagsPa Bold", "Microsoft Sans Serif", "Microsoft Tai Le", "Microsoft Tai Le Bold",
            "Microsoft Uighur", "Microsoft YaHei", "Microsoft YaHei Bold", "Microsoft Yi Baiti", "MingLiU",
            "MingLiU-ExtB", "MingLiU_HKSCS", "MingLiU_HKSCS-ExtB", "Miriam", "Miriam Fixed", "Mongolian Baiti",
            "MoolBoran", "NSimSun", "Narkisim", "Nyala", "PMingLiU", "PMingLiU-ExtB", "Palatino Linotype",
            "Palatino Linotype Bold", "Palatino Linotype Italic", "Palatino Linotype Bold Italic",
            "Plantagenet Cherokee", "Raavi", "Raavi Bold", "Rod", "Sakkal Majalla", "Sakkal Majalla Bold",
            "Segoe Print", "Segoe Print Bold", "Segoe Script", "Segoe Script Bold", "Segoe UI Symbol", "Segoe UI",
            "Segoe UI Bold", "Segoe UI Italic", "Segoe UI Bold Italic", "Segoe UI Light", "Segoe UI Semibold",
            "Shonar Bangla", "Shonar Bangla Bold", "Shruti", "Shruti Bold", "SimHei", "SimSun", "SimSun-ExtB",
            "Simplified Arabic", "Simplified Arabic Bold", "Simplified Arabic Fixed", "Sylfaen", "Symbol", "Tahoma",
            "Tahoma Bold", "Times New Roman", " Times New Roman Bold", "Times New Roman Italic",
            "Times New Roman Bold Italic", "Traditional Arabic", "Traditional Arabic Bold", "Trebuchet MS",
            "Trebuchet MS Bold", "Trebuchet MS Italic", "Trebuchet MS Bold Italic", "Tunga", "Tunga Bold", "Utsaah",
            "Utsaah Bold", "Utsaah Italic", "Utsaah Bold Italic", "Vani", "Vani Bold", "Verdana", "Verdana Bold",
            "Verdana Italic", "Verdana Bold Italic", "Vijaya", "Vijaya Bold", "Vrinda", "Vrinda Bold", "Webdings",
            "Wingdings"
        };

    public static FakeProfile Generate()
    {
        bool isX64 = RandomNumber.Between(0, 2) == 0;
        FakeProfile result = new FakeProfile()
        {
            BrowserTypeType = GetAllEnumValues<EBrowserType>(typeof(EBrowserType)).GetRandValue(),
            OsVersion = GetAllEnumValues<EOSVersion>(typeof(EOSVersion)).GetRandValue(),
            IsX64 = isX64
        };
        result.IsSendDoNotTrack = true;
        result.HideCanvas = true;
        result.UserAgent = GenerateUserAgent(result);
        result.CpuConcurrency = CpuConcurrency.GetRandValue();
        result.MemoryAvailable = result.CpuConcurrency != 12
            ? MemoryAvailable.Where(x => x >= result.CpuConcurrency).ToList().GetRandValue()
            : 8;
        result.CanvasFingerPrintHash =
            GetMd5Hash(result.UserAgent + DateTime.Now.ToString(CultureInfo.InvariantCulture));
        result.BaseLatency = GenerateBaseLatencyValue();
        result.ChannelDataDelta = GenerateRandomDouble();
        result.ChannelDataIndexDelta = GenerateRandomDouble();
        result.FloatFrequencyDataDelta = GenerateRandomDouble();
        result.FloatFrequencyDataIndexDelta = GenerateRandomDouble();
        result.ChromeLanguageInfo = EChromeLanguageHelper.GetFullInfo(EChromeLanguageExtensions.GetValue(GenerateRandomInt(1, 2)));
        result.ScreenSize = ScreenSizes[GenerateRandomInt(0, 4)];
        result.Fonts = GenerateAvailableFonts(result.OsVersion);
        result.WebGL = WebGLFactory.Generate();

        result.MediaDevicesSettings = new MediaDevicesSettings(GenerateRandomInt(0, 1), GenerateRandomInt(1, 3),
            GenerateRandomInt(0, 4));
        result.WebRtcSettings = new WebRTCSettings();
        result.GeoSettings = new GeoSettings();
        result.TimezoneSetting = new TimezoneSetting();

        
        return result;
    }

    public static FakeProfile Generate(PryGuard.Model.Fingerprint botFingerprint)
    {
        var fakeProfile = Generate();
        fakeProfile.Fonts = botFingerprint.Fonts;
        fakeProfile.WebGL = WebGLFactory.Generate(botFingerprint);
        return fakeProfile;
    }

    private static List<string> GenerateAvailableFonts(EOSVersion winVersion)
    {
        HashSet<string> source = new HashSet<string>();
        List<string> list = _allFonts.ToList();
        int count = list.Count;
        int int32 = Convert.ToInt32(count * 0.6);
        for (int index = 0; index < list.Count; ++index)
        {
            if (RandomNumber.Between(0, count) < int32)
                source.Add(list[index]);
        }

        return source.ToList();
    }

    public static double GenerateRandomDouble()
    {
        return RandomNumber.Between(1000, 9999) * 0.0001;
    }

    public static double GenerateRandomDouble(int from, int to)
    {
        return RandomNumber.Between(from, to);
    }

    public static int GenerateRandomInt(int from, int to)
    {
        return RandomNumber.Between(from, to);
    }

    private static string GenerateUserAgent(FakeProfile fakeProfile)
    {
        return "Mozilla/5.0 (" +
               GetOSInfo(fakeProfile.OsVersion, fakeProfile.IsX64) +
               ") AppleWebKit/537.36 (KHTML, like Gecko) Chrome/"
               + ChromeBuildVersion.GetRandValue()
               + " Safari/537.36";
    }

    private static string GetX64String(bool isX64)
    {
        if (!isX64)
            return string.Empty;
        return "; Win64; x64";
    }

    private static string GetOSInfo(EOSVersion osVersion, bool isX64)
    {
        return OsVersions[osVersion] + GetX64String(isX64);
    }

    private static List<T> GetAllEnumValues<T>(Type type)
    {
        if (type == null)
            throw new ArgumentNullException("type == null");
        if (!type.IsEnum)
            throw new InvalidEnumArgumentException(type.Name + " is not Enum");
        return Enum.GetValues(type).Cast<T>().ToList();
    }

    private static string GetMd5Hash(string value)
    {
        byte[] hash = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(value));
        StringBuilder stringBuilder = new StringBuilder();
        foreach (byte num in hash)
            stringBuilder.Append(num.ToString("x2"));
        return stringBuilder.ToString();
    }

    private static double GenerateBaseLatencyValue()
    {
        return double.Parse(
            string.Format("0,1{0}{1}{2}", RandomNumber.Between(0, 3), RandomNumber.Between(0, 99999),
                RandomNumber.Between(0, 9999999)), CultureInfo.GetCultureInfo("ru-Ru"));
    }
}