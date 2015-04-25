//
// AMGD
//

using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BerkeleyEntities;

using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IO;

using eBay.Service.Call;
using eBay.Service.Core.Sdk;
using eBay.Service.Core.Soap;
using System.Text.RegularExpressions;

namespace BSI_eBayOrders
{
    public partial class Form1 : Form
    {
        const int FIRST_PRIORITY = 0; // First priority: USPS and Next days
        const int LOW_PRIORITY = 100; // UPS standar
        const String OUTPUT_ORDERS_FILE = "{0}eBay-{1}-Orders.htm",
                     OUTPUT_LABELS_FILE = "{0}eBay-{1}-Labels.htm";

        String outputDirectory = "C:\\";
        Boolean lStopProcess = false;

        List<eBayOrder> mainOrderList;

        private static ApiContext apiContext = null;

        int gCurrentMarketplace = 0;
        MarketPlace[] marketPlaces = new MarketPlace[4]{ new MarketPlace("ShoesToGo247","ShoesToGo247", "AgAAAA**AQAAAA**aAAAAA**Sp14VA**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wDmYehAZeEpQqdj6x9nY+seQ**XVYBAA**AAMAAA**lKYa1c9HPKmCBrGPmZCHeB1+I+2Qk9YFORHLb/Jw9dmVpR1MZ42B2sh1zy3ZFA9XB00KIEaH2TFN8jqVkI5aXsR0pR99RMZj0Yt7a1S8c6AxPOdKYBJH7qiUiB5foss3A30O/NuymlPFQovVoIE4bkuRsWMm+ZkIJVaLqhwF7NjIbS+2xfx5QtrUWJY9esYGx4gpBjNyfLQFrtfZD7lkPhndGX6JCdTnsUFektUKV7OdIiCI83d/IKnizb0o+T6z+TLvWfnazkbVUDkpbUE/K9vttgOuOfPS0QDutAse1TLnhIL+dceTPHYV/s+A4AhV2ora8sG1YNElJkKidC/llyVixgUmNPngIHTLUUUzxHcN93CagyWfcpSMf2Px36fcGlbPK6yKe9hBKITCh9dNo25JsuZxa0zoKfBLZ7bFbzI2WogEPll+ntCsyJ1MbnhSo6fbTrMAZDBfeIp+rKzHe6Inya+bB5WpRKmxKmY0GTC/JU1mU7HXcDdzPLbKp9zLsSwdn1wJf8OcgFRVdZxblkftK6mYTVZdoY2gG6jP1agM35CuVGuPfRtIyPUyyLHCjIKRAJq+yQgEN/I1gBdJAHIuSP/7hzBBn9CTZiVDCq+ayOqPgCz2ST0Tu6MrRVzQTbQUszSNSOQZxrNMNp/zeKjXrNV/mgQTBeJ1EoMBHNwb4ByXgpZ1iheHIhzfRALiIvduQbXz8o3BTJsjeyoX+Tv4II2EkqA/SMjNOs16iRLfMZ5XdkexQCETEcj11gii","to get the best prices around.",16) ,
                                                         new MarketPlace("OneMillionShoes","OneMillionShoes", "AgAAAA**AQAAAA**aAAAAA**2orTVA**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wGk4GlDJaFqQSdj6x9nY+seQ**XVYBAA**AAMAAA**z7x2Wn67Pf0DXfPfdz4LS43XlfYzE9aGigT0wMdzDJEhzF4frwHRxZ8GlDxSFG6bUMHxJIcdoQlKHQpQjzOWKMTWh6+q2AoAiqQBV1LUhA2xP9z30Tte3zFuss0Ay0TChl9w8PEhv7FqwRePZHj3EjE5cxyas1eOmbpk9HF/0GJ56hbO0675AMbSoZx1PBKYiT/Y7b6LVgwYZRehflV9XAhykJOw5uUCPtueIyMME/IOsZ0+1+726iMDw3nUlkLi93b2o9r88o9R8D8Tgvfwlw0jmu5sGUFH50YVGcscJbpYchoMfkzFjfdts/w1F4csc+/gihnzAzd/LYrkdXJsMG49rMU3x4j98cjEoGnKJ7H5akPFsUef2FguYQMw7I+8Qm5oe/OmRTCYFvzbFRZF6MHHPjpRRF7ZpITidyzF58yLxjOMfweiEy3L7OeNQfmkKMfRUqzz4ilE1X4uA5cLOW414q1rVa1aF1B0igRTOotL0w2nxQFlhG4I++6CziFONI9BpQRsGnJ5y/hpZmu/4/xGiPzoorTUF5rxHSpCrHphHt8ppIGiko0zdEPP/n3ydmN1VmB38V1R5a5yLFgwnd7kc6IaZAU70Taux/lTCToDy75GdiB6FAhLARGxlRjXPny/+NeFtTx2hp5TClX47Ld6OFvVH5ooqih1sQwzwyTLoJd1UCLRcGju1SZxTsq4LbsAvQx7mkPjBFAXWvEIrQuoHoaMvyVoYbs2FARwAmqc+z90DLG6mSBS6595SGAV","to get the best prices around.",32),
                                                         new MarketPlace("Pick-A-Shoe","Pick-a-Shoe", "AgAAAA**AQAAAA**aAAAAA**G1RHUA**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6AFkoKjAZaBpgqdj6x9nY+seQ**XVYBAA**AAMAAA**U/jdy/lbe3UL+YYAzBLkx8LUgyf26P171zWxbgAD7fVqVmpKi3iweEl2zAA5IYvdtrk+KzH5JB2I8O+hRubnZrTnMIQ/SzZo+EIRsngVLGbs0UOMTulGcqzLK83HqZpo6tJvbCUTPAcvyhYUmHX170Aw/yEmZwkCALwQUWpwCqbeefYfdbrI8zeToPPETjZ1lPFBsNuMY451QTZlrIWPZRnXYZ3QcCUgcGr5iZHUusKuWxkqZkK+C6V8C8IhfVRE6tG9ydsANdKNuWpaA+VP4LO/isWxnEmSa6+klE6BPqDT4BXPsfj1nNQJ1UZwePGLEgr0hX/8ViFXKzx0KCC8uPLwWyffjx8D4a1786cHJJup/7HEixZHyKyxD/mXiR2je72qa+s4t0yYpgCf7YF31Om2CbaIECGdI2RIcRKoQbD1nZm4wDQ9myKRSSmjDOictv9lCnCVsnOMiQzIiLJ1ohAH+BMtfPYGNDcSuQDZieON6SeW8k9gfFwfdXtXOcaoB9M5ePnklxboub48FtVMmtxfwRXmb/l1lP24AfB/bHrpj5apMwhrcS6fMog/Sw83Ln2iu5uBzjUKQ1IR7j08LcgUu3wWSG3MBcI9gH2gtd1EndDDP/SvURowbUxYuWMLvtOEsDZ8BJfOYvAvEyo+IYVbc5apiOtt/biLGy4zzRomSJjTYR84BfgFjnQuqDJK5AxmhiUZjMEFoSk1JU8MNzyemZNLH71gJBDysT050o39jooYjP9lgXSyy4GkeORz","to get the best prices around.",64),
                                                         new MarketPlace("Loco Sneakers","LocoSneakers.com", "AgAAAA**AQAAAA**aAAAAA**8x5tUw**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wMkIGoDZCLoQ2dj6x9nY+seQ**XVYBAA**AAMAAA**6Tm7/rCZDqt/Fwc2ivNDugwKlk8FfHkFGIdfXspYmdI5XqcKh7kb6mYfimBezsjBGb45x/ukxTfpU/uicP8vXf6KPrMovtOOUhnrHGpdsmOOfWIWYLuTL7CFv2aQFsN6MfbmeJ3DMVUhQqTjQgT/lH5dIVq6QI+BkjSvJMJ+AhglAD8+ZuxvxhP6Bfh8vhielA6NcW7bIAa2vz+jigZ+jF1oAO9vL9Fc7vckODOTD6kzmUG5I+6lKZIimy4GcxcZQCl23WUuwP4JSgKlssWYDQQIn8GMlZFtAHghL4wrfahslOzTZ4RaSkxwecf8LZaJjJk9u8pgFjOsmji5pZDmdItdUd7+nJSGz7saxMKuc7+xAGb9nZD8nCj3cLE3D8IzLpyig4LohqMX4l2yd1kftg/A0TTSkI0SoP/58sgeGbZhj9+NRGcMtLbnXqbNK2PHxc69gYTGvl+Vtsnxi+BBIuqqSPAiydzTmqPXBwDV0e8rLNx2hQIjHC09g3x6AMIsUCx6cxI5BkovpwpsFRiBFFjf7gC8gdXo4vnr4EtIfUcWQclje6JFTkZNAJhgtt0/rzvMAXdYGkF71OOJTFbh2chNe1KWzlMlOVwwkl1pcrm/1MSCABo+y6mWjSK7ramqWE95DXaprDalFWvllmdFB/0Oy48RmcTfquhPGqDHcWCWTwmUMQlFxYzZpoF81S7GYnBQNbam1dotecEyU8uVj+8ivmCEHBBZFmR4BQ6dR0l730oenscGoTvzF6os4YM7","to get the best prices around.",128)
                                                       };

