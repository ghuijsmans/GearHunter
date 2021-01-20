// Copyright ocsidance.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support;
using System.IO;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;

namespace GearHunter
{
    class Program
    {
        private static void DriverManager(IWebDriver driver)
        {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);
        }

        private static string GetEmail()
        {
            SqlConnection conn = new SqlConnection("Data Source = localhost; Initial Catalog = GearHunter; Integrated Security = True");
            SqlCommand command = new SqlCommand("SELECT Email FROM TargetEmail WHERE Id = 1", conn);

            conn.Open();
            string TargetEmail = (string)command.ExecuteScalar();
            conn.Close();

            Console.WriteLine("Resultaten worden verstuurd naar: " + TargetEmail);

            return TargetEmail;
        }

        private static void SendEmail(string TargetEmail, string Keyword, string KeywordURL, string prijsAdvertentie, int aantalAdvertenties)
        {
            var mail = new MailMessage();
            var smtpServer = new SmtpClient("smtp.gmail.com", 587);
            mail.From = new MailAddress("csharpemailer@gmail.com", "GearHunter");
            mail.To.Add(TargetEmail);
            mail.Subject = "Mogelijke nieuwe advertentie: "+Keyword+". "+prijsAdvertentie;
            mail.Body = "Er is/zijn "+aantalAdvertenties+" nieuwe advertentie(s) aangetroffen onder de link: \n" + KeywordURL + ".";
            smtpServer.Credentials = new NetworkCredential("csharpemailer@gmail.com", "Watisdeze1213");
            smtpServer.EnableSsl = true;
            try
            {
                smtpServer.Send(mail);
            }
            catch 
            {
                Console.WriteLine("Mail niet verstuurd");
            }
        }
        
