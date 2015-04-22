//
// AMGD
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;

using System.Xml.Serialization;

using MarketplaceWebServiceOrders;
using MarketplaceWebServiceOrders.Mock;
using MarketplaceWebServiceOrders.Model;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections;


namespace BSI_AmazonShopUsLastOrders
{
    public partial class Form1 : Form
    {
            const int THROTTLING_WAIT_TIME = 7 * 1000; // Retry every 7 seconds
            const int FIRST_PRIORITY = 0; // First priority: USPS and Next days
            const int LOW_PRIORITY = 100; // UPS standar
            const int SALESREP_ID = 25; // RMS ID of the orders software

            /************************************************************************
             * Access Key ID and Secret Access Key ID
             * 1st one is for ShopUsLast
             * 2nd one if for HarvardStation
             ************************************************************************/


            String[] accessKeyId = { "AKIAIG7WFTMN2EQZUBKA",
                                     "AKIAJ4W3ALGZPVSGDYCA" };
            String[] secretAccessKey = { "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",
                                         "eESqVyUzMylEIGM2Iwc7+nljWSzhFBufF/X2WYwA"};
            String[] merchantId = { "A98JY3EWQYV6X", 
                                    "AOR65RSIHDNI7"};
            String[] marketplaceId = { "ATVPDKIKX0DER",
                                       "ATVPDKIKX0DER" };
            String[] outputOrdersFiles = { "AmazonShopUsLastOrder.htm",
                                           "AmazonHarvardStationOrder.htm" };
            String[] outputLabelsFiles = { "AmazonShopUsLabels.htm",
                                           "AmazonHarvardStationLabels.htm" };
            String[] outputFLFiles = { "AmazonShopUsFL.txt",
                                       "AmazonHarvardStationFL.txt" };

            String _accessKeyId = "",
                   _secretAccessKey = "",
                   _merchantId = "",
                   _marketplaceId = "",
                   _companyName = "",
                   _outputOrdersFile = "",
                   _outputLabelsFile = "",
                   _outputFLFile = "";
            int _RMSStoreCustomerId = -1;

            /************************************************************************
             * The application name and version are included in each MWS call's
             * HTTP User-Agent field.
             ***********************************************************************/
            const string applicationName = "ShopUsLast-Mecalzo";
            const string applicationVersion = "2011-11-01";

            String outputDirectory = @"C:\";
            MarketplaceWebServiceOrdersClient service;
            MarketplaceWebServiceOrdersConfig config = new MarketplaceWebServiceOrdersConfig();
            Boolean lStopProcess = false;

            SqlConnection gGlobalConn = null;

            List<AmazonOrder> mainOrderList;

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
        

        public Form1()
        {
            InitializeComponent();
        } // Form1

