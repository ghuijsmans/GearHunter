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

        private static void SendEmail(string TargetEmail, string Keyword)
        {
            try
            {
                var mail = new MailMessage();
                var smtpServer = new SmtpClient("smtp.gmail.com", 587);
                mail.From = new MailAddress("csharpemailer@gmail.com", "GearHunter");
                mail.To.Add(TargetEmail);
                mail.Subject = "Mogelijke nieuwe advertentie!";
                mail.Body = "Er is mogelijk een of meer nieuwe advertentie(s) aangetroffen onder de url: \n" + Keyword + ".";
                smtpServer.Credentials = new NetworkCredential("csharpemailer@gmail.com", "Watisdeze1213");
                smtpServer.EnableSsl = true;
                smtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Mail niet verstuurd: ");
                Console.WriteLine(ex);
            }
        }
        
        private static List<string> SelectKeywords()
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
                string foutmelding = "Foutmelding in de catch van de lijst ophalen.";
                List<string> melding = new List<string>();
                melding.Add(foutmelding);
                return melding;
            }
        } 

        private static int SelectKeyWordAmount()
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

        private static bool CompareListingsWithDatabase(string keyword, int listnoInt)
        {
            string conString = "Data Source = localhost; Initial Catalog = GearHunter; Integrated Security = True";
            string sqlQuery1 = "SELECT LastAmount FROM Keywords WHERE Keyword = '" +keyword+ "'";
            string sqlQuery2 = "UPDATE Keywords SET LastAmount = '" +listnoInt+ "' WHERE Keyword = '" +keyword+ "'";

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
                return true;
            }
            else if (AmountInDatabase > listnoInt)
            {
                conn.Open();
                cmd2.ExecuteNonQuery();
                conn.Close();
                return false;
            }
            else
            {
                return false;
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

        static void Main(string[] args)
        {
            int KeywordsAmount = SelectKeyWordAmount();
            bool Running = true;

            string TargetEmail = GetEmail();

            int intervalMinMainProgram = GetIntervalMinMain()*1000;
            int intervalMaxMainProgram = GetIntervalMaxMain()*1000;
            int intervalMinBetweenSearches = GetIntervalMinBetweenSearches()*1000;
            int intervalMaxBetweenSearches = GetIntervalMaxBetweenSearches()*1000;

            List<string> KeywordsUrl = new List<string>();
            KeywordsUrl = SelectKeywords();

            List<string> xPaden = new List<string>();
            xPaden.Add("/html/body/div[3]/section/div[2]/div/div/div/div[2]/div/div[2]/nav/div[1]/h2/div");
            xPaden.Add("/html/body/div[3]/section/div[2]/div/div/div/div[2]/div/div[2]/nav/section/div/h2");
            xPaden.Add("/html/body/div/section/div[2]/div/div/div/div[2]/div/div[2]/nav/section/div/h2");

            while (Running == true)
            {
                foreach(string Keyword in KeywordsUrl)
                {
                    Console.WriteLine("Zoekt nu naar: " + Keyword);
                    IWebDriver driver = new ChromeDriver();
                    driver.Manage().Window.Maximize();

                    //open chrome web driver
                    driver.Navigate().GoToUrl(Keyword);
                    DriverManager(driver);
                    //IWebElement element = driver.FindElement(By.ClassName("site-search__controls__input"));

                    //Switching to react modal
                    driver.SwitchTo().ActiveElement();
                    try
                    {
                        IWebElement element1 = driver.FindElement(By.Id("user[first_name]"));
                        element1.SendKeys(Keys.Escape);
                        Console.WriteLine("Popup closed.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    //Getting the Number of listings

                    // Ik had gehoopt dat dit zou werken, dus dat zodra je een hit hebt op regel 263 dat hij de foreach loop afbreekt omdat run true wordt.
                    // Echter blijft hij alle paden in xPaden aflopen totdat die ze allemaal gehad heeft, ookal heeft hij het eerste pad een hit. 
                    // Wellicht weet jij hier dan een betere oplossing voor, ik had gehoopt dat die whileloop 'overheerst' over die foreach zeg maar. 
                    // Ook hebben we die paden nu hardcoded erinstaan (regel 215), misschien iets om die ook maar 1x uit de database te halen. 
                    // Maar dat is mss ook iets voor later. 

                    string listno = "";
                    bool run = true;
                    while (run)
                    {
                        foreach(string pad in xPaden)
                        {
                            try
                            {
                                listno = driver.FindElement(By.XPath(pad)).Text.ToString();
                            }
                            catch
                            {
                                Console.WriteLine("Leeg");
                                listno = "Leeg";
                            }
                            bool checkListno = listno.Contains("listings");
                            if (checkListno)
                            {
                                run = false;
                            }
                        }
                    }

                    Console.WriteLine("Klaar met zoekwoord no#: " + Keyword);
                    Console.WriteLine("Aantal aangetroffen listings: " + listno.ToString());
                    driver.Close();

                    int listnoInt = TrimStringConvertToInt(listno);
                    //DEZE NOG AANPASSEN
                    bool sendMail = CompareListingsWithDatabase(Keyword, listnoInt);
                    if (sendMail)
                    {
                        SendEmail(TargetEmail, Keyword);
                    }

                    // hier sleep BetweenSearches
                    //Random r = new Random();
                    //int intervalBetweenSearches = r.Next(intervalMinBetweenSearches, intervalMaxBetweenSearches);
                    //Thread.Sleep(intervalBetweenSearches);
                    
                    Console.WriteLine("");
                }

                // hier sleep MainProgram
                //Random r2 = new Random();
                //int intervalMainProgram = r2.Next(intervalMinMainProgram, intervalMaxMainProgram);
                SendEmail("csharpemailer@gmail.com", "Still going strong!");
                //Thread.Sleep(intervalMainProgram);
            }
        }
    }
}