        private static List<string> SelectKeywordUrls()
        {
            List<string> list_keywords = new List<string>();
            SqlConnection conn = new SqlConnection("Data Source = localhost; Initial Catalog = GearHunter; Integrated Security = True");
            SqlCommand command = new SqlCommand("SELECT KeywordURL FROM Keywords", conn);
            try
            {
                conn.Open();
                SqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    int i = 0;
                    list_keywords.Add(dr.GetString(i));
                    i = i + 1;
                }
                conn.Close();
                return list_keywords;
            }
            catch
            {
                // Foutcode 4: Fout bij het inladen van de keywordsURL uit de database.
                SendErrorMail(4, "null");
                string foutmelding = "Foutmelding in de catch van de lijst keywordsURL ophalen.";
                List<string> melding = new List<string>();
                melding.Add(foutmelding);
                return melding;
            }
        }

        private static List<string> GetXpaden()
        {
            List<string> list_xPaden = new List<string>();
            SqlConnection conn = new SqlConnection("Data Source = localhost; Initial Catalog = GearHunter; Integrated Security = True");
            SqlCommand command = new SqlCommand("SELECT xPath FROM xPaths", conn);
            try
            {
                conn.Open();
                SqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    int i = 0;
                    list_xPaden.Add(dr.GetString(i));
                    i = i + 1;
                }
                conn.Close();
                return list_xPaden;
            }
            catch
            {
                // Foutcode 3: Fout bij het inladen van de xPaden uit de database.
                SendErrorMail(3, "null");
                string foutmelding = "Foutmelding in de catch van de lijst xPaden ophalen.";
                List<string> melding = new List<string>();
                melding.Add(foutmelding);
                return melding;
            }
        }

        private static int GetKeyWordAmount()
        {
            SqlConnection conn = new SqlConnection("Data Source = localhost; Initial Catalog = GearHunter; Integrated Security = True");
            conn.Open();

            SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Keywords", conn);
            int Count = (int)command.ExecuteScalar();
            Console.WriteLine("Aantal Zoektermen: " + Count);  

            conn.Close();
            return Count;
        }

        private static int TrimStringConvertToInt(string tekst)
        {
            string trimmedString = tekst.Trim(new char[] { ' ', 'l', 'i', 's', 't', 'n', 'g' });
            int listingInt = Int32.Parse(trimmedString);
            return listingInt;
        }

        private static int CompareListingsWithDatabase(string keyword, int listnoInt)
        {
            string conString = "Data Source = localhost; Initial Catalog = GearHunter; Integrated Security = True";
            string sqlQuery1 = "SELECT LastAmount FROM Keywords WHERE KeywordURL = '" +keyword+ "'";
            string sqlQuery2 = "UPDATE Keywords SET LastAmount = '" +listnoInt+ "' WHERE KeywordURL = '" +keyword+ "'";

            SqlConnection conn = new SqlConnection(conString);
            SqlCommand cmd1 = new SqlCommand(sqlQuery1, conn);
            SqlCommand cmd2 = new SqlCommand(sqlQuery2, conn);

            conn.Open();
            int AmountInDatabase = Convert.ToInt32(cmd1.ExecuteScalar());
            conn.Close();

            if (AmountInDatabase < listnoInt)
            {
                conn.Open();
                cmd2.ExecuteNonQuery();
                conn.Close();
                int verschil = listnoInt - AmountInDatabase;
                return verschil;
            }
            else if (AmountInDatabase > listnoInt)
            {
                conn.Open();
                cmd2.ExecuteNonQuery();
                conn.Close();
                return 0;
            }
            else
            {
                return 0;
            }
        }

        private static int GetIntervalMinMain()
        {
            string conString = "Data Source = localhost; Initial Catalog = GearHunter; Integrated Security = True";
            string sqlQuery = "SELECT IntervalMin FROM SetTimeInterval WHERE TimeIntervalFor = 'MainProgram'";

            SqlConnection conn = new SqlConnection(conString);
            SqlCommand cmd = new SqlCommand(sqlQuery, conn);
            
            conn.Open();
            int intervalMin = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();

            return intervalMin;
        }

        private static int GetIntervalMaxMain()
        {
            string conString = "Data Source = localhost; Initial Catalog = GearHunter; Integrated Security = True";
            string sqlQuery = "SELECT IntervalMax FROM SetTimeInterval WHERE TimeIntervalFor = 'MainProgram'";

            SqlConnection conn = new SqlConnection(conString);
            SqlCommand cmd = new SqlCommand(sqlQuery, conn);

            conn.Open();
            int intervalMax = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();

            return intervalMax;
        }

        private static int GetIntervalMinBetweenSearches()
        {
            string conString = "Data Source = localhost; Initial Catalog = GearHunter; Integrated Security = True";
            string sqlQuery = "SELECT IntervalMin FROM SetTimeInterval WHERE TimeIntervalFor = 'BetweenSearches'";

            SqlConnection conn = new SqlConnection(conString);
            SqlCommand cmd = new SqlCommand(sqlQuery, conn);

            conn.Open();
            int intervalMin = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();

            return intervalMin;
        }

        private static int GetIntervalMaxBetweenSearches()
        {
            string conString = "Data Source = localhost; Initial Catalog = GearHunter; Integrated Security = True";
            string sqlQuery = "SELECT IntervalMax FROM SetTimeInterval WHERE TimeIntervalFor = 'BetweenSearches'";

            SqlConnection conn = new SqlConnection(conString);
            SqlCommand cmd = new SqlCommand(sqlQuery, conn);

            conn.Open();
            int intervalMax = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();

            return intervalMax;
        }

        private static void SendErrorMail(int foutcode, string url)
        {
            string body1 = "Programma loopt nog.";
            string body2 = "Foutcode 2: xPad gebruikt door Reverb die wij niet afvangen." + url;
            string body3 = "Foutcode 3: Fout bij het ophalen van de xPaden in de database.";
            string body4 = "Foutcode 4: Fout bij het ophalen van de KeywordURL's in de database.";
            string body5 = "Foutcode 5: Fout bij het ophalen van de prijs van de advertentie.";
            string body = "";
            if (foutcode == 1)
            {
                body = body1;
            }
            else if (foutcode == 2)
            {
                body = body2;
            }
            else if (foutcode == 3)
            {
                body = body3;
            }
            else if (foutcode == 4)
            {
                body = body4;
            }
            else if (foutcode == 5)
            {
                body = body5;
            }
            else
            {
                body = "Geen afgevangen foutcode!";
            }

            var mail = new MailMessage();
            var smtpServer = new SmtpClient("smtp.gmail.com", 587);
            mail.From = new MailAddress("csharpemailer@gmail.com", "GearHunter");
            mail.To.Add("csharpemailer@gmail.com");
            mail.Subject = "Foutmelding GearHunter!";
            mail.Body = body;
            smtpServer.Credentials = new NetworkCredential("csharpemailer@gmail.com", "Watisdeze1213");
            smtpServer.EnableSsl = true;
            try
            {
                smtpServer.Send(mail);
            }
            catch 
            {
                Console.WriteLine("Send errormail is mislukt!");
            }
        }

        private static string knipVoorsteHelftStringEnOmdraaien(string keyWord)
        {
            string zoekwoord = keyWord.Substring(37);
            char[] array1 = zoekwoord.ToCharArray();
            Array.Reverse(array1);
            return new string(array1);
        }

        private static string knipAchtersteHeftStringEnOmdraaien(string keyWord)
        {
            string zoekwoordGetrimmed = keyWord.Substring(40);
            char[] array2 = zoekwoordGetrimmed.ToCharArray();
            Array.Reverse(array2);
            return new string(array2);
        }

        static void Main(string[] args)
        {
            GetKeyWordAmount();
            
            string TargetEmail = GetEmail();

            int intervalMinMainProgram = GetIntervalMinMain()*1000;
            int intervalMaxMainProgram = GetIntervalMaxMain()*1000;
            int intervalMinBetweenSearches = GetIntervalMinBetweenSearches()*1000;
            int intervalMaxBetweenSearches = GetIntervalMaxBetweenSearches()*1000;

            List<string> KeywordsUrl = new List<string>();
            KeywordsUrl = SelectKeywordUrls();

            List<string> xPaden = new List<string>();
            xPaden = GetXpaden();

            bool Running = true;
            while (Running)
            {
                foreach(string Keyword in KeywordsUrl)
                {
                    Console.WriteLine("Zoekt nu naar: " + Keyword);
                    IWebDriver driver = new ChromeDriver();
                    driver.Manage().Window.Maximize();

                    // Open chrome web driver
                    driver.Navigate().GoToUrl(Keyword);
                    DriverManager(driver);
                    //IWebElement element = driver.FindElement(By.ClassName("site-search__controls__input"));

                    // Switching to react modal
                    driver.SwitchTo().ActiveElement();
                    try
                    {
                        IWebElement element1 = driver.FindElement(By.Id("user[first_name]"));
                        element1.SendKeys(Keys.Escape);
                        Console.WriteLine("Popup closed.");
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Waarschijnlijk geen popup.");
                    }

                    // Listings aantal ophalen.

                    string listno = "";

                    int i = 0;
                    while (xPaden.Count >= i)
                    {
                        if (listno == "")
                        {
                            string pad = xPaden[i];
                            try
                            {   
                                listno = driver.FindElement(By.XPath(pad)).Text.ToString();
                            }
                            catch
                            {
                                Console.WriteLine("Pad niet gevonden, volgende proberen.");
                            }
                        }
                        i++;
                        bool checkListno = listno.Contains("listing");
                        bool checkListno2 = listno.Contains("listings");

                        if (checkListno == true || checkListno2 == true)
                        {
                            i = 100;

                            int listnoInt = TrimStringConvertToInt(listno);
                            int aantalNieuweAdvertenties = CompareListingsWithDatabase(Keyword, listnoInt);
                            
                            if (aantalNieuweAdvertenties > 0)
                            {
                                string prijsAdvertentie = "";
                                try
                                {
                                    prijsAdvertentie = driver.FindElement(By.XPath("/html/body/div[3]/section/div[2]/div/div/div/div[2]/div/div[2]/ul/li/div/a/div[2]/div/div/span")).Text.ToString();
                                    Console.WriteLine("Prijs advertentie = " + prijsAdvertentie);
                                }
                                catch
                                {
                                    SendErrorMail(5, "null");
                                }

                                string knipsel1 = knipVoorsteHelftStringEnOmdraaien(Keyword);
                                string knipsel2 = knipAchtersteHeftStringEnOmdraaien(knipsel1);
                                string finalZoekwoord = knipsel2;
                                finalZoekwoord = finalZoekwoord.Replace("%20", " ");
                                SendEmail(TargetEmail, finalZoekwoord, Keyword, prijsAdvertentie, aantalNieuweAdvertenties);
                            }
                        }
                    }

                    driver.Dispose();

                    if (i != 100)
                    {
                        //CW voor te testen!!!
                        //Console.WriteLine("Kijk voor xPad!@!@!@!@!");

                        //Foutcode 2: xPad gebruikt door Reverb die wij niet afvangen. 
                        SendErrorMail(2, Keyword);
                    }

                    //driver.Dispose();

                    Console.WriteLine("Klaar met zoekURL: " + Keyword);
                    Console.WriteLine("Resultaat: " + listno.ToString());

                    // Hier sleep BetweenSearches

                    Random r = new Random();
                    int intervalBetweenSearches = r.Next(intervalMinBetweenSearches, intervalMaxBetweenSearches);
                    Thread.Sleep(intervalBetweenSearches);

                    Console.WriteLine("");
                }

                // Foutcode 1: Programma loopt nog! 
                SendErrorMail(1, "null");

                // Hier sleep MainProgram
                Random r2 = new Random();
                int intervalMainProgram = r2.Next(intervalMinMainProgram, intervalMaxMainProgram);
                Thread.Sleep(intervalMainProgram);
            }
        }
    }
}