        String[] lzipcodes = { 
  "005","006","007","008","009","010","011","012","013","014","015","016","017","018","019","020","021","022","023","024","025",
  "026","027","028","029","030","031","032","033","034","035","036","037","038","039","040","041","042","043","044","045","046",
  "047","048","049","050","051","052","053","054","055","056","057","058","059","060","061","062","063","064","065","066","067",
  "068","069","070","071","072","073","074","075","076","077","078","079","080","081","082","083","084","085","086","087","088",
  "089","090","091","092","093","094","095","096","097","098","099","100","101","102","103","104","105","106","107","108","109",
  "110","111","112","113","114","115","116","117","118","119","120","121","122","123","124","125","126","127","128","129","130",
  "131","132","133","134","135","136","137","138","139","140","141","142","143","144","145","146","147","148","149","150","151",
  "152","153","154","155","156","157","158","159","160","161","162","163","164","165","166","167","168","169","170","171","172",
  "173","174","175","176","177","178","179","180","181","182","183","184","185","186","187","188","189","190","191","192","193",
  "194","195","196","197","198","199","200","201","202","203","204","205","206","207","208","209","210","211","212","213","214",
  "215","216","217","218","219","220","221","222","223","224","225","226","227","228","229","230","231","232","233","234","235",
  "236","237","238","239","240","241","242","243","244","245","246","247","248","249","250","251","252","253","254","255","256",
  "257","258","259","260","261","262","263","264","265","266","267","268","269","270","271","272","273","274","275","276","277",
  "278","279","280","281","282","283","284","285","286","287","288","289","290","291","292","293","294","295","296","297","298",
  "299","300","301","302","303","304","305","306","307","308","309","310","311","312","313","314","315","316","317","318","319",
  "320","321","322","323","324","325","326","327","328","329","330","331","332","333","334","335","336","337","338","339","340",
  "341","342","343","344","345","346","347","348","349","350","351","352","353","354","355","356","357","358","359","360","361",
  "362","363","364","365","366","367","368","369","370","371","372","373","374","375","376","377","378","379","380","381","382",
  "383","384","385","386","387","388","389","390","391","392","393","394","395","396","397","398","399","400","401","402","403",
  "404","405","406","407","408","409","410","411","412","413","414","415","416","417","418","419","420","421","422","423","424",
  "425","426","427","428","429","430","431","432","433","434","435","436","437","438","439","440","441","442","443","444","445",
  "446","447","448","449","450","451","452","453","454","455","456","457","458","459","460","461","462","463","464","465","466",
  "467","468","469","470","471","472","473","474","475","476","477","478","479","480","481","482","483","484","485","486","487",
  "488","489","490","491","492","493","494","495","496","497","498","499","500","501","502","503","504","505","506","507","508",
  "509","510","511","512","513","514","515","516","517","518","519","520","521","522","523","524","525","526","527","528","529",
  "530","531","532","533","534","535","536","537","538","539","540","541","542","543","544","545","546","547","548","549","550",
  "551","552","553","554","555","556","557","558","559","560","561","562","563","564","565","566","567","568","569","570","571",
  "572","573","574","575","576","577","578","579","580","581","582","583","584","585","586","587","588","589","590","591","592",
  "593","594","595","596","597","598","599","600","601","602","603","604","605","606","607","608","609","610","611","612","613",
  "614","615","616","617","618","619","620","621","622","623","624","625","626","627","628","629","630","631","632","633","634",
  "635","636","637","638","639","640","641","642","643","644","645","646","647","648","649","650","651","652","653","654","655",
  "656","657","658","659","660","661","662","663","664","665","666","667","668","669","670","671","672","673","674","675","676",
  "677","678","679","680","681","682","683","684","685","686","687","688","689","690","691","692","693","694","695","696","697",
  "698","699","700","701","702","703","704","705","706","707","708","709","710","711","712","713","714","715","716","717","718",
  "719","720","721","722","723","724","725","726","727","728","729","730","731","732","733","734","735","736","737","738","739",
  "740","741","742","743","744","745","746","747","748","749","750","751","752","753","754","755","756","757","758","759","760",
  "761","762","763","764","765","766","767","768","769","770","771","772","773","774","775","776","777","778","779","780","781",
  "782","783","784","785","786","787","788","789","790","791","792","793","794","795","796","797","798","799","800","801","802",
  "803","804","805","806","807","808","809","810","811","812","813","814","815","816","817","818","819","820","821","822","823",
  "824","825","826","827","828","829","830","831","832","833","834","835","836","837","838","839","840","841","842","843","844",
  "845","846","847","848","849","850","851","852","853","854","855","856","857","858","859","860","861","862","863","864","865",
  "866","867","868","869","870","871","872","873","874","875","876","877","878","879","880","881","882","883","884","885","886",
  "887","888","889","890","891","892","893","894","895","896","897","898","899","900","901","902","903","904","905","906","907",
  "908","909","910","911","912","913","914","915","916","917","918","919","920","921","922","923","924","925","926","927","928",
  "929","930","931","932","933","934","935","936","937","938","939","940","941","942","943","944","945","946","947","948","949",
  "950","951","952","953","954","955","956","957","958","959","960","961","962","963","964","965","966","967","968","969","970",
  "971","972","973","974","975","976","977","978","979","980","981","982","983","984","985","986","987","988","989","990","991",
  "992","993","994","995","996","997","998","999" };

        String[] lzones = { 
                               "2","7","7","7","7","2","2","2","2","1","1","1","1","1","1","1","1","1","1","1","1","1","1","1",
                               "1","1","1","1","1","1","2","2","2","1","2","2","2","2","2","3","2","3","3","2","3","2","2","2",
                               "2","2","1","2","2","2","2","2","2","2","2","2","2","2","2","2","2","3","3","3","3","3","3","3",
                               "3","3","3","3","3","3","3","3","3","3","3","3","3","3","3","3","3","3","3","3","3","3","3","3",
                               "3","3","3","3","2","2","2","2","2","3","3","3","3","3","2","3","2","2","2","2","2","2","2","2",
                               "2","2","2","2","3","3","3","3","3","3","3","3","3","3","3","4","4","4","4","4","4","4","4","4",
                               "4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","3","3","3",
                               "3","3","3","3","4","3","3","3","3","3","3","3","3","3","3","3","3","3","3","3","3","3","3","3",
                               "3","3","3","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4",
                               "4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","5","4","4",
                               "4","5","5","5","5","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4","4",
                               "4","4","4","4","4","4","4","4","4","4","4","5","5","5","5","5","4","4","5","5","5","5","5","5",
                               "5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","6",
                               "5","5","5","5","6","5","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6",
                               "6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","5","5","5","6","6","6","5","6","6",
                               "6","6","6","6","6","5","5","5","5","5","6","5","5","5","5","6","6","6","6","5","5","6","6","6",
                               "6","6","6","6","6","6","6","6","6","5","5","5","5","5","5","5","5","5","5","5","5","5","4","4",
                               "5","5","4","4","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5",
                               "5","5","4","4","4","4","4","4","4","4","4","4","4","5","5","5","5","5","5","5","4","5","5","5",
                               "5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","5",
                               "5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","6","6","6","6","6","6","6","6","6",
                               "6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","5","5","5","5","5","5","5",
                               "5","5","5","5","5","5","5","6","5","5","5","5","5","5","5","5","5","6","6","6","6","6","6","6",
                               "6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","7","7","7","7","7","6",
                               "6","6","6","6","7","7","7","7","7","8","8","7","8","8","8","8","8","8","8","5","5","5","5","5",
                               "5","5","5","5","5","5","5","5","5","5","5","5","5","5","5","6","6","6","6","5","5","5","5","5",
                               "5","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6",
                               "6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","7","7",
                               "7","7","7","6","6","6","6","6","6","6","6","6","6","7","7","7","7","7","7","7","7","7","7","6",
                               "6","6","6","6","7","7","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6","6",
                               "6","6","6","6","6","7","7","7","7","7","7","7","7","7","7","6","6","6","6","6","6","7","6","7",
                               "6","7","7","7","7","7","6","7","7","7","7","7","7","7","7","7","7","7","7","7","8","7","7","7",
                               "7","7","7","7","7","7","7","7","7","7","7","7","8","7","7","7","7","7","7","7","7","7","7","7",
                               "8","8","8","7","7","7","7","7","7","7","7","7","7","7","8","8","8","8","8","8","8","8","8","7",
                               "8","7","8","7","7","7","7","7","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8",
                               "8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8",
                               "8","8","8","8","8","8","8","8","8","8","8","8","7","8","8","7","8","8","8","8","8","8","8","8",
                               "8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8",
                               "8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8",
                               "8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8",
                               "8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8","8",
                               "8","8","8","8","8","8","8","8","8","8","8"
                              };


        private eBayOrder extendAddress(OrderType pl)
        // Determines the shipping zone and priority
        {

            String lzip = "", ltheZip = pl.ShippingAddress.PostalCode;
            String lzone;
            int lpriority;

            lzone = "NA";
            lpriority = LOW_PRIORITY;
            if (pl.ShippingAddress.CountryName.Equals("United States"))
            {
                if (pl.ShippingServiceSelected.ShippingService.Contains("UPSNextDayAir") ||
                    pl.ShippingServiceSelected.ShippingService.Contains("Overnight"))
                {
                    lpriority = FIRST_PRIORITY;
                }
                else
                {
                    String lstate = pl.ShippingAddress.StateOrProvince;
                    if (lstate.Equals("AK") || lstate.Equals("HI") || lstate.Equals("PR"))
                    {
                        lpriority = FIRST_PRIORITY;
                        pl.ShippingServiceSelected.ShippingService = "USPSPriorityMail";
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(ltheZip) && ltheZip.Length > 2)
                        {
                            int lx = 0;
                            lzip = ltheZip.Substring(0, 3);
                            foreach (String lzipstring in lzipcodes)
                            {
                                if (lzipstring.Contains(lzip))
                                {
                                    lzone = lzones[lx];
                                    break;
                                }
                                lx++;
                            }; // foreach

                            // Determine the shipping method based on the zone
                            int lzoneNo = 0;
                            if (Int32.TryParse(lzone, out lzoneNo))
                            {
                                if (lzoneNo < 5) lpriority = FIRST_PRIORITY;
                            };
                        }
                    }
                }
            }
            else
            {
                lpriority = FIRST_PRIORITY;
            }

            // Add the item
            eBayOrder lao = new eBayOrder(pl);
            lao.shippingPriority = lpriority;
            lao.zone = lzone;

            return lao;
        } // extendAddress

        public Form1()
        {
            InitializeComponent();
        } // Form1()

