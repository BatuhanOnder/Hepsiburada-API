using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace hbApi
{
  
    class Program
    {
        public class authorization
        {
            public string username { get; set; }
            public string password { get; set; }
            public string authenticationType { get; set; }
        }
        static void Main(string[] args)
        {
            // main code blocks
        }

        //HB kategori bilgilerini (kategoriye ait urunleri) dondurur
        //KategoriBilgileri(int, int): dynamic 
        public static dynamic KategoriBilgileri(int page, int size)
        {
            var client = new RestClient("https://mpop-sit.hepsiburada.com/product/api/categories/get-all-categories?leaf=true&status=ACTIVE&available=true&page=" + page + "&size="+size+"&version=1");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            string token = TokenAl();
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Cookie", "JSESSIONID=[your JSESSIONID]"); //Editable field
            IRestResponse response = client.Execute(request);
            // Console.WriteLine(response.Content);
            dynamic api = JObject.Parse(response.Content);
            return api.data;
        }

        //Hepsiburada api baglantisi icin gerekli dogrulama islemini yapar ve gerekli giris izni icin bir token uretir 
        //KategoriBilgileri():string
        public static string TokenAl()
        {
            var client = new RestClient("https://mpop-sit.hepsiburada.com/api/authenticate");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", "{\r\n    \"username\": \"[username]\",\r\n   \"password\": \"[password]\",\r\n   \"authenticationType\": \"INTEGRATOR\"\r\n}", ParameterType.RequestBody); // editable field
            IRestResponse response = client.Execute(request);
            var token = (response.Content.Replace("{\"id_token\":\"", "").Replace("\"}", ""));
            return token;
        }

        //Hepsiburada'da satis yapilan urunlerin listesini dondurur
        //ListingBigileriniCek():List<Listing>
        public static List<Listing> ListingBilgileriniCek(string merchantid)
        {
            var client = new RestClient("https://listing-external-sit.hepsiburada.com/listings/merchantid/" + merchantid);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Basic [Your username:password Base64 format]"); // editable field
            IRestResponse response = client.Execute(request);
            //Console.WriteLine(response.Content);

            string filtered_resp = response.Content;

            //Not required, you can parse direct
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(filtered_resp);
            doc.Save("SSML.xml");
            XDocument xDocument = XDocument.Load(@"C:\Users\[Username]\source\repos\hbApi\hbApi\bin\Debug\SSML.xml");// ur XML path
            List<Listing> ListelenenUrunler = (from e in xDocument.Root.Elements("Listings").Elements("Listing")
                                     select new Listing
                                     {
                                         HepsiburadaSku = (string)e.Element("HepsiburadaSku"),
                                         UniqueIdentifier = (string)e.Element("UniqueIdentifier"),
                                         MerchantSku = (string)e.Element("MerchantSku"),
                                         Price = (string)e.Element("Price"),
                                         AvailableStock = (string)e.Element("AvailableStock"),
                                         DispatchTime = (string)e.Element("DispatchTime"),
                                         CargoCompany1 = (string)e.Element("CargoCompany1"),
                                         CargoCompany2 = (string)e.Element("CargoCompany2"),
                                         CargoCompany3 = (string)e.Element("CargoCompany3"),
                                         ShippingAddressLabel = (string)e.Element("ShippingAddressLabel"),
                                         ClaimAddressLabel = (string)e.Element("ClaimAddressLabel"),
                                         MaximumPurchasableQuantity = (string)e.Element("MaximumPurchasableQuantity"),
                                         MinimumPurchasableQuantity = (string)e.Element("MinimumPurchasableQuantity"),
                                         Pricings = (string)e.Element("Pricings"),
                                         IsSalable = (string)e.Element("IsSalable"),
                                         CustomizableProperties = (string)e.Element("CustomizableProperties"),
                                         DeactivationReasons = (string)e.Element("DeactivationReasons"),
                                         IsSuspended = (string)e.Element("IsSuspended"),
                                         IsLocked = (string)e.Element("IsLocked"),
                                         LockReasons = (string)e.Element("LockReasons"),
                                         IsFrozen = (string)e.Element("IsFrozen"),
                                         CommissionRate = (string)e.Element("CommissionRate"),
                                         BuyboxOrder = (string)e.Element("BuyboxOrder"),
                                         AvailableWarehouses = (string)e.Element("AvailableWarehouses"),
                                         IsFulfilledByHB = (string)e.Element("IsFulfilledByHB"),
                                     }).ToList();
            return ListelenenUrunler;
        }

        //Belirtilen urunu satisa acar
        //UrunSatisaAcma(string,string):boolean
        public static bool UrunSatisaAcma(string merchantid, string hepsiburadaSku) {
            var client = new RestClient("https://listing-external-sit.hepsiburada.com/listings/merchantid/"+merchantid+"/sku/"+hepsiburadaSku+"/activate");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/xml");
            request.AddHeader("Authorization", "Basic [Your username:password Base64 format]"); // editable field
            IRestResponse response = client.Execute(request);
            if (response.IsSuccessful)
            {
                return true;
            }
            return false;
        }

        //Belirtilen urunu satisa kapatir
        //UrunSatisaKapama(string,string):boolean
        public static bool UrunSatisaKapama(string merchantid, string hepsiburadaSku)
        {
            var client = new RestClient("https://listing-external-sit.hepsiburada.com/listings/merchantid/" + merchantid + "/sku/" + hepsiburadaSku + "/deactivate");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/xml");
            request.AddHeader("Authorization", "Basic [Your username:password Base64 format]"); // editable field
            IRestResponse response = client.Execute(request);
            if (response.IsSuccessful)
            {
                return true;
            }
            return false;
        }

        //Belirlenen urunun fiyat(Price), stok(AvailableStock), kargoya verilis suresi(DispatchTime),
        //kisinin maksimum alacagi stok miktari(MaximumPurchasableQuantity), birinci kargo adi (CargoCompany1),
        //ikinci kargo adi (CargoCompany1), ucuncu kargo adi (CargoCompany1)
        //urunGuncelle(string, string, string, string, string, string, string, string, string, string):void
        public static bool UrunGuncelle(string mercantid,string HepsiburadaSku, string MerchantSku, string Price, string AvailableStock, string DispatchTime,string MaximumPurchasableQuantity, string CargoCompany1, string CargoCompany2, string CargoCompany3)
        {
           string xml = 
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<listings xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
                "\r\n        <listing>" +
                "\r\n            <HepsiburadaSku>"+HepsiburadaSku+"</HepsiburadaSku>" +
                "\r\n            <MerchantSku>"+MerchantSku+"</MerchantSku>" +
                "\r\n            <ProductName>"+"Product Name"+"</ProductName>" +
                "\r\n            <Price>"+Price+"</Price>" +
                "\r\n            <AvailableStock>"+AvailableStock+"</AvailableStock>" +
                "\r\n            <DispatchTime>"+DispatchTime+"</DispatchTime>" +
                "\r\n            <MaximumPurchasableQuantity>"+MaximumPurchasableQuantity+"</MaximumPurchasableQuantity>" +
                "\r\n            <CargoCompany1>"+CargoCompany1+"</CargoCompany1>" +
                "\r\n            <CargoCompany2>"+ CargoCompany2+ "</CargoCompany2>" +
                "\r\n            <CargoCompany3>"+CargoCompany3+"</CargoCompany3>" +
                "\r\n       </listing>\r\n            " +
                "\r\n</listings>";

            if (UrunBilgileriGuncelleme(mercantid, xml))
            {
                return true;
            }
            else
            {
                return false;

            }
            
           // return xml;
        }

        //Urun guncelleme xml'ini api baglantisina yollar
        //UrunBilgileriGuncelleme(string,string):boolean
        public static bool UrunBilgileriGuncelleme(string merchantid, string xml)
        {
            
            var client = new RestClient("https://listing-external-sit.hepsiburada.com/listings/merchantid/"+merchantid+"/inventory-uploads");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/xml");
            request.AddHeader("Authorization", "Basic [Your username:password Base64 format]"); // editable field
            request.AddParameter("application/xml", xml , ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            if (response.IsSuccessful)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(response.Content);
                XmlNodeList id = xmlDoc.GetElementsByTagName("Id");
                string inventoryuploadid = id[0].InnerXml;
                if (UrunGuncellemeKontrol( merchantid,  inventoryuploadid))
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        //Urun guncelleme bilgilerini yolladiktan sonra gelen upload id yi kontrol eder ve servera onay mesaji yollar. Bilgilerin dogrululuk kontrolunu yapar
        //UrunGuncellemeKontrol(string,string): bool
        public static bool UrunGuncellemeKontrol(string merchantid, string inventoryuploadid)
        {
            var client = new RestClient("https://listing-external-sit.hepsiburada.com/listings/merchantid/"+ merchantid + "/inventory-uploads/id/"+inventoryuploadid);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Basic [Your username:password Base64 format]"); // editable field
            IRestResponse response = client.Execute(request);
            if (response.IsSuccessful)
            {
                return true;
            }
            return false;
        }


    }

    //Veri sinifi listelenen urunleri depolamak icin bire veri yapisi gorevi ustlenir.
    class Listing
    {
        public string UniqueIdentifier { get; set; }
        public string HepsiburadaSku { get; set; }
        public string MerchantSku { get; set; }
        public string Price { get; set; }
        public string AvailableStock { get; set; }
        public string DispatchTime { get; set; }
        public string CargoCompany1 { get; set; }
        public string CargoCompany2 { get; set; }
        public string CargoCompany3 { get; set; }
        public string ShippingAddressLabel { get; set; }
        public string ClaimAddressLabel { get; set; }
        public string MaximumPurchasableQuantity { get; set; }
        public string MinimumPurchasableQuantity { get; set; }
        public string Pricings { get; set; }
        public string IsSalable { get; set; }
        public string CustomizableProperties { get; set; }
        public string DeactivationReasons { get; set; }
        public string IsSuspended { get; set; }
        public string IsLocked { get; set; }
        public string LockReasons { get; set; }
        public string IsFrozen { get; set; }
        public string CommissionRate { get; set; }
        public string BuyboxOrder { get; set; }
        public string AvailableWarehouses { get; set; }
        public string IsFulfilledByHB { get; set; }
    }
}