        private List<AmazonOrder> extendAddresses(List<Order> pl)
        // Determines the shipping zone and priority
        {
            List<AmazonOrder> pr = new List<AmazonOrder>();
            int lcount = 0;

                foreach (Order lo in pl)
                {
                    ++lcount;
                    try
                    {
                        String lzip = "", ltheZip = "";
                        String lzone = "NA";
                    int lpriority = LOW_PRIORITY;

                    if (lo.ShippingAddress != null)
                    {
                        ltheZip = lo.ShippingAddress.PostalCode;
                        lzone = "NA";
                        lpriority = LOW_PRIORITY;
                        if (lo.ShippingAddress.CountryCode.Equals("US"))
                        {
                            if (lo.ShipServiceLevel.Contains("Expedited"))
                            {
                                lpriority = FIRST_PRIORITY;
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
                        else
                        {
                            lpriority = FIRST_PRIORITY;
                        }
                    }

                    // Add the item
                    AmazonOrder lao = new AmazonOrder(lo);
                    lao.shippingPriority = lpriority;
                    lao.OrderZone = lzone;
                    pr.Add(lao);

                    }
                    catch (Exception pe)
                    {
                        MessageBox.Show(pe.ToString() + " : At order " + lo.AmazonOrderId);
                    }
                }; // foreach

            return pr;
        }

        private int compareOrders(AmazonOrder p1, AmazonOrder p2)
        // Gets 2 orders and sorts them based on the priority.
        // If they have the same priority then it sorts them by the title of the first item
        {
            int lret = 0; 
            SqlCommand lc = null;
            SqlDataReader lr = null;

            // weight is calculated like this: 
            // Native FL orders are the heaviest = -100
            // Returns FL orders are heavy = -50
            // Repeated orders are Medium = -10
            // After that they are weighted on their shipping priority and then in alphabeticall order
            AmazonOrder[] lorders = { p1, p2 };
            int[] lweights = new int[2];
            string lsku = "", lloc = "";

            for (int li = 0; li < 2; li++)
            {
                // Is this a FL order? Check item by item for their bin location
                lweights[li] = 0;
                foreach (OrderItem loi in lorders[li].orderItemList)
                {
                    try
                    {
                        lsku = getNormalizedSKU(loi.SellerSKU);

                        /* UNCOMMENT TO PRINT RETURN PRODUCTS WITH FL LOCATION
                        lc = new SqlCommand("SELECT * FROM item where itemlookupcode='" + lsku + "'", gGlobalConn);
                        lr = lc.ExecuteReader();
                        if (lr.Read())
                        {
                            //lloc = " | " + lr["notes"].ToString();
                            lloc = " | " + lr["binlocation"].ToString();
                        }; // if (lr.Read())
                        lr.Close();
                        lc.Cancel();
                        */

                        if (loi.SellerSKU.Contains("FL-"))
                        {
                            lweights[li] = -100;
                            lorders[li].IsFlorida = true;
                            break; // There's no need to check more items from this order
                        }
                        /* UNCOMMENT TO PRINT RETURN PRODUCTS WITH FL LOCATION
                        else
                        {
                            if (lloc.IndexOf("FL") >= 0)
                            {
                                lweights[li] = -50;
                                lorders[li].IsFlorida = true;
                                break; // There's no need to check more items from this order
                            }
                        }
                        */
                    }
                    catch (Exception pe)
                    {
                        MessageBox.Show("Error while verifying item: " + loi.SellerSKU + " in order: " + lorders[li].AmazonOrderId + "\r\n" + pe.ToString());
                    }
                } // for OrderItem
            } // for li

            lret = lweights[0] - lweights[1];
            if (lret == 0)
            {
                lret = p1.shippingPriority - p2.shippingPriority;
                if (lret == 0)
                {
                    if (p1.orderItemList.Count > 0 && p2.orderItemList.Count > 0)
                    {
                        lret = String.Compare(p1.orderItemList[0].Title,
                                              p2.orderItemList[0].Title);
                    }
                };
            }

            return lret;
        }

        private int compareOrderItems(OrderItem pi1, OrderItem pi2)
        {
            int lret = 0;

            if (!String.IsNullOrEmpty(pi1.Title) && !String.IsNullOrEmpty(pi2.Title))
                lret = String.Compare(pi1.Title, pi2.Title);
            return lret;
        }

        private void getTheOrders()
        {
            int li = 0, lpages = 0, lAllTheOrders = 0 ;
            bool lthrottling = false;
            Decimal lgrandShipping = 0, lgrandDiscounts = 0, lgrandTotal = 0;
            String lorderID = "", loperationStatus = "";
            string POBoxPattern = @"(?i)\b(?:p(?:ost)?\.?\s*[o0](?:ffice)?\.?\s*b(?:[o0]x)?|b[o0]x)";
            int ltotalPOBox = 0;
            
            try
            {
                lStopProcess = false;

                service = new MarketplaceWebServiceOrdersClient(applicationName, applicationVersion, _accessKeyId, _secretAccessKey, config);
                ListOrdersRequest request = new ListOrdersRequest();
                OrderStatusList lstatus = new OrderStatusList();
                lstatus.Status = new List<OrderStatusEnum>() { OrderStatusEnum.Unshipped, OrderStatusEnum.PartiallyShipped };
                request.OrderStatus = lstatus;
                
                /*
                request.MaxResultsPerPage = new MaxResults();
                request.MaxResultsPerPage.Value = 3;
                */

                // United States:
                config.ServiceURL = "https://mws.amazonservices.com/Orders/2011-01-01";

                request.MarketplaceId = new MarketplaceIdList();
                request.MarketplaceId.Id = new List<string>(new string[] { _marketplaceId });
                request.SellerId = _merchantId;

                // From 7 days ago, until 1 hour ago
                request.CreatedAfter = DateTime.Now.AddDays(-7); Console.WriteLine(request.CreatedAfter.ToString());
                request.CreatedBefore = DateTime.Now.AddHours(-1); Console.WriteLine(request.CreatedBefore.ToString());

                // Let's create the variables for the next token cycles
                ListOrdersByNextTokenResponse nextresponse = null;
                ListOrdersByNextTokenResult listOrdersByNextTokenResult = null;

                ListOrdersResponse response = service.ListOrders(request);

                if (response.IsSetListOrdersResult())
                {
                    ListOrdersResult listOrdersResult = response.ListOrdersResult;
                    if (listOrdersResult.IsSetOrders())
                    {
                        ListOrderItemsResponse ldresponse = null; // Response for the request for items in the order
                        OrderList orders = listOrdersResult.Orders;
                        mainOrderList = new List<AmazonOrder>();

                        List<AmazonOrder> orderList;
                        while (!lStopProcess)
                        {
                            lpages++;

                            orderList = this.extendAddresses(orders.Order);
                            foreach (AmazonOrder order in orderList)
                            {
                                Application.DoEvents();
                                if (lStopProcess) break;

                                DataGridViewRow lrow = new DataGridViewRow();
                                if (order.ShippingAddress != null)
                                {
                                    // Get all the details about this order
                                    ListOrderItemsRequest ldrequest = new ListOrderItemsRequest();
                                    // @TODO: set request parameters here
                                    ldrequest.SellerId = _merchantId;
                                    ldrequest.AmazonOrderId = order.AmazonOrderId;
                                    lthrottling = false;

                                    // Avoid throttling: from order 6 and up wait 6.5 second after each order (Actually is 6 secs but we'll play it safe)
                                    do
                                    {
                                        lthrottling = false;
                                        try
                                        {
                                            ldresponse = service.ListOrderItems(ldrequest);
                                        }
                                        catch (MarketplaceWebServiceOrdersException ordersErr)
                                        {
                                            // If the request is throttled, wait and try again.
                                            if (ordersErr.ErrorCode == "RequestThrottled")
                                            {
                                                this.txtStatus.Text = "Request is throttled on order " + li + "; waiting...\r\n" + this.txtStatus.Text ;
                                                this.txtStatus.Update();
                                                Application.DoEvents();
                                                lthrottling = true;
                                                System.Threading.Thread.Sleep(THROTTLING_WAIT_TIME);
                                            }
                                            else
                                            {
                                                // On any other error, re-throw the exception to be handled by the caller
                                                lStopProcess = true;
                                                throw;
                                            }
                                        }
                                    }
                                    while (lthrottling && !lStopProcess);


                                    if (ldresponse != null && ldresponse.IsSetListOrderItemsResult() && !lStopProcess)
                                    {
                                        ListOrderItemsResult listOrderItemsResult = ldresponse.ListOrderItemsResult;
                                        if (listOrderItemsResult.IsSetOrderItems())
                                        {
                                            OrderItemList orderItems = listOrderItemsResult.OrderItems;
                                            order.orderItemList = orderItems.OrderItem;
                                            order.orderItemList.Sort(compareOrderItems);

                                            foreach (OrderItem orderItem in order.orderItemList)
                                            {
                                                String lservice = order.ShipmentServiceLevelCategory + " ";

                                                int lrowno = this.dataGridView1.Rows.Add(order.AmazonOrderId,
                                                order.ShippingAddress.Name,
                                                order.ShippingAddress.AddressLine1,
                                                order.ShippingAddress.AddressLine2,
                                                order.ShippingAddress.City,
                                                order.ShippingAddress.StateOrRegion,
                                                order.ShippingAddress.PostalCode,
                                                order.ShippingAddress.CountryCode,
                                                orderItem.SellerSKU, orderItem.Title, orderItem.QuantityOrdered, order.OrderZone, lservice, "");

                                                this.dataGridView1.Rows[lrowno].HeaderCell.Value = li + 1;
                                            }; // foreach
                                        }
                                    }
                                    li++;
                                }

                            }; // foreach

                            mainOrderList.AddRange(orderList);

                            // Let's see if there are more orders in the next page
                            bool lnextorders = (lpages > 1) ? listOrdersByNextTokenResult.IsSetNextToken() : 
                                                              listOrdersResult.IsSetNextToken();
                            if (lnextorders)
                            {
                                // There's a second batch of orders
                                this.txtStatus.Text = "New batch of orders, page " + lpages + "\r\n" + this.txtStatus.Text;
                                ListOrdersByNextTokenRequest lrequest2 = new ListOrdersByNextTokenRequest();

                                lrequest2.SellerId = _merchantId;
                                lrequest2.NextToken = (lpages > 1) ? listOrdersByNextTokenResult.NextToken :
                                                                     listOrdersResult.NextToken;

                                nextresponse = service.ListOrdersByNextToken(lrequest2);
                                listOrdersByNextTokenResult = nextresponse.ListOrdersByNextTokenResult;
                                orders = listOrdersByNextTokenResult.Orders;
                            }
                            else lStopProcess = true;
                        }; // while there are pages

                    }; // if (listOrdersResult.IsSetOrders())
                }; // if (response.IsSetListOrdersResult())

                this.txtStatus.Text = "Generating HTML orders file..." + lpages + "\r\n" + this.txtStatus.Text;

                string lcs = getConnectionString("berkeleyConnectionString");
                if (lcs == null)
                {
                    MessageBox.Show("The connection string is wrong or empty. Correct the problem and try again.");
                    return;
                }
                else
                {
                    SqlCommand lc = null;
                    SqlDataReader lr = null;
                    SqlConnection lconn = null;

                    lconn = new SqlConnection(lcs);
                    lconn.Open();
                    gGlobalConn = lconn;
                    mainOrderList.Sort(compareOrders);

                    lgrandShipping = 0; lgrandTotal = 0;
                    try
                    {
                        int ltotalOrders = 0, ltotalNextday = 0, linternationalOrders = 0;
                        Decimal ltotalItems = 0;
                        List<Brand> lbrands = new List<Brand>();

                        StreamWriter lf = File.CreateText(this.outputDirectory + _outputOrdersFile),
                                    //lfl = File.CreateText(this.outputDirectory + _outputLabelsFile),
                                   lFLF = File.CreateText(this.outputDirectory + _outputFLFile);

                        lf.Write("<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>" +
                            "<html xmlns='http://www.w3.org/1999/xhtml'><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8' /><title>" +
                            "Untitled Document</title><style>body {font-family:Tahoma, Geneva, sans-serif;font-size:10px;} p{margin:0px;margin-bottom:5px;} .titulos {font-family:Arial," +
                            "Helvetica, sans-serif;font-size:large;font-weight:bold;}.fineprint {font-family:Arial, Helvetica, sans-serif;font-size:x-small;" +
                            "font-weight: normal;text-align:center;}.repeated-order {border-width: 2px;border-color: black;border-style:solid;padding: 2px; margin-right:2px;" + 
                            "font-family:Arial, Helvetica, sans-serif;font-size:15px;float:right;} .day-of-week {padding: 1px;background-color: black;color: white;}</style></head><body><div style='width: 800px'>");

                        //lfl.WriteLine("<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>" +
                        //      "<html xmlns='http://www.w3.org/1999/xhtml'><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8' />" +
                        //      "<style>.shipToAddress {padding-left: 30px;}</style></head>" +
                        //      "<body style='font-family:Arial, Helvetica, sans-serif; font-size:13px;'>");

                        ltotalOrders = 0;
                        ltotalItems = 0;
                        ltotalNextday = 0;
                        bool lisRepeated = false;
                        int lorderCounter = 0;

                        // Create the workorder
                        String lWorkOrderID = null;

                        /*
                        lc = new SqlCommand("INSERT INTO [order] "+ 
                                            "(StoreID,Closed,Type,Comment,CustomerID,Total,Taxable," + 
                                            "SalesRepID,ReferenceNumber,ShippingChargeOnOrder) " + 
                                            "VALUES (0,0,2,'" + _companyName + "'," + _RMSStoreCustomerId + 
                                            ",0,0,0,'" + (cmbAmazonStores.SelectedIndex + 1) + "-" +
                                                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + 
                                            "',0); SELECT SCOPE_IDENTITY() AS SCOPE_ID", lconn);
                        
                        lWorkOrderID = lc.ExecuteScalar().ToString();
                        */
                        foreach (AmazonOrder lao in mainOrderList)
                        {
                            String lshippingInfo = "", lrepeatedInfo = "", lBSIorderID = null; 
                            lorderID = lao.AmazonOrderId;

                            lBSIorderID = null;
                            lshippingInfo = "";
                            lrepeatedInfo = "";

                            // Is this a repeated order?
                            lc = new SqlCommand("SELECT * FROM bsi_orders where marketplaceId=1 AND orderId='" + lao.AmazonOrderId + "' ", lconn);
                            lr = lc.ExecuteReader();
                            if (lr.Read())
                            {
                                DateTime ldate = new DateTime();

                                lisRepeated = true;
                                ldate = DateTime.Parse(lr["printDate"].ToString());
                                lrepeatedInfo = "<p class='repeated-order'><span class='day-of-week'>" +
                                                ldate.DayOfWeek.ToString().Substring(0,3).ToUpper() + 
                                                "</span>&nbsp;" + ldate.ToShortDateString() + "&nbsp;</p>";
                                lr.Close();
                            }
                            else
                            {
                                lr.Close();
                                lisRepeated = false;
                                lrepeatedInfo = "";

                                // Save this order
                                lc = new SqlCommand("INSERT INTO bsi_orders (marketplaceId, orderId, printDate) VALUES (" +
                                                    (cmbAmazonStores.SelectedIndex+1) + ",'" + 
                                                    lao.AmazonOrderId + 
                                                    "','" +
                                                    DateTime.Now + "'); SELECT SCOPE_IDENTITY() AS SCOPE_ID", lconn);
                                lBSIorderID = lc.ExecuteScalar().ToString();

                                // Count only those non-repeated
                                ltotalOrders++;
                                ltotalItems += lao.NumberOfItemsUnshipped;
                            } // if (lr.Read())
                            //lc.Cancel();

                            if (!lisRepeated)
                            {
                                string line1 = !lao.ShippingAddress.IsSetAddressLine1() ? " " : lao.ShippingAddress.AddressLine1;
                                string line2 = !lao.ShippingAddress.IsSetAddressLine2() ? " " : lao.ShippingAddress.AddressLine2;
                                string line3 = !lao.ShippingAddress.IsSetAddressLine3() ? " " : lao.ShippingAddress.AddressLine3;

                                if (Regex.IsMatch(line1, POBoxPattern) || Regex.IsMatch(line2, POBoxPattern) || Regex.IsMatch(line3, POBoxPattern))
                                {
                                    ltotalPOBox++;
                                }
                            }

                            lorderCounter++;

                            this.txtStatus.Text = lao.AmazonOrderId + ", " + this.txtStatus.Text;

                            if (lao.ShippingAddress != null)
                            {
                                if (!lao.ShippingAddress.CountryCode.Equals("US"))
                                {
                                    //lfl.WriteLine("");
                                    //lfl.WriteLine("");
                                    //lfl.WriteLine("<p><b>World Shoe</b><br>");
                                    //lfl.WriteLine("135 Weston Road Suite 143<br>");
                                    //lfl.WriteLine("Weston, FL 33326<br>");
                                    //lfl.WriteLine("USA</p>");
                                    //lfl.WriteLine("<p>&nbsp;</p>");
                                    //lfl.WriteLine("<p>&nbsp;</p>");
                                    //lfl.WriteLine("<p class='shipToAddress'><b>" + lao.ShippingAddress.Name + "</b><br>");
                                    //lfl.WriteLine(lao.ShippingAddress.AddressLine1 + "<br>");
                                    //lfl.WriteLine(lao.ShippingAddress.AddressLine2 + "<br>");
                                    //lfl.WriteLine(lao.ShippingAddress.AddressLine3 + "<br>");
                                    //lfl.WriteLine(lao.ShippingAddress.City + ", " + lao.ShippingAddress.StateOrRegion + " " + lao.ShippingAddress.PostalCode + "<br>");
                                    //lfl.WriteLine(lao.ShippingAddress.CountryCode + "</p>");
                                    //lfl.WriteLine("");
                                    //lfl.WriteLine("<p style='page-break-before: always;'>&nbsp;</p>"); // Page break

                                    lshippingInfo = "International";
                                    if (!lisRepeated) linternationalOrders++;
                                }

                                if (lao.IsFlorida)
                                {
                                    lFLF.Write("\r\n\r\n----------------------------------------------------------------------------------\r\n");
                                    lFLF.Write("ORDER No: " + lao.AmazonOrderId);
                                }

                                if (lao.shippingPriority == FIRST_PRIORITY)
                                {
                                    if (lao.ShipServiceLevel.Contains("Expedited"))
                                    {
                                        if (!lisRepeated) ltotalNextday++;
                                        lshippingInfo += "NEXT DAY | UPS";
                                        if (lao.IsFlorida) lFLF.Write("\t --- NEXT DAY ---\r\n");
                                    }
                                    else
                                    {
                                        lshippingInfo += lao.ShipmentServiceLevelCategory + " | POSTAL ";
                                    }
                                }
                                else
                                {
                                    lshippingInfo += lao.ShipmentServiceLevelCategory + " | UPS";
                                };

                                if ( lshippingInfo.ToUpper().Contains("EXPEDITED") ||
                                     lshippingInfo.ToUpper().Contains("NEXT") )
                                    if (!lisRepeated) ltotalNextday++;

                                if (lao.IsFlorida)
                                {
                                    lFLF.Write(lao.ShippingAddress.Name + "\r\n");
                                    lFLF.Write(lao.ShippingAddress.AddressLine1 + "\r\n");
                                    lFLF.Write(lao.ShippingAddress.AddressLine2 + "\r\n");
                                    lFLF.Write(lao.ShippingAddress.AddressLine3 + "\r\n");
                                    lFLF.Write(lao.ShippingAddress.City + ", " + lao.ShippingAddress.StateOrRegion + " " + lao.ShippingAddress.PostalCode + "\r\n");
                                    lFLF.Write(lao.ShippingAddress.CountryCode + "\r\n");
                                    lFLF.Write(lao.ShippingAddress.CountryCode + "\r\n");
                                    lFLF.Write(lao.ShippingAddress.Phone + "\r\n");
                                    lFLF.Write("\r\nDETAILS\r\n");
                                }
                            }

                            lshippingInfo += " | Zone: " + lao.OrderZone;

                            lf.Write("<table border='0' bordercolor='#000000' cellpadding='1' cellspacing='0' width='100%'><tr><td style='height:5in'><center>");
                            lf.Write("<table width='800' border='0' cellspacing='0' cellpadding='0'><tr><td align='left' valign='top' width='33%'>");

                            if (lao.IsFlorida)
                            {
                                lrepeatedInfo += "&nbsp;<p class='repeated-order'>&nbsp;FL&nbsp;</p>";
                            }

                            if (lao.ShippingAddress != null)
                            {
                                lf.Write("<tr>" +
                                         "    <td align='left' valign='top' width='33%'><p class='titulos'>" + lao.ShippingAddress.Name + "</p><p><strong>" +
                                         lao.ShippingAddress.AddressLine1 + "<br />" +
                                         lao.ShippingAddress.AddressLine2 + "<br />" +
                                         lao.ShippingAddress.AddressLine3 + "<br />" +
                                         lao.ShippingAddress.City + ", " + lao.ShippingAddress.StateOrRegion + " " + lao.ShippingAddress.PostalCode + "<br />" +
                                         lao.ShippingAddress.CountryCode + "<br />" +
                                         lao.ShippingAddress.Phone + "</strong></p></td>" +
                                         "    <td align='center' valign='top' width='34%'><p class='titulos'><u>" + _companyName + "</u></p>" +
                                         "    <p><strong>THANK YOU FOR YOUR ORDER</strong></p>" + lrepeatedInfo + "</td>" +
                                         "    <td align='right' valign='top' width='33%'><p>" + lao.PurchaseDate.ToShortDateString() + " - " + ltotalOrders + "</p>" +
                                         "    <p><h2>" + lao.AmazonOrderId + "</h2></p>" +
                                         "    <p><font face='Free 3 of 9 Extended' size='+2'>*" + lao.AmazonOrderId + "*</font></p>" +
                                         "    <p><h2>" + lshippingInfo + "</h2></p></td>" +
                                         "  </tr>");
                            }

                            lf.Write("</table></center>");

                            // Now write the details of the order
                            lf.Write("<table width='800' border='1' cellspacing='0' cellpadding='2'><tr><td width='7%'><div align='center'><b>Qty</b></font></div></td><td width='58%'><div align='center'><font><b>Product</b></font></div></td><td width='21%'><div align='center'><font><b>Price</b></font></div></td><td width='14%'><div align='center'><font><b>Total</b></font></div></td></tr>");
                            Decimal ltotalShipping = 0, ltotalDiscounts = 0;

                            ltotalShipping = 0; ltotalDiscounts = 0;

                            string lbrand = "";

                            foreach (OrderItem loi in lao.orderItemList)
                            {
                                String lskuX = loi.SellerSKU;
                                String lsku,lloc;

                                if (!loi.IsSetItemPrice() || !loi.IsSetQuantityOrdered()) continue;

                                lloc = "";
                                /* if (lskuX.IndexOf("-nuco", StringComparison.OrdinalIgnoreCase) >= 0) <-- Original with "nuco"
                                if (lskuX.IndexOf("-", StringComparison.OrdinalIgnoreCase) >= 0) // <-- "nuco" no longer needed
                                {
                                    // lsku = lskuX.Substring(0, lskuX.IndexOf("-nuco", StringComparison.OrdinalIgnoreCase));
                                    lsku = lskuX.Substring(0, lskuX.IndexOf("-", StringComparison.OrdinalIgnoreCase));
                                }
                                else
                                    lsku = lskuX;
                                */


                                // lc = new SqlCommand("SELECT * FROM itemClass where itemlookupcode like '%" + lsku + "%'", lconn);
                                lsku = getNormalizedSKU(lskuX);
                                lc = new SqlCommand("SELECT * FROM item where itemlookupcode like '%" + lsku + "%'", lconn);
                                lr = lc.ExecuteReader();

                                String lItemCost = null;
                                String lItemId = null;
                                String lItemTitle = null;
                                if (lr.Read())
                                {
                                    //lloc = " | " + lr["notes"].ToString();
                                    lloc = " | " + lr["binlocation"].ToString();
                                    lItemCost = lr["Cost"].ToString();
                                    lItemId = lr["Id"].ToString();
                                    lItemTitle = ltotalOrders.ToString("00#") + "-" + lr["description"].ToString();
                                    if (lItemTitle.Length >= 30)
                                        lItemTitle = lItemTitle.Substring(0, 29);
                                }; // if (lr.Read())
                                lr.Close();
                                lc.Cancel();

                                ltotalShipping += Decimal.Parse(loi.ShippingPrice.Amount);
                                ltotalDiscounts += Decimal.Parse(loi.PromotionDiscount.Amount);

                                // Let's count the totals: per order and per brand
                                if (!lisRepeated)
                                {
                                    if (loi.IsSetPromotionDiscount()) lgrandDiscounts += Decimal.Parse(loi.PromotionDiscount.Amount);
                                    lbrand = GetBrand(loi.SellerSKU.Trim().ToUpper(), lconn);
                                    //String lbrand = loi.Title.Substring(0, loi.Title.IndexOf(" ")).ToUpper();
                                    Brand lbrand4Search = lbrands.Find(delegate(Brand pb)
                                    {
                                        return (pb.brand.CompareTo(lbrand) == 0);
                                    });

                                    if (lbrand4Search != null)
                                    {
                                        lbrand4Search.count += loi.QuantityOrdered;
                                    }
                                    else
                                    {
                                        lbrand4Search = new Brand();
                                        lbrand4Search.brand = lbrand;
                                        lbrand4Search.count = loi.QuantityOrdered;
                                        lbrands.Add(lbrand4Search);
                                    }

                                    if (lBSIorderID != null)
                                    {
                                        string lpriceS = loi.ItemPrice.Amount, 
                                               ldisc = loi.PromotionDiscount.Amount,
                                                 lqty = loi.QuantityOrdered.ToString(),
                                                 ldiscS = (loi.IsSetPromotionDiscount()) ? loi.PromotionDiscount.Amount : "0";

                                        // The loi.ItemPrice.Amount is the subtotal of all the items in this line (qty x price) so we need
                                        // to divide by qty to get the individual price
                                        Decimal lindividualPrice = new Decimal(); // We cannot multiply directly Money by Decimal
                                        if (loi.QuantityOrdered > 0) lindividualPrice = (Decimal.Parse(loi.ItemPrice.Amount)) / loi.QuantityOrdered;

                                        Decimal lthePrice = 0,
                                                ltheDisc = 0,
                                                ltheCorrectPrice = 0;

                                        Decimal.TryParse(lpriceS, out lthePrice);
                                        Decimal.TryParse(ldiscS, out ltheDisc);

                                        ltheCorrectPrice = lindividualPrice - ltheDisc;

                                        lc = new SqlCommand("INSERT INTO bsi_orders_details (marketplace,marketplaceid,OrderInOurTables," +
                                                            "itemlookupcode,price,quantity,discount) " +
                                                            "VALUES (" + (cmbAmazonStores.SelectedIndex+1) +
                                                                     ",'" + loi.OrderItemId + "','" + lBSIorderID + "','" +
                                                                     lsku + "','" + lindividualPrice.ToString() + "','" + lqty + "','" + 
                                                                     ldisc + "')", lconn);
                                        lc.ExecuteNonQuery();
                                        /*
                                        if (lWorkOrderID != null && lItemId != null)
                                        {
                                            lc = new SqlCommand("INSERT INTO orderEntry (Cost,OrderId,ItemId,FullPrice,PriceSource,Price," + 
                                                                "QuantityOnOrder,Description,LastUpdated,TransactionTime) VALUES (" +
                                                                lItemCost + "," + lWorkOrderID + "," + lItemId + ",10," +
                                                                lindividualPrice.ToString() + "," + ltheCorrectPrice.ToString() + 
                                                                "," + lqty + ",'" + lItemTitle +
                                                                "','" + DateTime.Now.ToString() + "','" + DateTime.Now.ToString() + "')", lconn);
                                            lc.ExecuteNonQuery();

                                            // Update the quantitycommited for the workorder
                                            lc = new SqlCommand("UPDATE item SET QuantityCommitted=QuantityCommitted+" +
                                                                lqty + " WHERE id=" + lItemId,lconn);
                                            lc.ExecuteNonQuery();
                                        }
                                         * */

                                        // Now substract the sold quantity from the post
                                        String lquantityId = null;
                                        String lqs = "SELECT thePost.id,thePost.marketplace,thePost.markerplaceItemID,thePost.status," +
                                                     "theQ.id AS QtyID, theQ.postId, theQ.itemLookupCode " +
                                                     "FROM bsi_posts AS thePost INNER JOIN " +
                                                     "bsi_quantities AS theQ ON thePost.id = theQ.postId " +
                                                     "WHERE (thePost.marketplace=" + (cmbAmazonStores.SelectedIndex + 1) + ") AND " +
                                                     "(thePost.status<>110) AND (theQ.itemLookupCode = '" + lsku + "')";

                                        lc = new SqlCommand(lqs, lconn);
                                        lr = lc.ExecuteReader();
                                        if (lr.Read())
                                        {
                                            lquantityId = lr["QtyID"].ToString();
                                        }
                                        else
                                        {
                                            loperationStatus = "Item: " + lsku + " IS NOT REGISTERED IN TABLE QUANTITIES\r\n" + loperationStatus;
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
                                }

                                Decimal laux = new Decimal(),lsubTotal; // We cannot multiply directly Money by Decimal

                                if ( loi.QuantityOrdered > 0 )
                                {
                                    decimal itemDiscount = Decimal.Parse(loi.PromotionDiscount.Amount);

                                    decimal discountedPrice =  Decimal.Parse(loi.ItemPrice.Amount) - itemDiscount;

                                    laux = discountedPrice / loi.QuantityOrdered;
                                }

                                lsubTotal = laux * loi.QuantityOrdered;

                                // Let's write to the file
                                lf.Write("  <tr>" +
                                         "    <td><div align='center'>" + loi.QuantityOrdered.ToString() + "</div></td>" +
                                         "    <td><div align='left'><b>" + loi.Title + "</b><br />" +
                                         "        <strong>SKU: " + loi.SellerSKU + lloc + "</strong></div></td>" +
                                         "    <td><div align='right'>$" + laux + "</div></td>" +
                                         "    <td><div align='right'>$" + lsubTotal + "</div></td>" +
                                         "  </tr>");
                                if (lao.IsFlorida)
                                {
                                    lFLF.Write("\t" + loi.QuantityOrdered.ToString() + 
                                               "\t" + loi.Title + "\r\n" +
                                               "\tSKU: " + loi.SellerSKU + "\r\n" +
                                               "\t$" + laux + "\r\n");
                                }
                            } // foreach orderitem

                            if (!lisRepeated && lao.IsSetOrderTotal())
                            {
                                Brand brand = lbrands.Single(p => p.brand.Equals(lbrand));
                                brand.subtotal += decimal.Parse(lao.OrderTotal.Amount);
                            }
                            

                            lf.Write("<tr><td><div align='center'></div></td><td><div align='left'></div></td>" +
                                     "<td><div align='right'>Shipping:</div></td><td><div align='right'>$" +
                                     ltotalShipping +
                                     "</div></td></tr>");

                            //lf.Write("<tr><td><div align='center'></div></td><td><div align='left'></div></td>" +
                            //         "<td><div align='right'>Discounts:</div></td><td><div align='right'>$" +
                            //         ltotalDiscounts +
                            //         "</div></td></tr>");

                            if (lao.OrderTotal != null)
                            {
                                lf.Write("<tr><td><div align='center'></div></td><td><div align='left'></div></td>" +
                                         "<td><div align='right'>Order total:</div></td><td><div align='right'>$" +
                                         lao.OrderTotal.ToString() +
                                         "</div></td></tr></table>");

                                if (!lisRepeated) lgrandShipping += ltotalShipping;
                                if (!lisRepeated) lgrandTotal += (Decimal.Parse(lao.OrderTotal.Amount));
                            }


                            // Now write the bottom of the invoice
                            lf.Write("<table width='100%' align='center' cellpadding='10' cellspacing='0' border='0'>" +
                                     "<tr><td align='left' valign='top' width='50%'><p><strong>Exchanges:</strong> " +
                                     "Looking to exchange for a different size? Contact us through Amazon messages to  " +
                                     "see if we can find the right pair for you.</p><p><strong>Returns:</strong> Our return " +
                                     "policies are stated in our profile page at <strong>Amazon.com</strong>. Please contact us through Amazon messages first. " +
                                     "We accept returns within <strong>30 days</strong> after receiving the product. " +
                                     "The items <strong>must not be worn</strong> and should be returned in their <b>original box</b> and  packaging. " +
                                     "<u>Buyer is responsible for returning shipping costs</u>.</p>" +
                                     "<br/><p align='center'><b>PLEASE CONTACT OUR CUSTOMER SERVICE</b></p>" +
                                     "<p align='center'><b>DEPARTMENT FOR RETURN INSTRUCTIONS</b></p>" +
                                     "We guarantee a reply to your return request within 24hrs. If you do not receive anything, we recommend checking your spam/junk mail." +
                                     "</td><td align='left' valign='top' width='50%'>" +
                                     "<p>To better serve our customers in the future, please write down the reason for returning the item:</p>" +
                                     "<p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p>&nbsp;</p><p><strong>Once we receive the shoes we will fully refund your  order. " +
                                     "Inspection and processing usually take </strong><strong>3-5 business days</strong><strong>.</strong><br /><strong>" +
                                     "We don't refund original  shipping charges.</strong></p></td></tr></table><p class='fineprint'>" +
                                     "*You MUST INCLUDE THIS PAGE in your return; otherwise we will have trouble  processing your order. " +
                                     "Contact us through Amazon messages for any questions.</p><!--p align='center'><strong>VISIT OUR WEBSITE " +
                                     "<a href='http://WWW.MECALZO.COM'>WWW.MECALZO.COM</a>  USE CODE &ldquo;RETCUST&rdquo;  " +
                                     "AT CHECKOUT AND GET $5.OO OFF THE TOTAL OF YOUR ORDER</strong></p-->");

                            lf.Write("</td></tr></table>");
                            
                            if ( lorderCounter % 2 == 0 )
                                lf.Write("<p style='page-break-before: always;'>&nbsp;</p>");
                            else
                                lf.Write("<hr style='margin:2px;padding:0px;' />");

                            lf.Flush();
                            //lfl.Flush();

                            //if (ltotalOrders > 0) break; 
                        }; // foreach

                        // Let's print out the summary
                        txtStatus.Text = loperationStatus + txtStatus.Text;
                        lf.Write("<h1>Amazon orders summary " + DateTime.Now.ToShortDateString() + "</h1><hr>" + 
                                 "<p>&nbsp;<b>TOTAL ORDERS       :" + ltotalOrders + "</b></p>" +
                                 "<p>&nbsp;<b>TOTAL NEXT DAY     :" + ltotalNextday + "</b></p>" +
                                 "<p>&nbsp;<b>TOTAL INTERNATIONAL:" + linternationalOrders + "</b></p>" +
                                 "<p>&nbsp;<b>TOTAL PO BOX:" + ltotalPOBox + "</b></p>" +
                                 "<p>&nbsp;<b>TOTAL ITEMS        :" + ltotalItems + "</b></p>" +
                                 "<p>&nbsp;<b>TOTAL SHIPPING     :" + lgrandShipping.ToString("C") + "</b></p>" +
                                 "<p>&nbsp;<b>GRAND TOTAL        :" + lgrandTotal.ToString("C") + "</b></p>" +
                                 "<p>&nbsp;</p><p>&nbsp;</p><p>DETAILS</p><ul>");

                        /* Let's save the total and shipping info of the orders in this round
                        if (lWorkOrderID != null)
                        {
                            lc = new SqlCommand("UPDATE [ORDER] SET total=" + lgrandTotal.ToString() +
                                                ",Deposit=" + lgrandTotal.ToString() + ",ShippingChargeOnOrder=" + lgrandShipping.ToString() +
                                                ",ShippingChargeOverride=1 WHERE id=" + lWorkOrderID, lconn);
                            lc.ExecuteNonQuery();
                        }*/

                        lAllTheOrders = (int)ltotalItems;

                        foreach (Brand lb in lbrands.OrderBy(p => p.brand))
                        {
                            lf.Write("<li> " + lb.brand + " (" + lb.count + ") " + string.Format("{0:C}", lb.subtotal) + " </li>");
                        }; // foreach

                        lf.Write("</ul></div></body></html>");
                        //lfl.Write("</ul></div></body></html>");

                        lf.Flush();
                        //lfl.Flush();
                        lFLF.Flush();

                        lf.Close();
                        //lfl.Close();
                        lFLF.Close();
                    }
                    catch (Exception pe)
                    {
                        MessageBox.Show("Order: " + lorderID + " Error: " + pe.ToString());
                    }
                    finally
                    {
                        if (lconn != null) lconn.Close();
                        if (lr != null) lr.Close();
                        if (lc != null) lc.Cancel();
                    }
                };


                this.btnStop.Enabled = false;
                this.lStopProcess = true;
                MessageBox.Show("Process ended with success! " + lAllTheOrders + " items");
            }
            catch (Exception pe)
            {
                MessageBox.Show("Error while connecting to the server" + pe.ToString());
            }
        } // getTheOrders

        private void testSKULookup()
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.dataGridView1.Rows.Clear();
            this.button1.Enabled = false;
            this.btnStop.Enabled = true;
            this.txtStatus.Text = "";

            int[] lstoresCustomerIds = { 1, 34 };

            _accessKeyId = accessKeyId[cmbAmazonStores.SelectedIndex];
            _secretAccessKey = secretAccessKey[cmbAmazonStores.SelectedIndex];
            _merchantId = merchantId[cmbAmazonStores.SelectedIndex];
            _marketplaceId = marketplaceId[cmbAmazonStores.SelectedIndex];
            _outputOrdersFile = outputOrdersFiles[cmbAmazonStores.SelectedIndex];
            _outputLabelsFile = outputLabelsFiles[cmbAmazonStores.SelectedIndex];
            _outputFLFile = outputFLFiles[cmbAmazonStores.SelectedIndex];
            _companyName = cmbAmazonStores.Text.Trim();
            _RMSStoreCustomerId = lstoresCustomerIds[cmbAmazonStores.SelectedIndex];

            getTheOrders();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            this.lStopProcess = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            using (SqlConnection lconn = new SqlConnection(Properties.Settings.Default.berkeleyConnectionString.ToString()))
            {
                try
                {
                    lconn.Open();
                    SqlCommand lc = new SqlCommand("select * from bsi_marketplaces where type=1 order by maskid", lconn);
                    SqlDataReader ldr = lc.ExecuteReader();
                    while (ldr.Read())
                        cmbAmazonStores.Items.Add(ldr["name"].ToString().Trim());
                    ldr.Close();
                    cmbAmazonStores.SelectedIndex = 0; // Select the first one from ebay
                }
                catch (Exception pe)
                {
                    MessageBox.Show("\nERROR WHILE READING MARKETPLACES: " + pe.ToString() + "\n", " Error on Load ");
                }
            } // using
        }

        private string getConnectionString(string pn)
        {
            string ls = null;

               
            foreach (ConnectionStringSettings lcs in ConfigurationManager.ConnectionStrings)
            {
                if ( lcs.Name.IndexOf(pn) >= 0 )
                {
                    ls = lcs.ConnectionString;
                    break;
                }
            } ; // foreach

            return ls;
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

        private void button2_Click(object sender, EventArgs e)
        {
            string lcs = getConnectionString("berkeleyConnectionString");
            if (lcs == null)
            {
                MessageBox.Show("The connection string is wrong or empty. Correct the problem and try again.");
                return;
            }
            else
            {
                try
                {
                    using (SqlConnection lconn = new SqlConnection(lcs))
                    {
                        lconn.Open();
                        SqlCommand lc = new SqlCommand("SELECT * FROM itemClass where itemlookupcode like '%" + this.txtSku.Text +"%'", lconn);
                        SqlDataReader lr = lc.ExecuteReader();
                        while (lr.Read())
                        {
                            this.txtStatus.Text += lr["itemlookupcode"].ToString() + " - " + lr["notes"].ToString() + "\r\n";
                        }; // while
                        lr.Close();
                        lc.Cancel();
                    };
                }
                catch (Exception pe)
                {
                    MessageBox.Show("Error: " + pe.ToString());
                };
            };
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                outputDirectory = folderBrowserDialog1.SelectedPath;
                
                if (!outputDirectory.EndsWith("\\"))
                    outputDirectory += "\\";

                this.lblOutput.Text = this.outputDirectory;
            };
        }

        private void lblOutput_Click(object sender, EventArgs e)
        {

        }

        private string getNormalizedSKU(string psku)
        {
            int li = 0;
            string lsku = null, lnuco = "", lwidth = "", lsize ="";
            StringBuilder lx = new StringBuilder();

            lx.Append(psku.Replace("--", ""));

            if (psku.Contains("nuco")) lnuco = "nuco";
            if (psku.Contains("nucco")) lnuco = "nucco";
            if (psku.Contains("nu-co")) lnuco = "nu-co";

            if (!String.IsNullOrEmpty(lnuco))
            {  
                // Remove the nuco and the price
                lx.Replace(lnuco, "");
                lx.Replace("--", "-");
            }

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

        private void button4_Click(object sender, EventArgs e)
        {
            txtStatus.Text = getNormalizedSKU(txtSku.Text);
        } // button4_Click

    } // Form1

    public class AmazonOrder : Order
    {
        String zone;
        public int shippingPriority;

        public List<OrderItem> orderItemList; // Not necessary because already exists: OrderItemList

        public String OrderZone
        {
            get { return zone; }
            set { zone = value; }
        }

        public bool IsFlorida { get; set; }

        public AmazonOrder()
        {
            this.orderItemList = new List<OrderItem>();
            this.IsFlorida = false;
        }

        public AmazonOrder(Order lo)
        {
            this.AmazonOrderId = lo.AmazonOrderId;
            this.SellerOrderId = lo.SellerOrderId;
            this.PurchaseDate = lo.PurchaseDate;
            this.LastUpdateDate = lo.LastUpdateDate;
            this.OrderStatus = lo.OrderStatus;
            this.FulfillmentChannel = lo.FulfillmentChannel;
            this.SalesChannel = lo.SalesChannel;
            this.OrderChannel = lo.OrderChannel;
            this.ShipServiceLevel = lo.ShipServiceLevel;
            this.ShippingAddress = lo.ShippingAddress;
            this.OrderTotal = lo.OrderTotal;
            this.NumberOfItemsShipped = lo.NumberOfItemsShipped;
            this.NumberOfItemsUnshipped = lo.NumberOfItemsUnshipped;
            this.MarketplaceId = lo.MarketplaceId;
            this.BuyerEmail = lo.BuyerEmail;
            this.BuyerName = lo.BuyerName;
            this.ShipmentServiceLevelCategory = lo.ShipmentServiceLevelCategory;
            this.orderItemList = new List<OrderItem>();
            this.IsFlorida = false;
        }

    } // AmazonOrder

    public class Brand : IComparer
    {
        public String brand = "";
        public Decimal count = 0;
        public Decimal subtotal = 0;

        public int Compare(object x, object y)
        {
            Brand brand1 = x as Brand;
            Brand brand2 = y as Brand;

            return brand1.brand.CompareTo(brand2.brand);
        }
    }

} // BSI_AmazonShopUsLastOrders