        private void btnOutputDirectory_Click(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                outputDirectory = folderBrowserDialog1.SelectedPath;

                if (!outputDirectory.EndsWith("\\"))
                    outputDirectory += "\\";

                this.lblOutput.Text = this.outputDirectory;
            };
        } // btnOutputDirectory_Click

        private void btnStop_Click(object sender, EventArgs e)
        {
            this.lStopProcess = true;
        } // btnStop_Click

        private void btnGeteBay_Click(object sender, EventArgs e)
        {
            this.btnStop.Enabled = true;
            this.btnStop.Visible = true;
            this.btnGeteBay.Enabled = false;
            this.txtStatus.Text = "";
            getTheOrders();
            this.btnStop.Visible = false;
        } // btnGeteBay_Click

        private int compareOrders(eBayOrder p1, eBayOrder p2)
        // Gets 2 orders and sorts them based on the priority.
        // If they have the same priority then it sorts them by the title of the first item
        {
            int lret = p1.shippingPriority - p2.shippingPriority;

            if (lret == 0) // If they're equal sort based on the title of the first item so each carrie's sort by brand)
            {
                if (p1.TransactionArray.Count > 0 && p2.TransactionArray.Count > 0)
                {
                    String ls1, ls2;

                    // Chek if the items are variations or full items
                    if (p1.TransactionArray[0].Variation != null &&
                        !String.IsNullOrEmpty(p1.TransactionArray[0].Variation.SKU))
                        ls1 = p1.TransactionArray[0].Variation.VariationTitle;
                    else
                        ls1 = p1.TransactionArray[0].Item.Title;

                    if (p2.TransactionArray[0].Variation != null &&
                        !String.IsNullOrEmpty(p2.TransactionArray[0].Variation.SKU))
                        ls2 = p2.TransactionArray[0].Variation.VariationTitle;
                    else
                        ls2 = p2.TransactionArray[0].Item.Title;

                    lret = String.Compare(ls1,ls2);
                }
            };

            return lret;
        }

        private int compareOrderItems(ItemType pi1, ItemType pi2)
        {
            int lret = 0;

            if (!String.IsNullOrEmpty(pi1.Title) && !String.IsNullOrEmpty(pi2.Title))
                lret = String.Compare(pi1.Title, pi2.Title);
            return lret;
        }

        private String getConfigurationKey(String pkey)
        {
            String lkey = "";

            for (int i = 0; i < ConfigurationManager.AppSettings.Count; i++)
            {
                if (ConfigurationManager.AppSettings.Keys[i].Contains(pkey))
                {
                    lkey = ConfigurationManager.AppSettings[i];
                    break;
                }
            } // for

            return lkey;
        } // getConfigurationKey

        private string getConnectionString(string pn)
        {
            string ls = null;

            foreach (ConnectionStringSettings lcs in ConfigurationManager.ConnectionStrings)
            {
                if (lcs.Name.IndexOf(pn) >= 0)
                {
                    ls = lcs.ConnectionString;
                    break;
                }
            }; // foreach

            return ls;
        } // getConnectionString

        /// <summary>
        /// Populate eBay SDK ApiContext object with data from application configuration file
        /// </summary>
        /// <returns>ApiContext</returns>
        ApiContext GetApiContext()
        {
                apiContext = new ApiContext();

                //set Api Server Url
                apiContext.SoapApiServerUrl = getConfigurationKey("ApiServerUrl");
                //set Api Token to access eBay Api Server
                ApiCredential apiCredential = new ApiCredential();
                apiCredential.eBayToken = marketPlaces[gCurrentMarketplace].Token;
                apiContext.ApiCredential = apiCredential;
                //set eBay Site target to US
                apiContext.Site = SiteCodeType.US;
                return apiContext;
        } // GetApiContext

        private void getTheOrders()
        {
            SqlCommand lc = null;
            SqlDataReader lr = null;
            SqlConnection lconn = null;
            int ltotalOrders = 0;
            int ltotalPOBox = 0;
            int ltotalItems = 0;
            double lgrandTotal = 0,
                  lgrandShipping = 0;
            String loperationStatus = "";

            string POBoxPattern = @"(?i)\b(?:p(?:ost)?\.?\s*[o0](?:ffice)?\.?\s*b(?:[o0]x)?|b[o0]x)";
            //[Step 1] Initialize eBay ApiContext object
            ApiContext apiContext = GetApiContext();

            //[Step 2] Create Call object and execute the Call
            GetOrdersCall call = new GetOrdersCall(apiContext);

            // x.CreateTimeFrom = DateTime.Now.AddDays(-2);
            call.CreateTimeFrom = DateTime.Now.AddDays(-60);
            call.CreateTimeTo = DateTime.Now.AddMinutes(-15);

            call.DetailLevelList = new DetailLevelCodeTypeCollection();
            call.DetailLevelList.Add(DetailLevelCodeType.ReturnAll);

            PaginationType lpager = new PaginationType();
            lpager.EntriesPerPage = 50;
            mainOrderList = new List<eBayOrder>();
            List<Brand> lbrands = new List<Brand>();
            int lpage = 0;
            lgrandTotal = 0;
            lgrandShipping = 0;
            this.txtStatus.Text = "-----------------> STARTING UP... \r\n";
            this.txtStatus.Update();
            this.lStopProcess = false;

            do
            {
                lpager.PageNumber = ++lpage;
                call.Pagination = lpager;
                OrderTypeCollection ebayOrders = call.GetOrders(call.CreateTimeFrom,
                                                          call.CreateTimeTo,
                                                          TradingRoleCodeType.Seller,
                                                          OrderStatusCodeType.Completed);

                Application.DoEvents();
                this.txtStatus.Text = "-----------------> Checking page " + call.PageNumber + "\r\n" + this.txtStatus.Text;
                this.txtStatus.Update();
                foreach (OrderType ebayOrder in ebayOrders)
                {
                    // Skip shipped and payment-not-completed orders 
                    if (!ebayOrder.ShippedTimeSpecified &&
                         ebayOrder.CheckoutStatus.eBayPaymentStatus == PaymentStatusCodeType.NoPaymentFailure) 
                    {
                        eBayOrder leo = extendAddress(ebayOrder);
                        mainOrderList.Add(leo);
                        Application.DoEvents();
                    }; // if (!lorder.ShippedTimeSpecified)
                }; // foreach
            } while (call.HasMoreOrders && !this.lStopProcess);

            if (!this.lStopProcess)
            {
                this.txtStatus.Text = ltotalOrders + " orders found. ..." + "\r\n" + this.txtStatus.Text;
                this.txtStatus.Update();

                // Sort the orders
                this.txtStatus.Text = "Sorting orders by shipping method and then by 1st item title..." + "\r\n" + this.txtStatus.Text;
                this.txtStatus.Update();
                mainOrderList.Sort(compareOrders);

                // Assign locations
                String lbrand = "";
                this.txtStatus.Text = "Assigning location to each product item..." + "\r\n" + this.txtStatus.Text;
                this.txtStatus.Update();
                try
                {
                    string lcs = getConnectionString("berkeleyConnectionString");
                    lconn = new SqlConnection(lcs);
                    lconn.Open();

                    ltotalOrders = 0;
                    foreach (eBayOrder lorder in mainOrderList)
                    {

                        // Is this a repeated order?
                        bool lrepeatedOrder = false;
                        string lBSIorderID = null;
                        lc = new SqlCommand("SELECT * FROM bsi_orders where marketplaceId=" +
                                            marketPlaces[gCurrentMarketplace].maskId + " AND orderId='" +
                                            lorder.ShippingDetails.SellingManagerSalesRecordNumber + "' ", lconn);
                        lr = lc.ExecuteReader();
                        if (lr.Read())
                        {
                            lrepeatedOrder = true;
                            lorder.repeated = true;
                            lorder.originalDate = DateTime.Parse(lr["printDate"].ToString());
                            lr.Close();
                        }
                        else
                        {
                            lr.Close();
                            lrepeatedOrder = false;
                            lorder.repeated = false;
                            ltotalOrders++;

                            // Save this order
                            lc = new SqlCommand("INSERT INTO bsi_orders (marketplaceId, orderId, printDate) VALUES (" +
                                                marketPlaces[gCurrentMarketplace].maskId + ",'" +
                                                lorder.ShippingDetails.SellingManagerSalesRecordNumber +
                                                "','" + DateTime.Now + "'); SELECT SCOPE_IDENTITY() AS SCOPE_ID", lconn);
                            lBSIorderID = lc.ExecuteScalar().ToString();
                        } // if (lr.Read())
                        lc.Cancel();




                        foreach (TransactionType lt in lorder.TransactionArray)
                        {
                            String lskuX, lsku, lloc, conditionDescription;

                            // Chek if the items are variations or full items
                            if (lt.Variation != null && !String.IsNullOrEmpty(lt.Variation.SKU))
                            {
                                lskuX = lt.Variation.SKU;
                                lbrand = GetBrand(lskuX, lconn);
                                //lbrand = lt.Variation.VariationTitle.Substring(0, lt.Variation.VariationTitle.IndexOf(" ")).ToUpper();
                            }
                            else
                            {
                                lskuX = lt.Item.SKU;
                                lbrand = GetBrand(lskuX, lconn);
                                //lbrand = lt.Item.Title.Substring(0, lt.Item.Title.IndexOf(" ")).ToUpper();
                            }

                            if ( !lrepeatedOrder ) ltotalItems += lt.QuantityPurchased;
                            lloc = "";

                            /*
                            if (lskuX.IndexOf("-", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                lsku = lskuX.Substring(0, lskuX.IndexOf("-", StringComparison.OrdinalIgnoreCase)).Trim();
                            }
                            else
                                lsku = lskuX.Trim();
                            */
                            
                            lsku = ( !string.IsNullOrEmpty(lskuX) ) ? getNormalizedSKU(lskuX.Trim()) : "";
                            lsku = ( !string.IsNullOrEmpty(lskuX) ) ? lskuX.Trim() : "";

                            lc = new SqlCommand("SELECT * FROM Item where itemlookupcode='" + lsku + "'", lconn);
                            lr = lc.ExecuteReader();
                            if (lr.Read())
                            {
                                lloc = " [" + lr["binlocation"].ToString() + " | " + lr["notes"].ToString() + "]";
                                lt.Item.ConditionDescription = lr["extendedDescription"].ToString();
                            }; // if (lr.Read())
                            lr.Close();
                            lc.Cancel();

                            if (lBSIorderID != null)
                            {
                                int lqty = lt.QuantityPurchased;
                                double lprice = lt.TransactionPrice.Value;

                                lc = new SqlCommand("INSERT INTO bsi_orders_details (marketplace,marketplaceid,OrderInOurTables," + 
                                                    "itemlookupcode,price,quantity,discount) " +
                                                    "VALUES (" + marketPlaces[gCurrentMarketplace].maskId + 
                                                             ",'" + lt.Item.ItemID + "','" + lBSIorderID + "','" + 
                                                             lsku + "','" + lprice +"','" + lqty + "','0')", lconn);
                                lc.ExecuteNonQuery();

                                // Now substract the sold quantity from the post
                                String lquantityId = null;
                                String lqs = "SELECT thePost.id,thePost.marketplace,thePost.markerplaceItemID," +
                                             "theQ.id AS QtyID, theQ.postId, theQ.itemLookupCode " +
                                             "FROM bsi_posts AS thePost INNER JOIN " +
                                             "bsi_quantities AS theQ ON thePost.id = theQ.postId " +
                                             "WHERE (thePost.markerplaceItemID = '" + lt.Item.ItemID + "') AND " +
                                             "(theQ.itemLookupCode = '" + lsku + "')";
                                
                                lc = new SqlCommand(lqs, lconn);
                                lr = lc.ExecuteReader();
                                if (lr.Read())
                                {
                                    lquantityId = lr["QtyID"].ToString();
                                }
                                else
                                {
                                    loperationStatus = "\r\nITEM: " + lsku + " IS NOT REGISTERED IN THE TABLE QUANTITIES\r\n" + loperationStatus;
                                }
                                lr.Close();

                                // Deduct the quantity from bsi_quantities
                                if (lquantityId != null)
                                {
                                    lqs = "UPDATE bsi_quantities SET quantity=quantity-" + lqty.ToString() +
                                          " WHERE id=" + lquantityId;
                                    lc = new SqlCommand(lqs, lconn);
                                    lc.ExecuteNonQuery();
                                }
                            }

                            lt.Item.SKU = lskuX + lloc;

                            Brand lbrand4Search = lbrands.Find(delegate(Brand pb)
                            {
                                return (pb.brand.CompareTo(lbrand) == 0);
                            });

                            if (!lrepeatedOrder)
                            {
                                if (lbrand4Search != null)
                                {
                                    lbrand4Search.count += lt.QuantityPurchased;
                                }
                                else
                                {
                                    lbrand4Search = new Brand();
                                    lbrand4Search.brand = lbrand;
                                    lbrand4Search.count = lt.QuantityPurchased;
                                    lbrands.Add(lbrand4Search);
                                }
                            }
                        }; // foreach

                        if (!lrepeatedOrder)
                        {
                            Brand b = lbrands.Find(p => p.brand.Equals(lbrand));
                            b.subtotal += Convert.ToDecimal(lorder.Subtotal.Value);
                        }

                        

                    } // foreach
                }
                catch (Exception pe)
                {
                    MessageBox.Show("SEVERE ERROR: " + pe.ToString(), "Error while processing orders");
                }
                finally
                {
                    if (lconn != null) lconn.Close();
                    if (lr != null) lr.Close();
                    if (lc != null) lc.Cancel();
                }

                // Create the HTML file
                this.txtStatus.Text = "Generating HTML orders file... \r\n" + this.txtStatus.Text;

                StreamWriter lf = File.CreateText(String.Format(OUTPUT_ORDERS_FILE, this.outputDirectory, marketPlaces[gCurrentMarketplace].Name)),
                             lfl = File.CreateText(String.Format(OUTPUT_LABELS_FILE, this.outputDirectory, marketPlaces[gCurrentMarketplace].Name));

                lf.AutoFlush = true; lfl.AutoFlush = true;
                lf.Write("<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>" +
                    "<html xmlns='http://www.w3.org/1999/xhtml'><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8' /><title>" +
                    "Untitled Document</title><style>body {font-family:Tahoma, Geneva, sans-serif;font-size:10px;} p{margin:0px;margin-bottom:5px;} .customerName {font-family:Arial,Helvetica, sans-serif;font-size:large;font-weight:bold;} .titulos {font-family:Arial," +
                    "Helvetica, sans-serif;font-size:large;font-weight:bold;" +
                    ((gCurrentMarketplace % 2 == 0) ? "border-style:solid; border-width:1px;" : " ") +
                    "}.fineprint {font-family:Arial, Helvetica, sans-serif;font-size:x-small;" +
                    "font-weight: normal;text-align:center;} .repeated-order {border-width: 2px;border-color: black;border-style:solid;padding: 2px; margin-right:2px;font-family:Arial, Helvetica, sans-serif;font-size:15px;float:right;} .day-of-week {padding: 1px;background-color: black;color: white;}</style></head><body><div style='width: 800px'>");

                lfl.WriteLine("<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>" + 
                              "<html xmlns='http://www.w3.org/1999/xhtml'><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8' />" + 
                              "<style>.shipToAddress {padding-left: 30px;}</style></head>" +
                              "<body style='font-family:Arial, Helvetica, sans-serif; font-size:13px;'>");


                int lx = 0, lnextdayorders = 0, linternationalOrders = 0;

                foreach (eBayOrder order in mainOrderList)
                {
                    String lshippingInfo = "", lrepeatedInfo = "";

                    this.txtStatus.Text = "Processing " + order.OrderID + "\r\n " + this.txtStatus.Text;

                    if (order.repeated)
                    { 
                        lrepeatedInfo = "<p class='repeated-order'><span class='day-of-week'>" +
                                        order.originalDate.DayOfWeek.ToString().Substring(0, 3).ToUpper() +
                                        "</span>&nbsp;" + order.originalDate.ToShortDateString() + "&nbsp;</p>";
                    }
                    else
                    {
                        lrepeatedInfo = "";
                    }

                    lx++;

                    // Code added on 2013-11-06.
                    // Check: http://developer.ebay.com/DevZone/XML/docs/Reference/eBay/types/ShippingServiceOptionsType.html#ExpeditedService
                    if (order.ShippingServiceSelected.ExpeditedService || order.ShippingServiceSelected.ShippingService.Contains("Overnight") )
                        if (!order.repeated)
                        {
                            lnextdayorders++;
                        }

                    if (order.shippingPriority == FIRST_PRIORITY)
                    {
                        if (order.ShippingServiceSelected.ShippingService.Contains("UPSNextDayAir") )
                        {
                            lshippingInfo = "NEXT DAY | UPS";
                            // if (!lao.repeated) lnextdayorders++;
                        }
                        else
                        {
                            if (order.ShippingAddress.CountryName.ToString().Contains("United States"))
                                lshippingInfo = " POSTAL ";
                            else
                            {
                                lshippingInfo = order.ShippingServiceSelected.ShippingService + " | POSTAL ";
                                
                                /*
                                lfl.WriteLine("<table cellpadding='0' cellspacing='0' style='width: 3.8in; height:3in;'><tr><td style='width:1.5in; height:1.5in;' align='left' valign='top'>" +
                                              "<B>SHOE NATION</B><br/>PO Box 189<br/>Salem, NH 03079<br/>USA</td><td>&nbsp;</td></tr><tr><td>&nbsp;</td><td  align='left' valign='top'>" +
                                              "<p><B>" + lao.ShippingAddress.Name + "</B><br/>" +
                                              lao.ShippingAddress.CompanyName + "<br />" +
                                              lao.ShippingAddress.Street + "<br />" +
                                              lao.ShippingAddress.Street1 + "<br/>" + 
                                              lao.ShippingAddress.Street2 + "<br />" +
                                              lao.ShippingAddress.CityName + ", " + lao.ShippingAddress.StateOrProvince + " " + lao.ShippingAddress.PostalCode + "<br />" +
                                              lao.ShippingAddress.CountryName + "</p>" +
                                              "</td></tr></table>"
                                             );
                                */

                                lfl.WriteLine("");
                                lfl.WriteLine("");
                                lfl.WriteLine("<p><b>SHOE NATION/" + marketPlaces[gCurrentMarketplace].Name + "</b><br>");
                                lfl.WriteLine("PO Box 189<br>");
                                lfl.WriteLine("Salem, NH 03079<br>");
                                lfl.WriteLine("USA</p>");
                                lfl.WriteLine("<p>&nbsp;</p>");
                                lfl.WriteLine("<p>&nbsp;</p>");
                                lfl.WriteLine("<p class='shipToAddress'><b>" + order.ShippingAddress.Name + "</b><br>");
                                lfl.WriteLine(order.ShippingAddress.CompanyName + "<br>");
                                lfl.WriteLine(order.ShippingAddress.Street + "<br>");
                                lfl.WriteLine(order.ShippingAddress.Street1 + "<br>");
                                lfl.WriteLine(order.ShippingAddress.Street2 + "<br>");
                                lfl.WriteLine(order.ShippingAddress.CityName + ", " + order.ShippingAddress.StateOrProvince + " " + order.ShippingAddress.PostalCode + "<br>");
                                lfl.WriteLine(order.ShippingAddress.CountryName + "</p>");
                                lfl.WriteLine("");
                                lfl.WriteLine("<p style='page-break-before: always;'>&nbsp;</p>"); // Page break

                                if (!order.repeated) linternationalOrders++;

                                /*
                                if (linternationalOrders % 2 == 0)
                                    lfl.WriteLine("\f"); // Page break
                                else
                                    lfl.WriteLine("---------------");

                                if (linternationalOrders % 2 == 0)
                                    lfl.WriteLine("<p style='page-break-before: always;'>&nbsp;</p>"); // Page break
                                else
                                    lfl.WriteLine("<hr/>");
                                */
                            };
                        }
                    }
                    else
                    {
                        lshippingInfo = order.ShippingServiceSelected.ShippingService + " | UPS";
                    };

                    lshippingInfo += " | Zone: " + order.zone;

                    lf.Write("<table border='0' bordercolor='#000000' cellpadding='1' cellspacing='0' width='100%'><tr><td style='height:5in'><center>");
                    lf.Write("<table width='800' border='0' cellspacing='0' cellpadding='0'><tr><td align='left' valign='top' width='33%'>");



                    if (!order.repeated)
                    {
                        string street = order.ShippingAddress.Street == null ? " " : order.ShippingAddress.Street;
                        string street1 = order.ShippingAddress.Street1 == null ? " " : order.ShippingAddress.Street1;

                        if (Regex.IsMatch(street, POBoxPattern) || Regex.IsMatch(street1, POBoxPattern))
                        {
                            ltotalPOBox++;
                        }
                    }

                    lf.Write("<tr>" +
                             "    <td align='left' valign='top' width='33%'><p class='customerName'>" + order.ShippingAddress.Name + "</p><p><strong>" +
                             order.BuyerUserID + "<br />" +
                             order.ShippingAddress.CompanyName + "<br />" +
                             order.ShippingAddress.Street + "<br />" +
                             order.ShippingAddress.Street1 + " | " + order.ShippingAddress.Street2 + "<br />" +
                             order.ShippingAddress.CityName + ", " + order.ShippingAddress.StateOrProvince + " " + order.ShippingAddress.PostalCode + "<br />" +
                             order.ShippingAddress.CountryName + "<br />" +
                             order.ShippingAddress.Phone + "</strong></p></td>" +
                             "    <td align='center' valign='top' width='34%'><p class='titulos'>" + marketPlaces[gCurrentMarketplace].Website + "</p>" +
                             "    <p>" + marketPlaces[gCurrentMarketplace].Name + " | eBay</p>" +
                             "    <p><strong>THANK YOU FOR YOUR ORDER</strong></p>" + lrepeatedInfo + "</td>" +
                             "    <td align='right' valign='top' width='33%'><p>" + order.CreatedTime.ToShortDateString() + " - " + lx + "</p>" +
                             "    <p><h2>" + order.ShippingDetails.SellingManagerSalesRecordNumber + "</h2></p>" +
                             "    <p><font face='Free 3 of 9 Extended' size='+2'>*" + order.ShippingDetails.SellingManagerSalesRecordNumber + "*</font></p>" +
                             "    <p><h2>" + lshippingInfo + "</h2></p></td>" +
                             "  </tr>");
                    lf.Write("</table></center>");

                    // Now write the details of the order
                    lf.Write("<table width='800' border='1' cellspacing='0' cellpadding='2'><tr><td width='7%'><div align='center'><b>Qty</b></font></div></td><td width='50%'><div align='center'><font><b>Product</b></font></div></td><td width='15%'><div align='center'><font><b>Condition</b></font></div></td> <td width='15%'><div align='center'><font><b>Price</b></font></div></td><td width='10%'><div align='center'><font><b>Total</b></font></div></td></tr>");
                    //Decimal ltotalShipping = 0, ltotalDiscounts = 0;

                    // ltotalShipping = 0; ltotalDiscounts = 0;
                    double ltotalRefund = 0;
                    foreach (TransactionType orderItem in order.TransactionArray)
                    {
                        String ltitle = (orderItem.Variation != null && !String.IsNullOrEmpty(orderItem.Variation.SKU)) ? orderItem.Variation.VariationTitle : orderItem.Item.Title;
                        String[] lorderline = orderItem.OrderLineItemID.Split(new char[] { '-' });
                        String ltype = (lorderline.Length > 1 && lorderline[1].Length > 2) ? "" : "[AUCTION] ";

                        int lqty = orderItem.QuantityPurchased;
                        
                        if (orderItem.RefundArray != null && orderItem.RefundArray.Count > 0)
                        {
                            foreach (RefundType lrefu in orderItem.RefundArray)
                            {
                                ltotalRefund += lrefu.TotalRefundToBuyer.Value;
                            } // foreach
                            
                        }
                        
                        double ltotalprice = lqty * orderItem.TransactionPrice.Value;

                        // Let's write to the file
                        lf.Write("  <tr>" +
                                 "    <td><div align='center'>" + lqty + "</div></td>" +
                                 "    <td><div align='left'><b>" + ltype + ltitle + "</b><br />" +
                                 "        <strong>eBay Item: " + orderItem.Item.ItemID + " | SKU: " + orderItem.Item.SKU + "</strong></div></td>" +
                                 "    <td align='center'><b>" + orderItem.Item.ConditionDisplayName + ": "  + orderItem.Item.ConditionDescription + "</b></td>" +
                                 "    <td><div align='right'>" + orderItem.TransactionPrice.Value.ToString("C") + "</div></td>" +
                                 "    <td><div align='right'>" + ltotalprice.ToString("C") + "</div></td>" +
                                 "  </tr>");
                    } // foreach orderitem

                    lf.Write("<tr><td><div align='center'></div></td><td><div align='left'></div></td><td><div align='left'></div></td>" +
                             "<td><div align='right'>Shipping:</div></td><td><div align='right'>" +
                             order.ShippingServiceSelected.ShippingServiceCost.Value.ToString("C") +
                             "</div></td></tr>");
                    
                    if ( ltotalRefund > 0 )
                       lf.Write("<tr><td><div align='center'></div></td><td><div align='left'></div></td>" +
                                "<td><div align='right'>Discounts:</div></td><td><div align='right'>$" +
                                ltotalRefund +
                                "</div></td></tr>");
                    
                    lf.Write("<tr><td><div align='center'></div></td><td><div align='left'></div></td><td><div align='left'></div></td>" +
                             "<td><div align='right'>Order total:</div></td><td><div align='right'>" +
                             order.Total.Value.ToString("C") +
                             "</div></td></tr></table>");

                    if ( !order.repeated ) lgrandShipping += order.ShippingServiceSelected.ShippingServiceCost.Value;
                    if (!order.repeated ) lgrandTotal += order.Total.Value;

                    // Now write the bottom of the invoice
                    lf.Write("<table width='100%' align='center' cellpadding='10' cellspacing='0' border='0'>" +
                             "<tr><td align='left' valign='top' width='50%'><p><strong>Exchanges:</strong> " +
                             "Looking to exchange for a different size? Contact us through eBay messages to  " +
                             "see if we can find the right pair for you.</p><p><strong>Returns:</strong> Our return " +
                             "policies are stated in our profile page at <strong>eBay.com</strong>. Please contact us through eBay messages <b>first</b> for a Return Authorization number. " +
                             "We accept returns within <strong>30 days</strong> after receiving the product. " +
                             "The items <strong>must not be worn</strong> and should be returned in their <b>original box</b> and  packaging. " +
                             "<u>Buyer is responsible for returning shipping costs</u>.</p>" +
                             "<p align='center'><b>PLEASE CONTACT OUR CUSTOMER SERVICE</b></p>" +
                             "<p align='center'><b>DEPARTMENT FOR RETURN INSTRUCTIONS</b></p>" +
                             "We guarantee a reply to your return request within 24hrs. If you do not receive anything, we recommend checking your spam/junk mail." +
                             "</td><td align='left' valign='top' width='50%'>" +
                             "<p>To better serve our customers in the future, please write down the reason for returning the item:</p>" +
                             "<p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p><strong>Once we receive the shoes we will refund your order as stated in the eBay page of the product. " +
                             "Inspection and processing usually take </strong><strong>3-5 business days</strong><strong>.</strong><br /><strong>" +
                             "We don't refund original  shipping charges.</strong></p></td></tr></table><p class='fineprint'>" +
                             "*You MUST INCLUDE THIS PAGE in your return; otherwise we will have trouble  processing your order. " +
                             "Contact us through eBay messages for any questions.</p><p align='center'><strong>VISIT OUR WEBSITE " +
                             marketPlaces[gCurrentMarketplace].Website + "  " + marketPlaces[gCurrentMarketplace].SpecialOffer + "</strong></p>");

                    lf.Write("</td></tr></table>");

                    if (lx % 2 == 0)
                        lf.Write("<p style='page-break-before: always;'>&nbsp;</p>");
                    else
                        lf.Write("<hr style='margin:2px;padding:0px;' />");

                    lf.Flush();
                }; // foreach

                // Let's print out the summary
                txtStatus.Text = loperationStatus + txtStatus.Text;
                lf.Write("<h1>eBay " + marketPlaces[gCurrentMarketplace].Name + " orders summary " + DateTime.Now.ToShortDateString() + "</h1><hr>" +
                         "<p>&nbsp;<b>TOTAL ORDERS       :" + ltotalOrders + "</b></p>" +
                         "<p>&nbsp;<b>TOTAL NEXT DAY     :" + lnextdayorders + "</b></p>" +
                         "<p>&nbsp;<b>TOTAL PO BOX       :" + ltotalPOBox + "</b></p>" +
                         "<p>&nbsp;<b>TOTAL INTERNATIONAL:" + linternationalOrders + "</b></p>" +
                         "<p>&nbsp;<b>TOTAL ITEMS        :" + ltotalItems + "</b></p>" +
                         "<p>&nbsp;<b>TOTAL SHIPPING     :" + lgrandShipping.ToString("C") + "</b></p>" +
                         "<p>&nbsp;<b>GRAND TOTAL        :" + lgrandTotal.ToString("C") + "</b></p>" +
                         "<p>&nbsp;</p><p>&nbsp;</p><p>DETAILS</p><ul>");


                foreach (Brand lb in lbrands.OrderBy(p => p.brand))
                {
                    lf.Write("<li>" + lb.brand + " (" + lb.count + ") " + string.Format("{0:C}", lb.subtotal) + "</li>");
                }; // foreach

                lf.Write("</ul></div></body></html>");
                lfl.Write("</body></html>");

                lf.Flush();
                lfl.Flush();
                lf.Close();
                lfl.Close();
            } // if (!this.lStopProcess)


            MessageBox.Show("THE PROCESS ENDED WITH " + ltotalItems + " IN " + ltotalOrders + " ORDERS");

        } // getTheOrders

        private void getTheOrdersW()
        {
            SqlCommand lc = null;
            SqlDataReader lr = null;
            SqlConnection lconn = null;
            int ltotalOrders = 0;
            int ltotalPOBox = 0;
            int ltotalItems = 0;
            double lgrandTotal = 0,
                  lgrandShipping = 0;
            String loperationStatus = "";

            string POBoxPattern = @"(?i)\b(?:p(?:ost)?\.?\s*[o0](?:ffice)?\.?\s*b(?:[o0]x)?|b[o0]x)";

            List<Brand> lbrands = new List<Brand>();

            int lpage = 0;

            lgrandTotal = 0;
            lgrandShipping = 0;


            using (berkeleyEntities dataContext = new berkeleyEntities())
            {
                

                foreach (eBayOrder orderDto in GetUnshippedOrders())
                {
                    bsi_orders order = dataContext.bsi_orders.SingleOrDefault(p => 
                            p.marketplaceId == marketPlaces[gCurrentMarketplace].maskId && 
                            p.orderId.Equals(orderDto.ShippingDetails.SellingManagerSalesRecordNumber));

                    if (order == null)
                    {
                        orderDto.repeated = false;

                        order = new bsi_orders();
                        order.marketplaceId = marketPlaces[gCurrentMarketplace].maskId;
                        order.orderId = orderDto.ShippingDetails.SellingManagerSalesRecordNumber.ToString();
                        order.printDate = DateTime.Now;

                        dataContext.bsi_orders.AddObject(order);
                        dataContext.SaveChanges();

                        foreach (TransactionType orderItemDto in orderDto.TransactionArray)
                        {
                            Item item = null;
                            
                            if (orderItemDto.Variation != null && !String.IsNullOrEmpty(orderItemDto.Variation.SKU))
                            {
                                item = dataContext.Items.Single(p => p.ItemLookupCode.Equals(orderItemDto.Variation.SKU));
                            }
                            else
                            {
                                item = dataContext.Items.Single(p => p.ItemLookupCode.Equals(orderItemDto.Item.SKU));
                            }

                            bsi_orders_details orderItem = new bsi_orders_details();
                            orderItem.marketplaceId = 

                            if (!lrepeatedOrder) ltotalItems += orderItemDto.QuantityPurchased;
                            lloc = "";

                            lsku = (!string.IsNullOrEmpty(lskuX)) ? getNormalizedSKU(lskuX.Trim()) : "";
                            lsku = (!string.IsNullOrEmpty(lskuX)) ? lskuX.Trim() : "";

                            lc = new SqlCommand("SELECT * FROM Item where itemlookupcode='" + lsku + "'", lconn);
                            lr = lc.ExecuteReader();
                            if (lr.Read())
                            {
                                lloc = " [" + lr["binlocation"].ToString() + " | " + lr["notes"].ToString() + "]";
                                orderItemDto.Item.ConditionDescription = lr["extendedDescription"].ToString();
                            }; // if (lr.Read())
                            lr.Close();
                            lc.Cancel();

                            if (lBSIorderID != null)
                            {
                                int lqty = orderItemDto.QuantityPurchased;
                                double lprice = orderItemDto.TransactionPrice.Value;

                                lc = new SqlCommand("INSERT INTO bsi_orders_details (marketplace,marketplaceid,OrderInOurTables," +
                                                    "itemlookupcode,price,quantity,discount) " +
                                                    "VALUES (" + marketPlaces[gCurrentMarketplace].maskId +
                                                             ",'" + orderItemDto.Item.ItemID + "','" + lBSIorderID + "','" +
                                                             lsku + "','" + lprice + "','" + lqty + "','0')", lconn);
                                lc.ExecuteNonQuery();

                                // Now substract the sold quantity from the post
                                String lquantityId = null;
                                String lqs = "SELECT thePost.id,thePost.marketplace,thePost.markerplaceItemID," +
                                             "theQ.id AS QtyID, theQ.postId, theQ.itemLookupCode " +
                                             "FROM bsi_posts AS thePost INNER JOIN " +
                                             "bsi_quantities AS theQ ON thePost.id = theQ.postId " +
                                             "WHERE (thePost.markerplaceItemID = '" + orderItemDto.Item.ItemID + "') AND " +
                                             "(theQ.itemLookupCode = '" + lsku + "')";

                                lc = new SqlCommand(lqs, lconn);
                                lr = lc.ExecuteReader();
                                if (lr.Read())
                                {
                                    lquantityId = lr["QtyID"].ToString();
                                }
                                else
                                {
                                    loperationStatus = "\r\nITEM: " + lsku + " IS NOT REGISTERED IN THE TABLE QUANTITIES\r\n" + loperationStatus;
                                }
                                lr.Close();

                                // Deduct the quantity from bsi_quantities
                                if (lquantityId != null)
                                {
                                    lqs = "UPDATE bsi_quantities SET quantity=quantity-" + lqty.ToString() +
                                          " WHERE id=" + lquantityId;
                                    lc = new SqlCommand(lqs, lconn);
                                    lc.ExecuteNonQuery();
                                }
                            }

                            orderItemDto.Item.SKU = lskuX + lloc;

                            Brand lbrand4Search = lbrands.Find(delegate(Brand pb)
                            {
                                return (pb.brand.CompareTo(lbrand) == 0);
                            });

                            if (!lrepeatedOrder)
                            {
                                if (lbrand4Search != null)
                                {
                                    lbrand4Search.count += orderItemDto.QuantityPurchased;
                                }
                                else
                                {
                                    lbrand4Search = new Brand();
                                    lbrand4Search.brand = lbrand;
                                    lbrand4Search.count = orderItemDto.QuantityPurchased;
                                    lbrands.Add(lbrand4Search);
                                }
                            }
                        }; // foreach

                    }
                    else
                    {
                        orderDto.repeated = true;
                        orderDto.originalDate = order.printDate;
                    }
                } 
            }

            if (!this.lStopProcess)
            {
                this.txtStatus.Text = ltotalOrders + " orders found. ..." + "\r\n" + this.txtStatus.Text;
                this.txtStatus.Update();

                // Sort the orders
                this.txtStatus.Text = "Sorting orders by shipping method and then by 1st item title..." + "\r\n" + this.txtStatus.Text;
                this.txtStatus.Update();
                mainOrderList.Sort(compareOrders);

                // Assign locations
                String lbrand = "";
                this.txtStatus.Text = "Assigning location to each product item..." + "\r\n" + this.txtStatus.Text;
                this.txtStatus.Update();
                try
                {
                    string lcs = getConnectionString("berkeleyConnectionString");
                    lconn = new SqlConnection(lcs);
                    lconn.Open();

                    ltotalOrders = 0;
                    foreach (eBayOrder lorder in mainOrderList)
                    {

                        // Is this a repeated order?
                        bool lrepeatedOrder = false;
                        string lBSIorderID = null;
                        lc = new SqlCommand("SELECT * FROM bsi_orders where marketplaceId=" +
                                            marketPlaces[gCurrentMarketplace].maskId + " AND orderId='" +
                                            lorder.ShippingDetails.SellingManagerSalesRecordNumber + "' ", lconn);
                        lr = lc.ExecuteReader();
                        if (lr.Read())
                        {
                            lrepeatedOrder = true;
                            lorder.repeated = true;
                            lorder.originalDate = DateTime.Parse(lr["printDate"].ToString());
                            lr.Close();
                        }
                        else
                        {
                            lr.Close();
                            lrepeatedOrder = false;
                            lorder.repeated = false;
                            ltotalOrders++;

                            // Save this order
                            lc = new SqlCommand("INSERT INTO bsi_orders (marketplaceId, orderId, printDate) VALUES (" +
                                                marketPlaces[gCurrentMarketplace].maskId + ",'" +
                                                lorder.ShippingDetails.SellingManagerSalesRecordNumber +
                                                "','" + DateTime.Now + "'); SELECT SCOPE_IDENTITY() AS SCOPE_ID", lconn);
                            lBSIorderID = lc.ExecuteScalar().ToString();
                        } // if (lr.Read())
                        lc.Cancel();




                        foreach (TransactionType lt in lorder.TransactionArray)
                        {
                            String lskuX, lsku, lloc, conditionDescription;

                            // Chek if the items are variations or full items
                            if (lt.Variation != null && !String.IsNullOrEmpty(lt.Variation.SKU))
                            {
                                lskuX = lt.Variation.SKU;
                                lbrand = GetBrand(lskuX, lconn);
                                //lbrand = lt.Variation.VariationTitle.Substring(0, lt.Variation.VariationTitle.IndexOf(" ")).ToUpper();
                            }
                            else
                            {
                                lskuX = lt.Item.SKU;
                                lbrand = GetBrand(lskuX, lconn);
                                //lbrand = lt.Item.Title.Substring(0, lt.Item.Title.IndexOf(" ")).ToUpper();
                            }

                            if (!lrepeatedOrder) ltotalItems += lt.QuantityPurchased;
                            lloc = "";

                            lsku = (!string.IsNullOrEmpty(lskuX)) ? getNormalizedSKU(lskuX.Trim()) : "";
                            lsku = (!string.IsNullOrEmpty(lskuX)) ? lskuX.Trim() : "";

                            lc = new SqlCommand("SELECT * FROM Item where itemlookupcode='" + lsku + "'", lconn);
                            lr = lc.ExecuteReader();
                            if (lr.Read())
                            {
                                lloc = " [" + lr["binlocation"].ToString() + " | " + lr["notes"].ToString() + "]";
                                lt.Item.ConditionDescription = lr["extendedDescription"].ToString();
                            }; // if (lr.Read())
                            lr.Close();
                            lc.Cancel();

                            if (lBSIorderID != null)
                            {
                                int lqty = lt.QuantityPurchased;
                                double lprice = lt.TransactionPrice.Value;

                                lc = new SqlCommand("INSERT INTO bsi_orders_details (marketplace,marketplaceid,OrderInOurTables," +
                                                    "itemlookupcode,price,quantity,discount) " +
                                                    "VALUES (" + marketPlaces[gCurrentMarketplace].maskId +
                                                             ",'" + lt.Item.ItemID + "','" + lBSIorderID + "','" +
                                                             lsku + "','" + lprice + "','" + lqty + "','0')", lconn);
                                lc.ExecuteNonQuery();

                                // Now substract the sold quantity from the post
                                String lquantityId = null;
                                String lqs = "SELECT thePost.id,thePost.marketplace,thePost.markerplaceItemID," +
                                             "theQ.id AS QtyID, theQ.postId, theQ.itemLookupCode " +
                                             "FROM bsi_posts AS thePost INNER JOIN " +
                                             "bsi_quantities AS theQ ON thePost.id = theQ.postId " +
                                             "WHERE (thePost.markerplaceItemID = '" + lt.Item.ItemID + "') AND " +
                                             "(theQ.itemLookupCode = '" + lsku + "')";

                                lc = new SqlCommand(lqs, lconn);
                                lr = lc.ExecuteReader();
                                if (lr.Read())
                                {
                                    lquantityId = lr["QtyID"].ToString();
                                }
                                else
                                {
                                    loperationStatus = "\r\nITEM: " + lsku + " IS NOT REGISTERED IN THE TABLE QUANTITIES\r\n" + loperationStatus;
                                }
                                lr.Close();

                                // Deduct the quantity from bsi_quantities
                                if (lquantityId != null)
                                {
                                    lqs = "UPDATE bsi_quantities SET quantity=quantity-" + lqty.ToString() +
                                          " WHERE id=" + lquantityId;
                                    lc = new SqlCommand(lqs, lconn);
                                    lc.ExecuteNonQuery();
                                }
                            }

                            lt.Item.SKU = lskuX + lloc;

                            Brand lbrand4Search = lbrands.Find(delegate(Brand pb)
                            {
                                return (pb.brand.CompareTo(lbrand) == 0);
                            });

                            if (!lrepeatedOrder)
                            {
                                if (lbrand4Search != null)
                                {
                                    lbrand4Search.count += lt.QuantityPurchased;
                                }
                                else
                                {
                                    lbrand4Search = new Brand();
                                    lbrand4Search.brand = lbrand;
                                    lbrand4Search.count = lt.QuantityPurchased;
                                    lbrands.Add(lbrand4Search);
                                }
                            }
                        }; // foreach

                        if (!lrepeatedOrder)
                        {
                            Brand b = lbrands.Find(p => p.brand.Equals(lbrand));
                            b.subtotal += Convert.ToDecimal(lorder.Subtotal.Value);
                        }



                    } // foreach
                }
                catch (Exception pe)
                {
                    MessageBox.Show("SEVERE ERROR: " + pe.ToString(), "Error while processing orders");
                }
                finally
                {
                    if (lconn != null) lconn.Close();
                    if (lr != null) lr.Close();
                    if (lc != null) lc.Cancel();
                }

                // Create the HTML file
                this.txtStatus.Text = "Generating HTML orders file... \r\n" + this.txtStatus.Text;

                StreamWriter lf = File.CreateText(String.Format(OUTPUT_ORDERS_FILE, this.outputDirectory, marketPlaces[gCurrentMarketplace].Name)),
                             lfl = File.CreateText(String.Format(OUTPUT_LABELS_FILE, this.outputDirectory, marketPlaces[gCurrentMarketplace].Name));

                lf.AutoFlush = true; lfl.AutoFlush = true;
                lf.Write("<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>" +
                    "<html xmlns='http://www.w3.org/1999/xhtml'><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8' /><title>" +
                    "Untitled Document</title><style>body {font-family:Tahoma, Geneva, sans-serif;font-size:10px;} p{margin:0px;margin-bottom:5px;} .customerName {font-family:Arial,Helvetica, sans-serif;font-size:large;font-weight:bold;} .titulos {font-family:Arial," +
                    "Helvetica, sans-serif;font-size:large;font-weight:bold;" +
                    ((gCurrentMarketplace % 2 == 0) ? "border-style:solid; border-width:1px;" : " ") +
                    "}.fineprint {font-family:Arial, Helvetica, sans-serif;font-size:x-small;" +
                    "font-weight: normal;text-align:center;} .repeated-order {border-width: 2px;border-color: black;border-style:solid;padding: 2px; margin-right:2px;font-family:Arial, Helvetica, sans-serif;font-size:15px;float:right;} .day-of-week {padding: 1px;background-color: black;color: white;}</style></head><body><div style='width: 800px'>");

                lfl.WriteLine("<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>" +
                              "<html xmlns='http://www.w3.org/1999/xhtml'><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8' />" +
                              "<style>.shipToAddress {padding-left: 30px;}</style></head>" +
                              "<body style='font-family:Arial, Helvetica, sans-serif; font-size:13px;'>");


                int lx = 0, lnextdayorders = 0, linternationalOrders = 0;

                foreach (eBayOrder order in mainOrderList)
                {
                    String lshippingInfo = "", lrepeatedInfo = "";

                    this.txtStatus.Text = "Processing " + order.OrderID + "\r\n " + this.txtStatus.Text;

                    if (order.repeated)
                    {
                        lrepeatedInfo = "<p class='repeated-order'><span class='day-of-week'>" +
                                        order.originalDate.DayOfWeek.ToString().Substring(0, 3).ToUpper() +
                                        "</span>&nbsp;" + order.originalDate.ToShortDateString() + "&nbsp;</p>";
                    }
                    else
                    {
                        lrepeatedInfo = "";
                    }

                    lx++;

                    // Code added on 2013-11-06.
                    // Check: http://developer.ebay.com/DevZone/XML/docs/Reference/eBay/types/ShippingServiceOptionsType.html#ExpeditedService
                    if (order.ShippingServiceSelected.ExpeditedService || order.ShippingServiceSelected.ShippingService.Contains("Overnight"))
                        if (!order.repeated)
                        {
                            lnextdayorders++;
                        }

                    if (order.shippingPriority == FIRST_PRIORITY)
                    {
                        if (order.ShippingServiceSelected.ShippingService.Contains("UPSNextDayAir"))
                        {
                            lshippingInfo = "NEXT DAY | UPS";
                            // if (!lao.repeated) lnextdayorders++;
                        }
                        else
                        {
                            if (order.ShippingAddress.CountryName.ToString().Contains("United States"))
                                lshippingInfo = " POSTAL ";
                            else
                            {
                                lshippingInfo = order.ShippingServiceSelected.ShippingService + " | POSTAL ";

                                /*
                                lfl.WriteLine("<table cellpadding='0' cellspacing='0' style='width: 3.8in; height:3in;'><tr><td style='width:1.5in; height:1.5in;' align='left' valign='top'>" +
                                              "<B>SHOE NATION</B><br/>PO Box 189<br/>Salem, NH 03079<br/>USA</td><td>&nbsp;</td></tr><tr><td>&nbsp;</td><td  align='left' valign='top'>" +
                                              "<p><B>" + lao.ShippingAddress.Name + "</B><br/>" +
                                              lao.ShippingAddress.CompanyName + "<br />" +
                                              lao.ShippingAddress.Street + "<br />" +
                                              lao.ShippingAddress.Street1 + "<br/>" + 
                                              lao.ShippingAddress.Street2 + "<br />" +
                                              lao.ShippingAddress.CityName + ", " + lao.ShippingAddress.StateOrProvince + " " + lao.ShippingAddress.PostalCode + "<br />" +
                                              lao.ShippingAddress.CountryName + "</p>" +
                                              "</td></tr></table>"
                                             );
                                */

                                lfl.WriteLine("");
                                lfl.WriteLine("");
                                lfl.WriteLine("<p><b>SHOE NATION/" + marketPlaces[gCurrentMarketplace].Name + "</b><br>");
                                lfl.WriteLine("PO Box 189<br>");
                                lfl.WriteLine("Salem, NH 03079<br>");
                                lfl.WriteLine("USA</p>");
                                lfl.WriteLine("<p>&nbsp;</p>");
                                lfl.WriteLine("<p>&nbsp;</p>");
                                lfl.WriteLine("<p class='shipToAddress'><b>" + order.ShippingAddress.Name + "</b><br>");
                                lfl.WriteLine(order.ShippingAddress.CompanyName + "<br>");
                                lfl.WriteLine(order.ShippingAddress.Street + "<br>");
                                lfl.WriteLine(order.ShippingAddress.Street1 + "<br>");
                                lfl.WriteLine(order.ShippingAddress.Street2 + "<br>");
                                lfl.WriteLine(order.ShippingAddress.CityName + ", " + order.ShippingAddress.StateOrProvince + " " + order.ShippingAddress.PostalCode + "<br>");
                                lfl.WriteLine(order.ShippingAddress.CountryName + "</p>");
                                lfl.WriteLine("");
                                lfl.WriteLine("<p style='page-break-before: always;'>&nbsp;</p>"); // Page break

                                if (!order.repeated) linternationalOrders++;

                                /*
                                if (linternationalOrders % 2 == 0)
                                    lfl.WriteLine("\f"); // Page break
                                else
                                    lfl.WriteLine("---------------");

                                if (linternationalOrders % 2 == 0)
                                    lfl.WriteLine("<p style='page-break-before: always;'>&nbsp;</p>"); // Page break
                                else
                                    lfl.WriteLine("<hr/>");
                                */
                            };
                        }
                    }
                    else
                    {
                        lshippingInfo = order.ShippingServiceSelected.ShippingService + " | UPS";
                    };

                    lshippingInfo += " | Zone: " + order.zone;

                    lf.Write("<table border='0' bordercolor='#000000' cellpadding='1' cellspacing='0' width='100%'><tr><td style='height:5in'><center>");
                    lf.Write("<table width='800' border='0' cellspacing='0' cellpadding='0'><tr><td align='left' valign='top' width='33%'>");



                    if (!order.repeated)
                    {
                        string street = order.ShippingAddress.Street == null ? " " : order.ShippingAddress.Street;
                        string street1 = order.ShippingAddress.Street1 == null ? " " : order.ShippingAddress.Street1;

                        if (Regex.IsMatch(street, POBoxPattern) || Regex.IsMatch(street1, POBoxPattern))
                        {
                            ltotalPOBox++;
                        }
                    }

                    lf.Write("<tr>" +
                             "    <td align='left' valign='top' width='33%'><p class='customerName'>" + order.ShippingAddress.Name + "</p><p><strong>" +
                             order.BuyerUserID + "<br />" +
                             order.ShippingAddress.CompanyName + "<br />" +
                             order.ShippingAddress.Street + "<br />" +
                             order.ShippingAddress.Street1 + " | " + order.ShippingAddress.Street2 + "<br />" +
                             order.ShippingAddress.CityName + ", " + order.ShippingAddress.StateOrProvince + " " + order.ShippingAddress.PostalCode + "<br />" +
                             order.ShippingAddress.CountryName + "<br />" +
                             order.ShippingAddress.Phone + "</strong></p></td>" +
                             "    <td align='center' valign='top' width='34%'><p class='titulos'>" + marketPlaces[gCurrentMarketplace].Website + "</p>" +
                             "    <p>" + marketPlaces[gCurrentMarketplace].Name + " | eBay</p>" +
                             "    <p><strong>THANK YOU FOR YOUR ORDER</strong></p>" + lrepeatedInfo + "</td>" +
                             "    <td align='right' valign='top' width='33%'><p>" + order.CreatedTime.ToShortDateString() + " - " + lx + "</p>" +
                             "    <p><h2>" + order.ShippingDetails.SellingManagerSalesRecordNumber + "</h2></p>" +
                             "    <p><font face='Free 3 of 9 Extended' size='+2'>*" + order.ShippingDetails.SellingManagerSalesRecordNumber + "*</font></p>" +
                             "    <p><h2>" + lshippingInfo + "</h2></p></td>" +
                             "  </tr>");
                    lf.Write("</table></center>");

                    // Now write the details of the order
                    lf.Write("<table width='800' border='1' cellspacing='0' cellpadding='2'><tr><td width='7%'><div align='center'><b>Qty</b></font></div></td><td width='50%'><div align='center'><font><b>Product</b></font></div></td><td width='15%'><div align='center'><font><b>Condition</b></font></div></td> <td width='15%'><div align='center'><font><b>Price</b></font></div></td><td width='10%'><div align='center'><font><b>Total</b></font></div></td></tr>");
                    //Decimal ltotalShipping = 0, ltotalDiscounts = 0;

                    // ltotalShipping = 0; ltotalDiscounts = 0;
                    double ltotalRefund = 0;
                    foreach (TransactionType orderItem in order.TransactionArray)
                    {
                        String ltitle = (orderItem.Variation != null && !String.IsNullOrEmpty(orderItem.Variation.SKU)) ? orderItem.Variation.VariationTitle : orderItem.Item.Title;
                        String[] lorderline = orderItem.OrderLineItemID.Split(new char[] { '-' });
                        String ltype = (lorderline.Length > 1 && lorderline[1].Length > 2) ? "" : "[AUCTION] ";

                        int lqty = orderItem.QuantityPurchased;

                        if (orderItem.RefundArray != null && orderItem.RefundArray.Count > 0)
                        {
                            foreach (RefundType lrefu in orderItem.RefundArray)
                            {
                                ltotalRefund += lrefu.TotalRefundToBuyer.Value;
                            } // foreach

                        }

                        double ltotalprice = lqty * orderItem.TransactionPrice.Value;

                        // Let's write to the file
                        lf.Write("  <tr>" +
                                 "    <td><div align='center'>" + lqty + "</div></td>" +
                                 "    <td><div align='left'><b>" + ltype + ltitle + "</b><br />" +
                                 "        <strong>eBay Item: " + orderItem.Item.ItemID + " | SKU: " + orderItem.Item.SKU + "</strong></div></td>" +
                                 "    <td align='center'><b>" + orderItem.Item.ConditionDisplayName + ": " + orderItem.Item.ConditionDescription + "</b></td>" +
                                 "    <td><div align='right'>" + orderItem.TransactionPrice.Value.ToString("C") + "</div></td>" +
                                 "    <td><div align='right'>" + ltotalprice.ToString("C") + "</div></td>" +
                                 "  </tr>");
                    } // foreach orderitem

                    lf.Write("<tr><td><div align='center'></div></td><td><div align='left'></div></td><td><div align='left'></div></td>" +
                             "<td><div align='right'>Shipping:</div></td><td><div align='right'>" +
                             order.ShippingServiceSelected.ShippingServiceCost.Value.ToString("C") +
                             "</div></td></tr>");

                    if (ltotalRefund > 0)
                        lf.Write("<tr><td><div align='center'></div></td><td><div align='left'></div></td>" +
                                 "<td><div align='right'>Discounts:</div></td><td><div align='right'>$" +
                                 ltotalRefund +
                                 "</div></td></tr>");

                    lf.Write("<tr><td><div align='center'></div></td><td><div align='left'></div></td><td><div align='left'></div></td>" +
                             "<td><div align='right'>Order total:</div></td><td><div align='right'>" +
                             order.Total.Value.ToString("C") +
                             "</div></td></tr></table>");

                    if (!order.repeated) lgrandShipping += order.ShippingServiceSelected.ShippingServiceCost.Value;
                    if (!order.repeated) lgrandTotal += order.Total.Value;

                    // Now write the bottom of the invoice
                    lf.Write("<table width='100%' align='center' cellpadding='10' cellspacing='0' border='0'>" +
                             "<tr><td align='left' valign='top' width='50%'><p><strong>Exchanges:</strong> " +
                             "Looking to exchange for a different size? Contact us through eBay messages to  " +
                             "see if we can find the right pair for you.</p><p><strong>Returns:</strong> Our return " +
                             "policies are stated in our profile page at <strong>eBay.com</strong>. Please contact us through eBay messages <b>first</b> for a Return Authorization number. " +
                             "We accept returns within <strong>30 days</strong> after receiving the product. " +
                             "The items <strong>must not be worn</strong> and should be returned in their <b>original box</b> and  packaging. " +
                             "<u>Buyer is responsible for returning shipping costs</u>.</p>" +
                             "<p align='center'><b>PLEASE CONTACT OUR CUSTOMER SERVICE</b></p>" +
                             "<p align='center'><b>DEPARTMENT FOR RETURN INSTRUCTIONS</b></p>" +
                             "We guarantee a reply to your return request within 24hrs. If you do not receive anything, we recommend checking your spam/junk mail." +
                             "</td><td align='left' valign='top' width='50%'>" +
                             "<p>To better serve our customers in the future, please write down the reason for returning the item:</p>" +
                             "<p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p><strong>Once we receive the shoes we will refund your order as stated in the eBay page of the product. " +
                             "Inspection and processing usually take </strong><strong>3-5 business days</strong><strong>.</strong><br /><strong>" +
                             "We don't refund original  shipping charges.</strong></p></td></tr></table><p class='fineprint'>" +
                             "*You MUST INCLUDE THIS PAGE in your return; otherwise we will have trouble  processing your order. " +
                             "Contact us through eBay messages for any questions.</p><p align='center'><strong>VISIT OUR WEBSITE " +
                             marketPlaces[gCurrentMarketplace].Website + "  " + marketPlaces[gCurrentMarketplace].SpecialOffer + "</strong></p>");

                    lf.Write("</td></tr></table>");

                    if (lx % 2 == 0)
                        lf.Write("<p style='page-break-before: always;'>&nbsp;</p>");
                    else
                        lf.Write("<hr style='margin:2px;padding:0px;' />");

                    lf.Flush();
                }; // foreach

                // Let's print out the summary
                txtStatus.Text = loperationStatus + txtStatus.Text;
                lf.Write("<h1>eBay " + marketPlaces[gCurrentMarketplace].Name + " orders summary " + DateTime.Now.ToShortDateString() + "</h1><hr>" +
                         "<p>&nbsp;<b>TOTAL ORDERS       :" + ltotalOrders + "</b></p>" +
                         "<p>&nbsp;<b>TOTAL NEXT DAY     :" + lnextdayorders + "</b></p>" +
                         "<p>&nbsp;<b>TOTAL PO BOX       :" + ltotalPOBox + "</b></p>" +
                         "<p>&nbsp;<b>TOTAL INTERNATIONAL:" + linternationalOrders + "</b></p>" +
                         "<p>&nbsp;<b>TOTAL ITEMS        :" + ltotalItems + "</b></p>" +
                         "<p>&nbsp;<b>TOTAL SHIPPING     :" + lgrandShipping.ToString("C") + "</b></p>" +
                         "<p>&nbsp;<b>GRAND TOTAL        :" + lgrandTotal.ToString("C") + "</b></p>" +
                         "<p>&nbsp;</p><p>&nbsp;</p><p>DETAILS</p><ul>");


                foreach (Brand lb in lbrands.OrderBy(p => p.brand))
                {
                    lf.Write("<li>" + lb.brand + " (" + lb.count + ") " + string.Format("{0:C}", lb.subtotal) + "</li>");
                }; // foreach

                lf.Write("</ul></div></body></html>");
                lfl.Write("</body></html>");

                lf.Flush();
                lfl.Flush();
                lf.Close();
                lfl.Close();
            } // if (!this.lStopProcess)


            MessageBox.Show("THE PROCESS ENDED WITH " + ltotalItems + " IN " + ltotalOrders + " ORDERS");

        } // getTheOrders

        private List<eBayOrder> GetUnshippedOrders()
        {
            List<eBayOrder> unshippedOrders = new List<eBayOrder>();

            ApiContext apiContext = GetApiContext();

            GetOrdersCall call = new GetOrdersCall(apiContext);
            call.CreateTimeFrom = DateTime.Now.AddDays(-60);
            call.CreateTimeTo = DateTime.Now.AddMinutes(-15);
            call.DetailLevelList = new DetailLevelCodeTypeCollection();
            call.DetailLevelList.Add(DetailLevelCodeType.ReturnAll);

            call.Pagination = new PaginationType() { EntriesPerPage = 50, EntriesPerPageSpecified = true };

            int currentPage = 0;

            do
            {
                call.Pagination.PageNumber = ++currentPage;

                OrderTypeCollection ebayOrders = call.GetOrders(call.CreateTimeFrom, call.CreateTimeTo, TradingRoleCodeType.Seller, OrderStatusCodeType.Completed);

                foreach (OrderType ebayOrder in ebayOrders)
                {
                    // Skip shipped and payment-not-completed orders 
                    if (!ebayOrder.ShippedTimeSpecified && ebayOrder.CheckoutStatus.eBayPaymentStatus == PaymentStatusCodeType.NoPaymentFailure)
                    {
                        unshippedOrders.Add(extendAddress(ebayOrder));
                    };
                }; 
            } while (call.HasMoreOrders);

            return unshippedOrders;
        }

        private string GetBrand(string sku, SqlConnection con)
        {
            string brand = "";

            SqlCommand command = new SqlCommand("SELECT * FROM Item WHERE ItemLookupCode= '" + sku + "'", con);

            SqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                brand = reader["Subdescription1"].ToString();
            }

            reader.Close();

            return brand;
        }

        private string getNormalizedSKU(string psku)
        {
            int li = 0;
            string lsku = null, lwidth = "", lsize = "";
            StringBuilder lx = new StringBuilder();

            if (String.IsNullOrEmpty(psku)) return "";

            lx.Append(psku.Replace("--", ""));

            li = lx.Length - 1;

            if (Char.IsLetter(lx[li])) // if we've got a char as the last charactet then we have width
            {
                while (li > 0 && Char.IsLetter(lx[li]) && lx[li] != '-' && lx[li] != ' ')
                    lwidth = lx[li--] + lwidth;
            }

            // Check now the size
            if (lx[li] == '-' || lx[li] == ' ') --li;
            while (li > 0 && (Char.IsDigit(lx[li]) || lx[li] == '.') && lx[li] != '-' && lx[li] != ' ')
                lsize = lx[li--] + lsize;

            // Check if the key has at least one "-"
            if (lx.ToString().IndexOf('-') > 0)
                lsku = lx.ToString().Substring(0, lx.ToString().IndexOf('-')) + "-" + lsize + ((String.IsNullOrEmpty(lwidth)) ? "" : ("-" + lwidth));
            else
                lsku = lx.ToString();

            return lsku;
        } // getNormalizedSKU

        private void cmbMarketplaces_SelectedIndexChanged(object sender, EventArgs e)
        {
            gCurrentMarketplace = this.cmbMarketplaces.SelectedIndex;
        } // cmbMarketplaces_SelectedIndexChanged

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbMarketplaces.SelectedIndex = 0;
        } // Form1_Load

    } // Form1 

    class MarketPlace
    {
        public String Name { get; set; }
        public String Website { get; set; }
        public String Token { get; set; }
        public String SpecialOffer { get; set; }
        public int maskId { get; set; }

        public MarketPlace(String pname, String pweb, String pToken, String poffer, int pid)
        {
            Name = pname;
            Website = pweb;
            Token = pToken;
            SpecialOffer = poffer;
            maskId = pid;
        } // public MarketPlace
    } // class MarketPlace
} // BSI_eBayOrders
