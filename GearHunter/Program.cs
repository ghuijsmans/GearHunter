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
            SqlConnection conn = new SqlConnection("Data Source = DESKTOP-MOM6ULT; Initial Catalog = GearHunter; Integrated Security = True");
            conn.Open();

            SqlCommand command = new SqlCommand("SELECT Email FROM TargetEmail WHERE Id = 1", conn);

            string TargetEmail = (string)command.ExecuteScalar();
            Console.WriteLine("Resultaten worden verstuurd naar: " + TargetEmail);

            conn.Close();
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
                mail.Subject = "!! " + Keyword;
                mail.Body = "Er is mogelijk een of meer nieuwe advertentie(s) aangetroffen onder de zoekterm: " + Keyword + ".";
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

        private static void Sleepysleep()
        {
            Thread.Sleep(60000);
        }

        private static string SelectKeyWord(int CurrentKeyword)
        {
            SqlConnection conn = new SqlConnection("Data Source = DESKTOP-MOM6ULT; Initial Catalog = GearHunter; Integrated Security = True");
            conn.Open();

            SqlCommand command = new SqlCommand("SELECT Keyword FROM Keywords WHERE Id = '" + CurrentKeyword + "'", conn);
            
            string SearchWord = (string)command.ExecuteScalar();
            Console.WriteLine("Huidige Zoekterm: " + SearchWord);    

            conn.Close();
            return SearchWord;
        }
        
        private static List<string> SelectKeywords()
        {
            List<string> list_keywords = new List<string>();
            SqlConnection conn = new SqlConnection("Data Source = DESKTOP-MOM6ULT; Initial Catalog = GearHunter; Integrated Security = True");
            SqlCommand command = new SqlCommand("SELECT Keyword FROM Keywords", conn);
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
                string foutmelding = "foutmelding in de catch van de lijst ophalen.";
                List<string> melding = new List<string>();
                melding.Add(foutmelding);
                return melding;
                /* error */
            }
        } 
        private static int SelectKeyWordAmount()
        {
            SqlConnection conn = new SqlConnection("Data Source = DESKTOP-MOM6ULT; Initial Catalog = GearHunter; Integrated Security = True");
            conn.Open();

            SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Keywords", conn);
            int Count = (int)command.ExecuteScalar();
            Console.WriteLine("Aantal Zoektermen: " + Count);  

            conn.Close();
            return Count;
        }
            
        static void Main(string[] args)
        {
            int KeywordsAmount = SelectKeyWordAmount();
            bool Running = true;
            string TargetEmail = GetEmail();

            List<string> Keywords = new List<string>();
            Keywords = SelectKeywords();


            while (Running == true)
            {   
                foreach(string Keyword in Keywords)
                {
                    Console.WriteLine("Zoekt nu naar: " + Keyword);
                    IWebDriver driver = new ChromeDriver();
                    driver.Manage().Window.Maximize();

                    //open chrome web driver
                    driver.Navigate().GoToUrl("https://reverb.com");
                    DriverManager(driver);
                    IWebElement element = driver.FindElement(By.ClassName("site-search__controls__input"));

                    //Enter what you want to search for

                    element.SendKeys(Keyword);
                    element.SendKeys(Keys.Enter);
                    DriverManager(driver);

                    //Switching to react modal

                    driver.SwitchTo().ActiveElement();
                    IWebElement element1 = driver.FindElement(By.Id("user[first_name]"));
                    element1.SendKeys(Keys.Escape);

                    //Getting the Number of listings

                    try
                    {
                        string listno = driver.FindElement(By.XPath("/html/body/div[3]/section/div[2]/div/div/div/div[2]/div/div[2]/nav/div[1]/h2/div")).Text.ToString();
                        Console.WriteLine("Klaar met zoekwoord no#: " + Keyword);
                        Console.WriteLine("Aantal aangetroffen listings: " + listno.ToString());
                        driver.Close();
                        //listno.Replace(" Listings", "");
                        //int listnoint = Convert.ToInt32(listno);
                        //SendEmail(TargetEmail, Keyword);
                        //Console.WriteLine(listnoint);
                        Console.WriteLine("");
                    }
                    catch
                    {
                        string listno = driver.FindElement(By.XPath("/html/body/div[3]/section/div[2]/div/div/div/div[2]/div/div[2]/nav/section/div/h2")).Text.ToString();
                        Console.WriteLine("Klaar met zoekwoord no#: " + Keyword);
                        Console.WriteLine("Aantal aangetroffen listings: " + listno.ToString());
                        driver.Close();
                        //listno.Replace(" Listings", "");
                        //int listnoint = Convert.ToInt32(listno);
                        //SendEmail(TargetEmail, Keyword);
                        //Console.WriteLine(listnoint);
                        Console.WriteLine("");
                    }
                    
                    
                }
            }  
        }
    }
}



